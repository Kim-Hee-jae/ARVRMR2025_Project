using UnityEngine;

public class AnchorHoverEffect : MonoBehaviour // 앵커 선택 스크립트
{
    [Tooltip("색상을 변경할 대상 렌더러입니다. 비워두면 이 오브젝트의 렌더러를 사용합니다.")]
    public Renderer targetRenderer;

    [Tooltip("활성화/비활성화할 자식 Plane 오브젝트입니다.")]
    public GameObject childPlane;

    public Color hoverColor = Color.blue;
    public Color defaultColor = Color.red;

    // 런타임에 사용할 고유한 머티리얼 인스턴스
    private Material materialInstance;

    void Start()
    {
        // 1. 렌더러 찾기
        // targetRenderer가 인스펙터에서 할당되지 않았다면, 이 게임 오브젝트에 붙어있는 Renderer를 찾습니다.
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogError("AnchorHoverEffect: 대상 렌더러를 찾을 수 없습니다!", this);
            return;
        }

        // 2. 고유 머티리얼 인스턴스 생성
        // renderer.material을 사용하면 런타임에 고유한 머티리얼 인스턴스가 생성됩니다.
        // 이렇게 해야 원본 머티리얼 애셋이 변경되지 않고, 각 앵커가 고유한 색상을 가질 수 있습니다.
        materialInstance = targetRenderer.material;

        // 3. 초기 상태 설정 (빨간색, Plane 비활성화)
        materialInstance.color = defaultColor;
        if (childPlane != null)
        {
            childPlane.SetActive(false);
        }
    }

    // 이 함수는 Teleport Anchor의 'Hover Entered' 이벤트에 연결합니다.
    public void HandleHoverEnter()
    {
        if (materialInstance == null) return;

        // 색상을 파란색으로 변경
        materialInstance.color = hoverColor;

        // 자식 Plane 활성화
        if (childPlane != null)
        {
            childPlane.SetActive(true);
        }
    }

    // 이 함수는 Teleport Anchor의 'Hover Exited' 이벤트에 연결합니다.
    public void HandleHoverExit()
    {
        if (materialInstance == null) return;

        // 색상을 다시 빨간색으로 변경
        materialInstance.color = defaultColor;

        // 자식 Plane 비활성화
        if (childPlane != null)
        {
            childPlane.SetActive(false);
        }
    }
}