using UnityEngine;
using Unity.MLAgents;   // ★ 추가

public class Hazard : MonoBehaviour
{
    public float damage = 20f;
    public float dangerAdd01 = 0.5f;
    EnvManager env;

    void Awake()
    {
        env = GetComponentInParent<EnvManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponentInParent<BasicController>();
        if (!controller) return;

        var agent = other.GetComponentInParent<SurvivalAgent>();
        if (agent) agent.AddReward(-1.0f);

        // ★ 커스텀 지표 기록 (Hazard 1번 맞음)
        Academy.Instance.StatsRecorder.Add("custom/hazard_hit", 1f);

        controller.ApplyDamage(damage);
        controller.AddDanger01(dangerAdd01);

        if (env) env.RespawnOneHazard(transform, controller.transform.position);

        Debug.Log($"HAZARD HIT: {name} -> {controller.name}");
    }
}
