using UnityEngine;

public class ArrowGuideController : MonoBehaviour
{
    [Header("Settings")]
    public Transform targetAnchor; // 목표지점
    public bool lookAtTarget = true;

    [Header("Movement")]
    public float moveSpeed = 0.5f;
    public bool loopMovement = true;

    // 회전 보정값 (X축으로 90도 더하기)
    [Header("Rotation Adjustment")]
    public float xRotationOffset;

    private bool isCompleted = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isCompleted || targetAnchor == null) return;

        // 1. 이동 로직 (회전과 상관없이 무조건 목표 위치로 이동)
        // MoveTowards는 내 회전 상태와 무관하게 A지점에서 B지점으로 이동시켜 줍니다.
        transform.position = Vector3.MoveTowards(transform.position, targetAnchor.position, moveSpeed * Time.deltaTime);

        // 2. 회전 로직 (목표를 보되, 90도 꺾기)
        if (lookAtTarget)
        {
            // 먼저 목표를 바라봄
            transform.LookAt(targetAnchor);

            // 그 상태에서 X축으로 90도 더 회전 (Local 축 기준)
            transform.Rotate(xRotationOffset, 0f, 0f);
        }

        // 3. 반복 이동 (목표 도착 체크)
        if (loopMovement)
        {
            float distance = Vector3.Distance(transform.position, targetAnchor.position);

            // 목표에 거의 도착했으면(0.1m 이내) 시작점으로 리셋
            if (distance < 0.1f)
            {
                transform.position = startPosition;
            }
        }
    }

    public void CompleteTutorial()
    {
        if (isCompleted) return;
        isCompleted = true;
        gameObject.SetActive(false);
        Debug.Log("튜토리얼 이동 완료.");
    }
}