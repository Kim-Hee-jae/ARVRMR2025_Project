using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SketchBook : MonoBehaviour
{
    // 스케치북 텍스처
    public Texture2D texture;
    // 정적 드로잉 색상 (전역으로 사용)
    public static Color drawColor;
    public int brushSize = 5; // 기본 브러시 크기
    public bool drawTrue;

    public Color backgroundColor; // 배경색
    private int minBrushSize = 1;
    private int maxBrushSize = 50;

    [Header("Brush Controls")]
    [SerializeField] private InputActionReference joystickAction; // 오른쪽 조이스틱 입력
    [SerializeField] private float joystickThreshold = 0.2f; // 조이스틱 민감도 (데드존)

    [SerializeField] private float brushSizeChangeSpeed = 10.0f; // 초당 브러시 크기 변경 속도
    private float currentBrushSize; // 현재 크기

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        // 텍스처 초기화 (1024x768)
        // TextureFormat.RGBA32는 투명도를 포함하며, Read/Write가 필요할 때 설정해야 합니다. (이 스크립트는 Write만 함)
        texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        renderer.material.mainTexture = texture;

        // URP 쉐이더 설정 (기존과 동일)
        Shader transparentShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (transparentShader != null)
        {
            renderer.material.shader = transparentShader;
            renderer.material.SetFloat("_Surface", 1);
            renderer.material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            renderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            Debug.LogWarning("URP/Unlit Shader not found.");
        }

        drawColor = Color.black;

        // 텍스처 배경색 설정
        backgroundColor = Color.white;
        ClearTexture();
        drawTrue = false;

        if (joystickAction != null) joystickAction.action.Enable();

        currentBrushSize = brushSize;
    }

    private void Update()
    {
        // 조이스틱 입력으로 브러시 크기 조절 (기존 로직 유지)
        if (joystickAction != null)
        {
            Vector2 joystickValue = joystickAction.action.ReadValue<Vector2>();
            float horizontalInput = joystickValue.x;

            if (Mathf.Abs(horizontalInput) > joystickThreshold)
            {
                float changeAmount = horizontalInput * brushSizeChangeSpeed * Time.deltaTime;
                currentBrushSize += changeAmount;
                currentBrushSize = Mathf.Clamp(currentBrushSize, minBrushSize, maxBrushSize);
                brushSize = Mathf.RoundToInt(currentBrushSize);
            }
        }
    }

    // 드로잉 함수 (기존 로직 유지)
    public void DrawAtUV(Vector2 uv)
    {
        int pixelX = (int)(uv.x * texture.width);
        int pixelY = (int)(uv.y * texture.height);

        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                // 원형 브러시 효과 (옵션)
                if (x * x + y * y <= brushSize * brushSize)
                {
                    int px = pixelX + x;
                    int py = pixelY + y;

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, drawColor);
                    }
                }
            }
        }

        texture.Apply(); // 텍스처 업데이트
        drawTrue = true;
    }

    // 스케치북 전체 지우기 (기존 로직 유지)
    public void ClearTexture()
    {
        Color32[] colors = new Color32[texture.width * texture.height];
        Color32 bgColor = backgroundColor;

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = bgColor;
        }

        texture.SetPixels32(colors);
        texture.Apply();
        drawTrue = false;
    }

    /* * 주의: 이 함수는 더 이상 DrawWithRayInteractor.cs에서 사용되지 않습니다.
     * UI 기반 색상 피커가 이 로직을 대신합니다.
     * public static Color PickColorFromTexture(Renderer targetRenderer, Vector2 uv) { ... }
    */
}