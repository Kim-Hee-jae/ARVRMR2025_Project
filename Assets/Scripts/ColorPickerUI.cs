using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

// IPointerClickHandler: 클릭(트리거) 이벤트를 받습니다.
// IPointerMoveHandler: 레이저 포인터가 이미지 위를 움직이는 이벤트를 받습니다. (호버링 색상 변경용)
public class ColorPickerUI : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
{
    private Image colorImage;
    private RectTransform rectTransform;

    // 레이저 포인터의 색상을 변경할 LineVisual 컴포넌트 참조
    // (DrawWithRayInteractor와 동일한 Interactor에 있어야 합니다.)
    [Header("XR Interactor Settings")]
    [Tooltip("색상을 변경할 XR Ray Interactor의 Line Visual을 연결하세요.")]
    public XRInteractorLineVisual rayLineVisual;

    private Gradient originalValidGradient;
    private Gradient hoverColorGradient;

    private void Awake()
    {
        colorImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // UI Image에 텍스처가 할당되어 있는지 확인
        if (colorImage == null || colorImage.sprite == null || colorImage.sprite.texture == null)
        {
            Debug.LogError("[ColorPickerUI] Image 컴포넌트나 텍스처가 할당되지 않았습니다. 이 스크립트는 UI Image에 부착되어야 합니다.");
            return;
        }

        // 텍스처의 Read/Write Enabled 설정이 활성화되어 있는지 확인
        if (!colorImage.sprite.texture.isReadable)
        {
            Debug.LogError("[ColorPickerUI] 컬러 휠 텍스처의 'Read/Write Enabled' 설정을 활성화해야 합니다!");
        }

        // 레이저 색상 초기화
        if (rayLineVisual != null)
        {
            originalValidGradient = rayLineVisual.validColorGradient;
            hoverColorGradient = new Gradient();
        }
    }

    // 포인터가 UI 요소를 클릭했을 때 호출됩니다. (색상 선택)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 클릭 위치를 Image의 로컬 좌표로 변환
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            return;
        }

        // 2. Local 좌표를 0.0 ~ 1.0의 UV 좌표로 변환
        Vector2 normalizedUV = new Vector2(
            (localCursor.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
            (localCursor.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height
        );

        // 3. 텍스처에서 픽셀 색상 읽기
        Color pickedColor = GetColorFromImage(normalizedUV);

        // 4. 유효한 색상인지 확인 (투명도 검사)
        if (pickedColor.a > 0.01f)
        {
            SketchBook.drawColor = pickedColor;
            Debug.Log($"[ColorPickerUI] Color Selected: {pickedColor}");

            // 선택된 색상으로 레이저 색상을 즉시 변경
            SetLaserColor(pickedColor);
        }
        else
        {
            Debug.Log("[ColorPickerUI] Transparent area clicked. No color change.");
        }
    }

    // 포인터가 이미지 위를 움직일 때 호출됩니다. (호버링 색상 변경)
    public void OnPointerMove(PointerEventData eventData)
    {
        // 1. 포인터 위치를 로컬 좌표로 변환 (OnPointerClick과 동일)
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            return;
        }

        // 2. Local 좌표를 0.0 ~ 1.0의 UV 좌표로 변환
        Vector2 normalizedUV = new Vector2(
            (localCursor.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
            (localCursor.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height
        );

        // 3. 호버링 중인 픽셀 색상 읽기
        Color hoverColor = GetColorFromImage(normalizedUV);

        // 4. 투명하지 않은 영역일 때만 레이저 색상을 변경합니다.
        if (hoverColor.a > 0.01f)
        {
            SetLaserColor(hoverColor);
        }
        else
        {
            // 투명한 영역을 호버할 경우, 레이저 색상을 원래대로 돌려놓습니다.
            SetLaserToOriginalColor();
        }
    }

    // --- 유틸리티 함수 ---
    private Color GetColorFromImage(Vector2 uv)
    {
        Texture2D texture = colorImage.sprite.texture;

        int pixelX = Mathf.FloorToInt(uv.x * texture.width);
        int pixelY = Mathf.FloorToInt(uv.y * texture.height);

        if (pixelX < 0 || pixelX >= texture.width || pixelY < 0 || pixelY >= texture.height)
        {
            return Color.clear;
        }

        // Texture2D.GetPixel() 호출
        return texture.GetPixel(pixelX, pixelY);
    }

    private void SetLaserColor(Color color)
    {
        if (rayLineVisual == null) return;

        hoverColorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0.0f), new GradientAlphaKey(color.a, 1.0f) }
        );
        rayLineVisual.validColorGradient = hoverColorGradient;
    }

    private void SetLaserToOriginalColor()
    {
        if (rayLineVisual == null) return;
        rayLineVisual.validColorGradient = originalValidGradient;
    }

    // 포인터가 영역을 벗어났을 때 원래 색상으로 복원
    public void OnPointerExit(PointerEventData eventData)
    {
        SetLaserToOriginalColor();
    }
}