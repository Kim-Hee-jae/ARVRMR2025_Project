using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Sentis;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class StyleTransferManager : MonoBehaviour
{
    [Header("XR")]
    public XRRayInteractor rayInteractor; // XR Ray Interactor
    public InputActionProperty triggerButton;

    [Header("Inputs")]
    public DrawingTextureManager textureManager;
    public SDImg2ImgClient styleReference;


    void execute(GameObject clickedGO)
    {
        int idx;
        if (clickedGO.tag == "style1")
            idx = 0;
        else if (clickedGO.tag == "style2")
            idx = 1;
        else if (clickedGO.tag == "style3")
            idx = 2;
        else
            idx = 3;

        textureManager.SetContentST(styleReference.lastStyleResults[idx]);
    }

    void StyleChanged()
    {
        if (triggerButton.reference.action.IsPressed()) // 클릭
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject clickedGO = hit.collider.gameObject;
                if (clickedGO != null & clickedGO.layer.Equals(LayerMask.NameToLayer("StyleImage"))) // clickedGO.GetInstanceID() != styleGO.GetInstanceID()
                {
                    Debug.Log($"Style texture changed to: {clickedGO.name}");
                    execute(clickedGO);
                    Debug.Log("Output image updated");
                }
            }
        }
    }

    private Texture2D GetTexture2D(GameObject go)
    {
        if (go == null)
            return null;

        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null && rend.material != null && rend.material.mainTexture is Texture2D)
        {
            return rend.material.mainTexture as Texture2D;
        }

        return null;
    }

    private Texture2D ResizeTexture(Texture2D source, int width = 512, int height = 512)
    {
        // RenderTexture를 임시로 생성
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);

        // RenderTexture를 읽어 Texture2D로 변환
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        // 리소스 정리
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }


    private void Awake()
    {
    }

    private void Update()
    {
        StyleChanged();
    }

    private void OnDestroy()
    {
    }
}