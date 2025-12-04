using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class defaultStyleTransferManager : MonoBehaviour
{
    [Header("Model")]
    public NNModel AdaINModel;
    private Model m_RuntimeModel;

    [Header("Inputs")]
    public Texture2D content;
    public RenderTexture result;
    IWorker worker;
    public GameObject _rawstyle;
    RawImage rawImageTexture;

        public enum style_list
    {
        antimonocromatismo,
        asheville,
        brushstrokes,
        contrast_of_forms
    }
    public style_list set_style = style_list.antimonocromatismo;
    style_list prev_condition;

    Dictionary<string, Tensor> Inputs = new Dictionary<string, Tensor>();


    void Set()
    {
        m_RuntimeModel = ModelLoader.Load(AdaINModel);

        rawImageTexture = _rawstyle.GetComponent<RawImage>();
        set_style = style_list.antimonocromatismo;

        execute();
    }

    void execute()
    {
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
        Inputs = new Dictionary<string, Tensor>(); 
        Tensor t_content = new Tensor(content, 3);
        Tensor t_style = new Tensor(rawImageTexture.texture, 3);
        Inputs.Add("content", t_content);
        Inputs.Add("style", t_style);

        worker.Execute(Inputs);

        Tensor output = worker.PeekOutput("output");
        output.ToRenderTexture(result);

        t_content.Dispose();
        t_style.Dispose();
        output.Dispose();
    }

    void StyleChanged()
    {
        Texture2D antimonocromatismo = Resources.Load<Texture2D>("antimonocromatismo");
        Texture2D asheville = Resources.Load<Texture2D>("asheville");
        Texture2D brushstrokes = Resources.Load<Texture2D>("brushstrokes");
        Texture2D contrast_of_forms = Resources.Load<Texture2D>("contrast_of_forms");

        if (set_style == prev_condition)
        {
            Debug.Log("Not updated");
            return;
        }
        else if (set_style == style_list.antimonocromatismo)
        {
            rawImageTexture.texture = antimonocromatismo;
            prev_condition = style_list.antimonocromatismo;
            execute();
            Debug.Log("Updated");
        }
        else if (set_style == style_list.asheville)
        {
            rawImageTexture.texture = asheville;
            prev_condition = style_list.asheville;
            execute();
            Debug.Log("Updated");
        }
        else if (set_style == style_list.brushstrokes)
        {
            rawImageTexture.texture = brushstrokes;
            prev_condition = style_list.brushstrokes;
            execute();
            Debug.Log("Updated");
        }
    }

    private void Awake()
    {
        Set();
    }

    private void Update()
    {
        StyleChanged();
    }
}
