using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskDrawingManager : MonoBehaviour
{
    [Header("입력 오브젝트")]
    public DrawingTextureManager textureManager;

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

    private static Material _blendMat;
    private static bool _blendMatInitialized = false;

    void Start()
    {
        // 마스크 텍스처 생성 (처음엔 전부 검정 -> content만 보임)
        _maskTex = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false);
        _maskTex.filterMode = FilterMode.Bilinear;
        ClearMask(Color.black);
        _maskTex.Apply();

        // 머티리얼의 _MaskTex 슬롯에 연결 
        textureManager.SetMaterialMask(_maskTex);
    }

    void Update()
    {
        if (textureManager.ContentChanged())
        {
            ClearMask(Color.black);
            _maskTex.Apply();
            textureManager.SetMaterialMask(_maskTex);
        }

        if (!textureManager.HasStyle) return;

        bool isDrawing = drawAction.reference != null &&
                         drawAction.reference.action != null &&
                         drawAction.reference.action.IsPressed();

        bool isErasing = eraseAction.reference != null &&
                         eraseAction.reference.action != null &&
                         eraseAction.reference.action.IsPressed();

        if (textureManager.StyleChanged())
        {
            Texture2D newContent = BlendContentStyleWithMask(
                ConvertToTexture2D(textureManager.GetMaterialContent()),
                ConvertToTexture2D(textureManager.GetContentSTp()), 
                _maskTex
                );

            ClearMask(Color.black);
            _maskTex.Apply();
            textureManager.SetMaterialMask(_maskTex);

            textureManager.SetMaterialContent_(newContent);           
        }

        if (isDrawing)
        {
            ShootRayAndPaint();
        }

        if (isErasing)
        {
            // textureManager.SetForErase();
            textureManager.SetContentST(textureManager.GetContentSel());
        }
    }

    public void SaveOutput()
    {
        Texture2D newContent = BlendContentStyleWithMask(
                ConvertToTexture2D(textureManager.GetMaterialContent()),
                ConvertToTexture2D(textureManager.GetContentST()),
                _maskTex
                );
        textureManager.SetOutput(newContent);

        ClearMask(Color.black);
        _maskTex.Apply();
        textureManager.SetMaterialMask(_maskTex);
    }

    void ShootRayAndPaint()
    {
        if (rayInteractor != null)
        {
            bool hasHit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (hasHit && hit.collider.gameObject == textureManager.gameObject)
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

    public Texture2D BlendContentStyleWithMask(Texture2D content, Texture2D style, Texture2D mask)
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

        EnsureBlendMaterial(); // 씬에 있는 DrawingTextureManager 머티리얼 복제

        if (_blendMat == null)
        {
            Debug.LogError("BlendContentStyleWithMask: _blendMat 초기화 실패 (Shader를 찾지 못함).");
            return null;
        }

        // 머티리얼에 텍스처 세팅 (현재 MaskBlend.shader에서 쓰는 이름 그대로)
        _blendMat.SetTexture("_MainTex", content);   // content
        _blendMat.SetTexture("_StyleTex", style);    // style
        _blendMat.SetTexture("_MaskTex", mask);      // mask

        // GPU에서 Blit → RT → ReadPixels로 Texture2D 생성
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        var prevActive = RenderTexture.active;

        Graphics.Blit(content, rt, _blendMat);

        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        result.Apply();

        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    // 씬에 있는 DrawingTextureManager의 머티리얼을 한 번만 복제해서 사용
    private void EnsureBlendMaterial()
    {
        if (_blendMatInitialized && _blendMat != null)
            return;

        _blendMatInitialized = true;

        if (textureManager != null && textureManager.targetRenderer != null && textureManager.targetRenderer.material != null)
        {
            // 현재 사용 중인 머티리얼(=MaskBlend shader 포함)을 복제
            _blendMat = new Material(textureManager.targetRenderer.material);
        }
        else
        {
            // 혹시 못 찾았을 때의 fallback (Shader 이름은 프로젝트에 맞게 수정 가능)
            Shader s = Shader.Find("MaskBlend");
            if (s == null)
                s = Shader.Find("Unlit/MaskBlend");
            if (s != null)
                _blendMat = new Material(s);
        }
    }

    public static Texture2D _BlendContentStyleWithMask(Texture2D content, Texture2D style, Texture2D mask)
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
