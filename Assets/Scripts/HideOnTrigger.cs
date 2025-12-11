using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
// 리스트를 사용하기 위해 추가 (배열만 쓸 거면 없어도 되지만, List 사용 시 필요)
using System.Collections.Generic;

[RequireComponent(typeof(XRSimpleInteractable))]
public class HideOnTrigger : MonoBehaviour
{
    // [변경점 1] 단일 변수 대신 배열로 선언
    public GameObject[] objectsToHide;

    public InputActionProperty triggerButton;

    private XRSimpleInteractable interactable;

    void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            Debug.LogError("XR Simple Interactable 컴포넌트가 필요합니다!");
        }
    }

    void Update()
    {
        bool isTriggerPressed = triggerButton.action != null && triggerButton.action.WasPressedThisFrame();
        bool isHovered = interactable != null && interactable.isHovered;

        if (isHovered && isTriggerPressed)
        {
            HideTargetObjects(); // 함수 이름도 복수형으로 변경
        }
    }

    private void HideTargetObjects()
    {
        // [변경점 2] 배열에 있는 모든 오브젝트를 하나씩 꺼내서 비활성화
        if (objectsToHide != null)
        {
            foreach (GameObject obj in objectsToHide)
            {
                // 리스트에 빈 칸이 있거나 이미 삭제된 객체가 있을 수 있으므로 null 체크
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
            //Debug.Log($"{objectsToHide.Length}개의 오브젝트가 숨겨졌습니다.");
        }
    }
}