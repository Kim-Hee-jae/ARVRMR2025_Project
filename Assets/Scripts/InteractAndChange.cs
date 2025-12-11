using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Input System 네임스페이스
using UnityEngine.XR.Interaction.Toolkit; // XR Toolkit 네임스페이스

[RequireComponent(typeof(XRSimpleInteractable))] // 이 컴포넌트가 자동으로 추가됩니다.
public class InteractAndChange : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("사용할 트리거 버튼의 Input Action을 할당하세요 (예: XRI RightHand Interaction/Activate)")]
    public InputActionProperty triggerButton;

    [Header("Target Objects")]
    [Tooltip("보이게 할 물체 B")]
    public GameObject objectB;
    [Tooltip("보이게 할 물체 C")]
    public GameObject objectC;

    [Header("Material Settings")]
    [Tooltip("B에 적용할 새로운 머티리얼")]
    public Material targetMaterialForB;

    // 내부 변수: 현재 컨트롤러가 이 물체 위에 있는지 확인
    private bool isHovering = false;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        // XRI 이벤트를 코드로 연결 (인스펙터에서 안 해도 됨)
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        isHovering = true;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
    }

    private void Update()
    {
        // 1. 컨트롤러가 물체 위에 있고(Hover)
        // 2. 트리거 버튼 액션이 유효하며
        // 3. 이번 프레임에 버튼이 눌렸다면
        if (isHovering && triggerButton.action != null && triggerButton.action.WasPressedThisFrame())
        {
            PerformAction();
        }
    }

    private void PerformAction()
    {
        // B와 C를 보이게 함
        if (objectB != null) objectB.SetActive(true);
        if (objectC != null) objectC.SetActive(true);

        // B의 머티리얼 변경
        if (objectB != null && targetMaterialForB != null)
        {
            Renderer bRenderer = objectB.GetComponent<Renderer>();
            if (bRenderer != null)
            {
                bRenderer.material = targetMaterialForB;
            }
        }

        //Debug.Log($"{gameObject.name} 상호작용 성공!");
    }
}