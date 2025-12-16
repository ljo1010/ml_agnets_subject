using Unity.MLAgents;
using UnityEngine;
using System.Collections;

public class BasicController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 180f;
    public EnvManager env;

    [Header("Needs")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;

    [SerializeField] private float health;
    [SerializeField] private float hunger;
    [SerializeField] private float danger;

    public float Health => health;
    public float Hunger => hunger;
    public float Danger => danger;

    [Header("Needs Drain")]
    public float hungerDrainPerSec = 1f;
    public float starvingDamagePerSec = 5f;

    Rigidbody rb;
    Vector3 startPos;
    SurvivalAgent agent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<SurvivalAgent>();
        startPos = transform.position;
    }

    void OnEnable()
    {
        // ResetEpisode();
    }
    IEnumerator Start()
{
    yield return null; // 1프레임 기다려서 EnvManager 초기화 끝내기
    ResetEpisode();
}

    void FixedUpdate()
    {
        // 1) 플레인 밖 방지(하드 클램프) + “벽 박치기” 처리 추가
        if (env)
        {
            var p = rb.position;
            var clamped = env.ClampAgentPos(p);

            if (clamped != p)
            {
                // 경계 닿으면 패널티 주고 에피소드 종료(벽러쉬 방지)
                if (agent)
                {
                    agent.AddReward(-0.2f);   // 값은 -0.2~-1.0 사이에서 튜닝
                    agent.EndEpisode();
                }

                rb.MovePosition(clamped);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                return; // 아래 needs 처리까지 가지 않게(선택)
            }
        }

        // 2) needs 감소 (학습 step과 맞추기 좋음)
        float dt = Time.fixedDeltaTime;

        hunger = Mathf.Max(0f, hunger - hungerDrainPerSec * dt);

        if (hunger <= 0f)
            health = Mathf.Max(0f, health - starvingDamagePerSec * dt);

        // 3) 사망 처리(Agent가 EndEpisode 호출)
        if (health <= 0f && agent)
        {
            agent.AddReward(-0.001f);
            agent.EndEpisode();
        }
    }


    public void ResetEpisode()
    {
        health = maxHealth;
        hunger = maxHunger;
        danger = 0f;

        var p = env ? env.ClampAgentPos(startPos) : startPos;
        rb.position = p;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (env) env.ResetEnvironment(transform);
    }

    public void MoveContinuous(float ax, float az)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var delta = new Vector3(ax, 0f, az) * (moveSpeed * Time.fixedDeltaTime);
        var next = rb.position + delta;

        if (env) next = env.ClampAgentPos(next);
        rb.MovePosition(next);
    }

    public void Rotate(float turn)
    {
        float angle = turn * turnSpeed * Time.fixedDeltaTime;
        var rot = rb.rotation * Quaternion.Euler(0f, angle, 0f);
        rb.MoveRotation(rot);
    }

    public void ApplyDamage(float amount) => health = Mathf.Max(0f, health - amount);
    public void Eat(float amount) => hunger = Mathf.Min(maxHunger, hunger + amount);
    public void AddDanger01(float amount01) => danger = Mathf.Clamp01(danger + amount01);
}
