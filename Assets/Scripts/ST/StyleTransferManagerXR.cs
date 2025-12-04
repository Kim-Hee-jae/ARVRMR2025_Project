//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using Unity.Sentis;
//using Unity.VisualScripting;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//using UnityEngine.XR.Interaction.Toolkit;

//public class StyleTransferManagerXR : MonoBehaviour
//{
//    [Header("XR")]
//    public XRRayInteractor rayInteractor; // XR Ray Interactor
//    public InputActionProperty triggerButton;

//    [Header("Model")]
//    public ModelAsset AdaINModel;

//    [Header("Inputs")]
//    public DrawingTextureManager textureManager;
//    private GameObject styleGO;

//    private Texture _content;
//    private Texture _style;
//    private RenderTexture _output;

//    private Model _runtimeModel;
//    private Worker _worker;

//    private Dictionary<string, Tensor> Inputs = new Dictionary<string, Tensor>();

//    private TextureTransform _transform;
//    Tensor<float> _contentTensor;
//    Tensor<float> _styleTensor;
//    Tensor<float> _outputTensor;


//    void Set()
//    {
//        _runtimeModel = ModelLoader.Load(AdaINModel);
//        _content = textureManager.GetContentSel();
//        _transform = new TextureTransform().SetDimensions(width: 512, height: 512);

//        SetTensor();
//        //execute();
//    }

//    void SetTensor()
//    {
//        var shape = new TensorShape(1, 3, 512, 512);
//        _contentTensor = new Tensor<float>(shape);
//        _styleTensor = new Tensor<float>(shape);
//    }

//    void execute()
//    {
//        if (_worker != null)
//        {
//            _worker.Dispose();
//        }

//        _worker = new Worker(_runtimeModel, BackendType.GPUCompute);
//        Inputs.Clear();
//        SetTensor();

//        _content = textureManager.GetContentSel();
//        _style = GetTexture2D(styleGO);
//        // style = ResizeTexture(style);
//        //print(_content.IsUnityNull());
//        //print(_contentTensor.IsUnityNull());
//        //print(_transform.IsUnityNull());
//        TextureConverter.ToTensor(_content, _contentTensor, _transform);
//        TextureConverter.ToTensor(_style, _styleTensor, _transform);
//        _worker.SetInput("content", _contentTensor);
//        _worker.SetInput("style", _styleTensor);

//        _worker.Schedule();

//        _output = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
//        _output.Create();

//        _outputTensor = _worker.PeekOutput("output") as Tensor<float>;
//        TextureConverter.RenderToTexture(_outputTensor, _output);
//        textureManager.SetContentST(_output);

//        _contentTensor.Dispose();
//        _styleTensor.Dispose();
//        _outputTensor.Dispose();
//    }

//    void StyleChanged()
//    {
//        if (triggerButton.reference.action.IsPressed()) // 클릭
//        {
//            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
//            {
//                GameObject clickedGO = hit.collider.gameObject;
//                if (clickedGO.layer.Equals(LayerMask.NameToLayer("StyleImage"))) // clickedGO.GetInstanceID() != styleGO.GetInstanceID()
//                {
//                    if (styleGO == null || styleGO.GetInstanceID() != clickedGO.GetInstanceID())
//                    {
//                        styleGO = clickedGO;
//                        Debug.Log($"Style texture changed to: {styleGO.name}");
//                        execute();
//                        Debug.Log("Output image updated");
//                    }
//                }
//            }
//        }
//    }

//    private Texture2D GetTexture2D(GameObject go)
//    {
//        if (go == null)
//            return null;

//        Renderer rend = go.GetComponent<Renderer>();
//        if (rend != null && rend.material != null && rend.material.mainTexture is Texture2D)
//        {
//            return rend.material.mainTexture as Texture2D;
//        }

//        return null;
//    }

//    private Texture2D ResizeTexture(Texture2D source, int width = 512, int height = 512)
//    {
//        // RenderTexture를 임시로 생성
//        RenderTexture rt = RenderTexture.GetTemporary(width, height);
//        Graphics.Blit(source, rt);

//        // RenderTexture를 읽어 Texture2D로 변환
//        RenderTexture.active = rt;
//        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
//        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//        result.Apply();

//        // 리소스 정리
//        RenderTexture.active = null;
//        RenderTexture.ReleaseTemporary(rt);

//        return result;
//    }


//    private void Awake()
//    {
//        Set();
//    }

//    private void Update()
//    {
//        StyleChanged();
//    }

//    private void OnDestroy()
//    {
//        _worker.Dispose();
//    }
//}
