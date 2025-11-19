using UnityEngine;

public class ZoneUIActivator : MonoBehaviour
{
    public GameObject targetSystemObject; // 활성화/비활성화 할 대상 (Drawing System 전체)
    public string playerTag = "Player";   // XR Origin(플레이어)에 설정된 태그

    private void Start()
    {
        // 게임 시작 시에는 꺼둡니다 (필요에 따라 변경 가능)
        targetSystemObject.SetActive(false);
    }

    // 플레이어가 영역(앵커)에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetSystemObject.SetActive(true);
        }
    }

    // 플레이어가 영역(앵커)에서 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetSystemObject.SetActive(false);
        }
    }
}