using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class StyleTransferManagerXR : MonoBehaviour
{
    [Header("XR")]
    public XRRayInteractor rayInteractor; // XR Ray Interactor
    public InputActionProperty triggerButton;

    [Header("Model")]
    public NNModel AdaINModel;
    
    [Header("Inputs")]
    public GameObject contentGO;
    public GameObject styleGO;

    [Header("Output")]
    public RenderTexture output;

    private Texture2D content;
    private Model _runtimeModel;
    private IWorker _worker;

    private Dictionary<string, Tensor> Inputs = new Dictionary<string, Tensor>();


    void Set()
    {
        _runtimeModel = ModelLoader.Load(AdaINModel);
        content = GetTexture2D(contentGO);

        //execute();
    }

    void execute()
    {
        if (_worker != null)
        {
            _worker.Dispose();
        }

        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);
        Inputs.Clear();

        content = GetTexture2D(contentGO);
        Texture2D style = GetTexture2D(styleGO);
        style = ResizeTexture(style);

        Tensor t_content = new Tensor(content, 3);
        Tensor t_style = new Tensor(style, 3);
        Inputs.Add("content", t_content);
        Inputs.Add("style", t_style);

        _worker.Execute(Inputs);

        Tensor t_output = _worker.PeekOutput("output");
        t_output.ToRenderTexture(output);

        t_content.Dispose();
        t_style.Dispose();
        t_output.Dispose();
    }

    void StyleChanged()
    {
        if (triggerButton.reference.action.IsPressed()) // 클릭
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject clickedGO = hit.collider.gameObject;
                if (styleGO != null) // clickedGO.GetInstanceID() != styleGO.GetInstanceID()
                {
                    styleGO = clickedGO;
                    Debug.Log($"Style texture changed to: {styleGO.name}");
                    execute();
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

    private Texture2D ResizeTexture(Texture2D source, int width=512, int height=512)
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
        Set();
    }

    private void Update()
    {
        StyleChanged();
    }

    private void OnDestroy()
    {
        _worker.Dispose();
    }
}
