using System;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// 2D 연속 이동용 ActuatorComponent (X: 좌/우, Z: 전/후)
    /// - Continuous 2개: [0]=X, [1]=Z
    /// - 값 범위: -1 ~ +1 (MoveContinuous에서 속도/스케일 적용)
    /// </summary>
    public class BasicActuatorComponent : ActuatorComponent
    {
        public BasicController basicController;

        // Continuous 2개(X,Z)
        private readonly ActionSpec m_ActionSpec = ActionSpec.MakeContinuous(3);

        public override ActionSpec ActionSpec => m_ActionSpec;

        public override IActuator[] CreateActuators()
        {
            return new IActuator[] { new BasicActuator(basicController, m_ActionSpec) };
        }
    }

    public class BasicActuator : IActuator
    {
        private readonly BasicController basicController;
        private readonly ActionSpec m_ActionSpec;

        public BasicActuator(BasicController controller, ActionSpec actionSpec)
        {
            basicController = controller;
            m_ActionSpec = actionSpec;
        }

        public ActionSpec ActionSpec => m_ActionSpec;

        public string Name => "Basic2D_Continuous";

        public void ResetData() { }

        public void OnActionReceived(ActionBuffers actionBuffers)
        {
            float ax   = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
            float az   = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
            float turn = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
            

            // ★ [추가] 데드존: 5% 미만의 미세한 입력은 그냥 무시(0으로 처리)
        if (Mathf.Abs(ax) < 0.05f) ax = 0f;
        if (Mathf.Abs(az) < 0.05f) az = 0f;
        if (Mathf.Abs(turn) < 0.05f) turn = 0f;
            // turn 절댓값이 클수록 아주 미세하게 손해
            if (basicController && basicController.TryGetComponent<SurvivalAgent>(out var ag))
            {
                float turnCost = 0.0002f * Mathf.Abs(turn);
                ag.AddReward(-turnCost);
            }

            basicController.MoveContinuous(ax, az);
            basicController.Rotate(turn);
        }

        // 키보드 입력 안 쓸 거면 비워둬도 됨(컴파일만 되게 0으로)
        public void Heuristic(in ActionBuffers actionBuffersOut)
        {
            var ca = actionBuffersOut.ContinuousActions;
            ca[0] = Input.GetAxisRaw("Horizontal"); // A/D
            ca[1] = Input.GetAxisRaw("Vertical");   // W/S
            ca[2] = 0f;
            if (Input.GetKey(KeyCode.Q)) ca[2] = -1f;
            if (Input.GetKey(KeyCode.E)) ca[2] =  1f;
        }

        // Continuous만 쓰므로 비워둬도 됨
        public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) { }
    }
}
