using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // UI 이벤트 시스템을 사용할 필요는 없지만, UI Interactor와 함께 사용됩니다.

// DrawWithRayInteractor 스크립트는 3D Raycast를 통해 스케치북에 그림을 그리는 역할을 합니다.
public class DrawWithRayInteractor : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public InputActionProperty triggerButton;

    private Vector2? lastUV = null;
    // LayerMask.NameToLayer("color") 관련 변수는 이제 필요 없습니다.
    private XRInteractorLineVisual lineVisual; // LineVisual 컴포넌트
    private Gradient originalValidGradient;
    // private Gradient hoverColorGradient; // 이제 이 스크립트에서 레이저 색상을 직접 바꾸지 않습니다.
    private Renderer cachedSketchBookRenderer;

    private void Start()
    {
        // 색상 레이어 관련 코드는 제거합니다.
        // colorLayer = LayerMask.NameToLayer("color"); 

        lineVisual = rayInteractor.GetComponent<XRInteractorLineVisual>();

        if (lineVisual != null)
        {
            // 원래 레이저 색상 그라디언트를 저장해 둡니다.
            originalValidGradient = lineVisual.validColorGradient;
        }
        else
        {
            Debug.LogError("XRInteractorLineVisual component not found on the ray interactor!");
        }
        // hoverColorGradient = new Gradient(); // 제거
    }

    private void Update()
    {
        // 1. Raycast Hit 확인
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // 레이저 색상 복원 (만약 DrawWithRayInteractor가 유효한 히트를 할 때)
            // 레이저 색상은 이제 UI 상호작용 스크립트나 기본 XR Interactor 설정에 의해 결정됩니다.
            if (lineVisual != null)
            {
                lineVisual.validColorGradient = originalValidGradient;
            }

            // --- UI 색상 피킹 로직 제거 ---
            // UI를 Hit 했을 때의 색상 피킹 및 레이저 색상 변경 로직은 
            // ColorPickerUI.cs 스크립트와 XR UI Interactor에 의해 처리됩니다.
            // --------------------------------

            SketchBook sketchBook = hit.collider.GetComponent<SketchBook>();

            if (sketchBook != null)
            {
                // 스케치북 Renderer 캐싱
                if (cachedSketchBookRenderer == null || cachedSketchBookRenderer.gameObject != sketchBook.gameObject)
                {
                    cachedSketchBookRenderer = sketchBook.GetComponent<Renderer>();
                }

                if (triggerButton.reference.action.IsPressed())
                {
                    // --- 1. 드로잉 중 ---
                    Vector2 currentUV;
                    if (TryGetUVCoordinates(hit, sketchBook, out currentUV))
                    {
                        if (lastUV.HasValue)
                        {
                            DrawLineBetweenUVs(sketchBook, lastUV.Value, currentUV);
                        }
                        lastUV = currentUV;
                    }
                }
                else
                {
                    // --- 2. 드로잉 중이 아님 ---
                    lastUV = null;
                }
            }
            else
            {
                // 스케치북이 아닌 다른 오브젝트를 Hit
                lastUV = null;
                cachedSketchBookRenderer = null;
            }
        }
        else
        {
            // Raycast Hit 실패
            lastUV = null;
            cachedSketchBookRenderer = null;
        }
    }

    // 레이저 색상 변경 함수는 이제 ColorPickerUI.cs나 XR UI Interactor에서 처리해야 합니다.
    /*
    private void SetLaserToColor(Color color)
    {
        // 이 함수는 DrawWithRayInteractor.cs에서 더 이상 사용되지 않습니다.
    }
    */

    // 두 UV 좌표 사이에 선을 그리는 함수 (기존 로직 유지)
    private void DrawLineBetweenUVs(SketchBook sketchBook, Vector2 startUV, Vector2 endUV)
    {
        float texWidth = sketchBook.texture.width;
        float texHeight = sketchBook.texture.height;

        Vector2 startPixel = new Vector2(startUV.x * texWidth, startUV.y * texHeight);
        Vector2 endPixel = new Vector2(endUV.x * texWidth, endUV.y * texHeight);

        // 브러시 크기에 따라 stepSize를 조절하여 끊김 현상을 방지합니다.
        float stepSize = Mathf.Max(1.0f, sketchBook.brushSize / 2.0f);
        float pixelDistance = Vector2.Distance(startPixel, endPixel);

        int steps = Mathf.CeilToInt(pixelDistance / stepSize);
        steps = Mathf.Max(1, steps);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / (float)steps;
            Vector2 interpolatedUV = Vector2.Lerp(startUV, endUV, t);
            sketchBook.DrawAtUV(interpolatedUV);
        }
    }

    // Raycast Hit 정보를 UV 좌표로 변환하는 함수 (기존 로직 유지)
    private bool TryGetUVCoordinates(RaycastHit hit, SketchBook sketchBook, out Vector2 uv)
    {
        uv = Vector2.zero;
        // 3D Collider Hit을 통해 UV 좌표를 얻습니다.
        if (hit.collider != null && hit.collider.GetComponent<SketchBook>() == sketchBook)
        {
            // hit.textureCoord는 Mesh Renderer의 UV를 반환합니다.
            uv = hit.textureCoord;
            return true;
        }
        return false;
    }
}