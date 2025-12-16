using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class BasicSensorComponent : SensorComponent
    {
        public BasicController basicController;

        void Awake()
        {
            if (!basicController) basicController = GetComponentInParent<BasicController>();
        }

        public override ISensor[] CreateSensors()
        {
            return new ISensor[] { new BasicSensor(basicController) };
        }
    }

    public class BasicSensor : SensorBase
    {
        readonly BasicController c;

        public BasicSensor(BasicController controller) { c = controller; }

        public override ObservationSpec GetObservationSpec() => ObservationSpec.Vector(10);
        public override string GetName() => "BasicSensor";

        public override int Write(ObservationWriter writer)
        {
            // writer는 길이 10짜리
            // 0~2: needs
            writer[0] = (c && c.maxHealth > 0f) ? c.Health / c.maxHealth : 0f;
            writer[1] = (c && c.maxHunger > 0f) ? c.Hunger / c.maxHunger : 0f;
            writer[2] = c ? Mathf.Clamp01(c.Danger) : 0f;

            Vector3 pos = c ? c.transform.position : Vector3.zero;

            // env bounds + 정규화 위치(0~1)
            float minX = 0f, maxX = 1f, minZ = 0f, maxZ = 1f;
            float nx = 1f, nz = 1f;

            if (c && c.env)
            {
                minX = c.env.minXZ.x;
                maxX = c.env.maxXZ.x;
                minZ = c.env.minXZ.y;
                maxZ = c.env.maxXZ.y;

                nx = Mathf.Max(0.0001f, (maxX - minX) * 0.5f);
                nz = Mathf.Max(0.0001f, (maxZ - minZ) * 0.5f);
            }

            writer[3] = (maxX > minX) ? Mathf.InverseLerp(minX, maxX, pos.x) : 0.5f;
            writer[4] = (maxZ > minZ) ? Mathf.InverseLerp(minZ, maxZ, pos.z) : 0.5f;

            Transform foodTf   = GetNearestActive((c && c.env) ? c.env.Foods   : null, pos);
            Transform hazardTf = GetNearestActive((c && c.env) ? c.env.Hazards : null, pos);

            Vector3 fWorld = foodTf ? (foodTf.position - pos) : Vector3.zero;
            Vector3 hWorld = hazardTf ? (hazardTf.position - pos) : Vector3.zero;

            Vector3 fLocal = (c ? c.transform.InverseTransformDirection(fWorld) : Vector3.zero);
            Vector3 hLocal = (c ? c.transform.InverseTransformDirection(hWorld) : Vector3.zero);

            writer[5] = Mathf.Clamp(fLocal.x / nx, -1f, 1f);
            writer[6] = Mathf.Clamp(fLocal.z / nz, -1f, 1f);
            writer[7] = Mathf.Clamp(hLocal.x / nx, -1f, 1f);
            writer[8] = Mathf.Clamp(hLocal.z / nz, -1f, 1f);

            float hunger01 = (c && c.maxHunger > 0f) ? Mathf.Clamp01(c.Hunger / c.maxHunger) : 0f;
            writer[9] = 1f - hunger01;

            return 10; // ★ 이게 중요: "내가 10개를 썼다"
        }

        public override void WriteObservation(float[] output)
        {
            // 길이 10 보장(혹시 아니면 방어)
            if (output == null || output.Length < 10) return;

            // 0으로 초기화
            Array.Clear(output, 0, output.Length);

            output[0] = (c && c.maxHealth > 0f) ? c.Health / c.maxHealth : 0f;
            output[1] = (c && c.maxHunger > 0f) ? c.Hunger / c.maxHunger : 0f;
            output[2] = c ? Mathf.Clamp01(c.Danger) : 0f;

            Vector3 pos = c ? c.transform.position : Vector3.zero;

            float minX = 0f, maxX = 1f, minZ = 0f, maxZ = 1f;
            float nx = 1f, nz = 1f;

            if (c && c.env)
            {
                minX = c.env.minXZ.x;
                maxX = c.env.maxXZ.x;
                minZ = c.env.minXZ.y;
                maxZ = c.env.maxXZ.y;

                nx = Mathf.Max(0.0001f, (maxX - minX) * 0.5f);
                nz = Mathf.Max(0.0001f, (maxZ - minZ) * 0.5f);
            }

            output[3] = (maxX > minX) ? Mathf.InverseLerp(minX, maxX, pos.x) : 0.5f;
            output[4] = (maxZ > minZ) ? Mathf.InverseLerp(minZ, maxZ, pos.z) : 0.5f;

            Transform foodTf   = GetNearestActive((c && c.env) ? c.env.Foods   : null, pos);
            Transform hazardTf = GetNearestActive((c && c.env) ? c.env.Hazards : null, pos);

            Vector3 fWorld = foodTf ? (foodTf.position - pos) : Vector3.zero;
            Vector3 hWorld = hazardTf ? (hazardTf.position - pos) : Vector3.zero;

            Vector3 fLocal = (c ? c.transform.InverseTransformDirection(fWorld) : Vector3.zero);
            Vector3 hLocal = (c ? c.transform.InverseTransformDirection(hWorld) : Vector3.zero);

            output[5] = Mathf.Clamp(fLocal.x / nx, -1f, 1f);
            output[6] = Mathf.Clamp(fLocal.z / nz, -1f, 1f);
            output[7] = Mathf.Clamp(hLocal.x / nx, -1f, 1f);
            output[8] = Mathf.Clamp(hLocal.z / nz, -1f, 1f);

            float hunger01 = (c && c.maxHunger > 0f) ? Mathf.Clamp01(c.Hunger / c.maxHunger) : 0f;
            output[9] = 1f - hunger01;
        }


        static Transform GetNearestActive(Transform[] arr, Vector3 from)
        {
            if (arr == null || arr.Length == 0) return null;

            Transform best = null;
            float bestD2 = float.PositiveInfinity;

            for (int i = 0; i < arr.Length; i++)
            {
                var t = arr[i];
                if (!t || !t.gameObject.activeInHierarchy) continue;

                float d2 = (t.position - from).sqrMagnitude;
                if (d2 < bestD2) { bestD2 = d2; best = t; }
            }
            return best;
        }
    }
}
