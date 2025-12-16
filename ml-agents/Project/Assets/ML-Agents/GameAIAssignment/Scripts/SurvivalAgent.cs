using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SurvivalAgent : Agent
{
    public BasicController controller;

    Vector3 lastPos;
    int stuckSteps;

    public float stuckDistSqr = 0.0004f; // (0.02m)^2
    public int stuckLimitSteps = 200;

    [Header("Shaping")]
    public float moveRewardScale = 0.0f;
    public float turnPenaltyScale = 0.0002f;
    public float stuckEndPenalty = -0.2f;

    bool episodeEnding;

    public override void Initialize()
    {
        if (!controller) controller = GetComponent<BasicController>();
    }

    public override void OnEpisodeBegin()
    {
        episodeEnding = false;

        controller?.ResetEpisode();
        lastPos = transform.position;
        stuckSteps = 0;
    }

    void EndOnce(float reward)
    {
        if (episodeEnding) return;
        episodeEnding = true;
        AddReward(reward);
        EndEpisode();
    }

    void FixedUpdate()
        {
            if (episodeEnding) return;

            // ★ [추가 1] 존재 페널티 (Existence Penalty)
            // "숨만 쉬어도 손해"여야 밥을 찾으러 나갑니다.
            // 벽에 붙어서 스턱(Stuck) 페널티를 피해도, 이 점수는 계속 깎이므로 결국 움직이게 됩니다.
            AddReward(-0.0005f); 

            Vector3 delta = transform.position - lastPos;
            float movedSqr = delta.sqrMagnitude;

            // 1) 이동 보상 (주석 처리 잘 하셨습니다!)
            // AddReward(moveRewardScale * delta.magnitude);

            // 2) 스턱 체크
            stuckSteps = (movedSqr < stuckDistSqr) ? (stuckSteps + 1) : 0;

            // 3) 위치 갱신은 맨 마지막에
            lastPos = transform.position;

            if (stuckSteps > stuckLimitSteps)
                EndOnce(stuckEndPenalty);
        }

        // ★ [추가 2] 벽 페널티 (Wall Penalty)
        // 벽에 닿아있는 동안(비비는 동안) 매 프레임 벌점을 줍니다.
        // 주의: 유니티 에디터에서 벽 오브젝트의 Tag를 반드시 "Wall"로 설정해야 작동합니다.
        void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                AddReward(-0.005f); // 벽에 비비면 꽤 아프게 감점
            }
        }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (episodeEnding) return;

        float turn = (actions.ContinuousActions.Length > 2)
            ? Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f)
            : 0f;

        // 회전 페널티(제자리 빙글빙글 억제)
        AddReward(-turnPenaltyScale * Mathf.Abs(turn));
    }
}
