using System.Collections;
using UnityEngine;


public class EnvManager : MonoBehaviour
{
    [Header("Ground (Platform / Plane)")]
    public Transform ground;      // Platform(바닥) 넣기
    public float margin = 0.5f;   // 가장자리 여유

    [Header("Spawn Y")]
    public float y = 0.5f;

    [Header("Spawn Area (auto)")]
    public Vector2 minXZ;
    public Vector2 maxXZ;

    [Header("Refs")]
    public Transform foodPrefab;
    public Transform hazardPrefab;

    public int foodCount = 10;
    public int hazardCount = 2;

    private Transform[] foods;
    private Transform[] hazards;

    [Header("Spawn Rules")]
    public float minDistanceFromAgent = 1.5f;
    public float minDistanceFoodHazard = 1.5f;
    public int maxTries = 50;

    void Awake()
    {
        if (!ground)
            ground = transform.Find("Platform"); // 너의 실제 오브젝트 이름으로

        RecalcBoundsFromGround();
        SpawnAll();
    }

    void SpawnAll()
    {
        foods = new Transform[foodCount];
        hazards = new Transform[hazardCount];

        for (int i = 0; i < foodCount; i++)
        {
            foods[i] = Instantiate(foodPrefab, transform);
            foods[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < hazardCount; i++)
        {
            hazards[i] = Instantiate(hazardPrefab, transform);
            hazards[i].gameObject.SetActive(false);
        }
    }

public void RecalcBoundsFromGround()
{
    if (!ground) return;

    // 월드 기준 바운드
    var col = ground.GetComponent<Collider>();
    Bounds b;

    if (col != null) b = col.bounds;
    else
    {
        var rend = ground.GetComponent<Renderer>();
        if (rend == null) return;
        b = rend.bounds;
    }

    // 월드 좌표 min/max를 그대로 사용
    minXZ = new Vector2(b.min.x + margin, b.min.z + margin);
    maxXZ = new Vector2(b.max.x - margin, b.max.z - margin);
}


    public Vector3 ClampAgentPos(Vector3 p)
    {
        p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);
        p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);
        p.y = y;
        return p;
    }

    public void ResetEnvironment(Transform agent)
    {
        EnsureSpawned();
        RespawnFoods(agent.position);
        RespawnHazards(agent.position);
    }

    void EnsureSpawned()
{
    if (foods == null || hazards == null) SpawnAll();
}

    void RespawnFoods(Vector3 agentPos)
    {
        if (foods == null) return;

        for (int i = 0; i < foods.Length; i++)
        {
            if (!foods[i]) continue; // 혹시 null 슬롯 방어
            foods[i].position = SampleValidPos(agentPos, Vector3.positiveInfinity);
            foods[i].gameObject.SetActive(true);
        }
    }

    public void RespawnOneFood(Transform eatenFood, Vector3 agentPos)
{
    eatenFood.position = SampleValidPos(agentPos, Vector3.positiveInfinity);
    eatenFood.gameObject.SetActive(true);
}

    void RespawnHazards(Vector3 agentPos)
    {
        for (int i = 0; i < hazards.Length; i++)
        {
            Vector3 other = (i > 0) ? hazards[i - 1].position : Vector3.positiveInfinity;
            hazards[i].position = SampleValidPos(agentPos, other);
            hazards[i].gameObject.SetActive(true);
        }
    }
    public void RespawnOneHazard(Transform hitHazard, Vector3 agentPos)
    {
        // 가장 가까운 food 하나를 otherPos로 쓰거나, 그냥 첫 food를 써도 됨
        Vector3 other = (foods != null && foods.Length > 0) ? foods[0].position : Vector3.positiveInfinity;

        hitHazard.position = SampleValidPos(agentPos, other);
        hitHazard.gameObject.SetActive(true);
    }



    Vector3 SampleValidPos(Vector3 agentPos, Vector3 otherPos)
    {
        for (int i = 0; i < maxTries; i++)
        {
            var p = new Vector3(
                Random.Range(minXZ.x, maxXZ.x),
                y,
                Random.Range(minXZ.y, maxXZ.y)
            );

            if (Vector3.Distance(p, agentPos) < minDistanceFromAgent) continue;
            if (otherPos != Vector3.positiveInfinity && Vector3.Distance(p, otherPos) < minDistanceFoodHazard) continue;

            return p;
        }
        return new Vector3(0f, y, 0f);
        
    }
    public Transform[] Foods => foods;
    public Transform[] Hazards => hazards;
}
