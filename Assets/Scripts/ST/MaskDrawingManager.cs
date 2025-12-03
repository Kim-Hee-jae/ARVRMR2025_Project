using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskDrawingManager : MonoBehaviour
{
    [Header("Canvas 설정")]
    public GameObject canvas;

    [Header("XR 레이 / 레이 설정")]
    public XRRayInteractor rayInteractor;
    public float maxDistance = 10f;
    public LayerMask canvasLayer;

    [Header("XR 입력")]
    public InputActionProperty drawAction;  // 예: 오른손 Trigger
    public InputActionProperty eraseAction;  // 예: 왼손 Trigger

    [Header("브러시 설정")]
    public int maskResolution = 512;
    [Tooltip("0~1 (UV 기준 브러시 바깥 반지름)")]
    public float brushSizeUV = 0.03f;
    [Tooltip("innerRadius = outerRadius * innerSize")]
    [Range(0f, 1f)]
    public float innerSize = 0.8f;
    public Color brushColor = Color.white;  // 중심 밝기 1 기준이라 사실 R 채널만 씀

    private Texture2D _maskTex;
    public Texture2D maskTex => _maskTex;
    [HideInInspector]
    public bool hasStyle = false;
    public int mode = -1;

    void Start()
    {
        // 마스크 텍스처 생성 (처음엔 전부 검정 -> content만 보임)
        _maskTex = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false);
        ResetMask();
    }

    void Update()
    {

        if (!hasStyle) return;

        bool isDrawing = drawAction.reference != null &&
                         drawAction.reference.action != null &&
                         drawAction.reference.action.IsPressed();

        bool isErasing = eraseAction.reference != null &&
                         eraseAction.reference.action != null &&
                         eraseAction.reference.action.IsPressed();

        if (isDrawing) mode = 0;
        else if (isErasing) mode = 1;
        else mode = -1;

        if (mode == -1) return;
        ShootRayAndPaint();
    }

    public Texture SaveOutput(Texture content, Texture style)
    {
        Texture2D newContent = BlendContentStyleWithMask(
                ConvertToTexture2D(content),
                ConvertToTexture2D(style),
                _maskTex
                );

        ResetMask();
        return newContent;
    }

    public void ResetMask()
    {
        ClearMask(Color.black);
        _maskTex.Apply();
    }

    void ShootRayAndPaint()
    {
        if (rayInteractor != null)
        {
            bool hasHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (hasHit && hit.collider.gameObject == canvas)
            {
                Vector2 uv = hit.textureCoord; // 0~1
                PaintAtUV(uv);
            }
        }
       
    }

    void PaintAtUV(Vector2 uv)
    {
        int texWidth = _maskTex.width;
        int texHeight = _maskTex.height;

        int centerX = Mathf.FloorToInt(uv.x * texWidth);
        int centerY = Mathf.FloorToInt(uv.y * texHeight);

        int outerRadius = Mathf.RoundToInt(brushSizeUV * texWidth);
        int innerRadius = Mathf.RoundToInt(outerRadius * innerSize);

        int xStart = Mathf.Clamp(centerX - outerRadius, 0, texWidth - 1);
        int xEnd = Mathf.Clamp(centerX + outerRadius, 0, texWidth - 1);
        int yStart = Mathf.Clamp(centerY - outerRadius, 0, texHeight - 1);
        int yEnd = Mathf.Clamp(centerY + outerRadius, 0, texHeight - 1);

        for (int y = yStart; y <= yEnd; y++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > outerRadius)
                    continue; // 브러시 범위 밖

                float newVal;

                // 1) 중심부는 무조건 1
                if (dist <= innerRadius)
                {
                    newVal = 1f;
                }
                else
                {
                    // 2) innerRadius ~ outerRadius 페더링
                    float t = (outerRadius - dist) / (outerRadius - innerRadius);
                    t = Mathf.Clamp01(t);
                    t = t * t;
                    newVal = t; // 1 → 0
                }

                // 기존 값보다 더 밝게만 칠하기 (중첩)
                Color current = _maskTex.GetPixel(x, y);
                float blended = Mathf.Max(current.r, newVal);
                _maskTex.SetPixel(x, y, new Color(blended, blended, blended, 1f));
            }
        }

        _maskTex.Apply();
    }

    void ClearMask(Color c)
    {
        var cols = new Color[maskResolution * maskResolution];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        _maskTex.SetPixels(cols);
    }

    public static Texture2D BlendContentStyleWithMask(Texture2D content, Texture2D style, Texture2D mask)
    {
        if (content == null)
        {
            Debug.LogError("BlendContentStyleWithMask: content 텍스처가 null입니다.");
            return null;
        }
        if (style == null)
        {
            Debug.LogError("BlendContentStyleWithMask: style 텍스처가 null입니다.");
            return null;
        }
        if (mask == null)
        {
            Debug.LogError("BlendContentStyleWithMask: mask 텍스처가 null입니다.");
            return null;
        }

        int w = content.width;
        int h = content.height;

        if (style.width != w || style.height != h ||
            mask.width != w || mask.height != h)
        {
            Debug.LogError("BlendContentStyleWithMask: 텍스처 해상도가 서로 다릅니다.");
            return null;
        }

        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);

        Color[] contentPixels = content.GetPixels();
        Color[] stylePixels = style.GetPixels();
        Color[] maskPixels = mask.GetPixels();
        Color[] outPixels = new Color[contentPixels.Length];

        for (int i = 0; i < contentPixels.Length; i++)
        {
            Color ca = contentPixels[i];
            Color cb = stylePixels[i];
            float m = maskPixels[i].r;   // 마스크는 R 채널만 사용 (0~1)

            outPixels[i] = Color.Lerp(ca, cb, m);
        }

        result.SetPixels(outPixels);
        result.Apply();

        return result;
    }

    Texture2D ConvertToTexture2D(Texture tex)
    {
        RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0);
        Graphics.Blit(tex, rt);

        Texture2D tex2d = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);

        RenderTexture.active = rt;
        tex2d.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex2d.Apply();
        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(rt);

        return tex2d;
    }

}
