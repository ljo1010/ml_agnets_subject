using UnityEngine;
using Unity.MLAgents;   // ★ 추가

public class Food : MonoBehaviour
{
    public float eatAmount = 40f;
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
        if (agent) agent.AddReward(+3.0f);

        // ★ 커스텀 지표 기록 (Food 1개 먹음)
        Academy.Instance.StatsRecorder.Add("custom/food_eat", 1f);

        controller.Eat(eatAmount);

        if (env) env.RespawnOneFood(transform, controller.transform.position);

        Debug.Log($"FOOD EAT: {name} <- {controller.name}");
    }
}
