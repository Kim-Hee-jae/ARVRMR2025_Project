using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class DrawingTextureManager1 : MonoBehaviour
{
    [Header("Target Renderer (optional)")]
    public Renderer targetRenderer;
    public MaskDrawingManager maskManager;

    // ≈ÿΩ∫√≥
    public Texture _contentOri;
    public Texture _contentBase;
    public Texture _contentCur;

    public List<Texture2D> _styleList;
    public Texture _style;


    private void Awake()
    {
        InitAll();
    }

    private void OnEnable()
    {
        InitAll();
    }

    private void Update()
    {
        if (maskManager.mode == 1) ChangeStyle(-1);
        targetRenderer.material.SetTexture("_MaskTex", maskManager.maskTex);
    }

    private void InitAll()
    {
        _contentOri = null;
        _contentBase = null;
        _contentCur = null;
        _styleList = null;
        _style = null;

        targetRenderer.material.mainTexture = null;
        targetRenderer.material.SetTexture("_StyleTex", null);
        maskManager.ResetMask();
        targetRenderer.material.SetTexture("_MaskTex", maskManager.maskTex);
    }

    // On 1 -> 2 Click
    public void SetUserDrawing(Texture content)
    {
        // set user image
        _contentOri = content;
        targetRenderer.material.mainTexture = content;
    }

    // On 2,3,4 -> 2 Click
    public void DisplayUserDrawing()
    {
        ////// Reset _contentCur
        targetRenderer.material.mainTexture = _contentOri;
    }

    // On Finish Clik
    public Texture GetUserDrawing()
    {
        return _contentOri;
    }

    public Texture GetOuput()
    {
        return _contentCur;
    }

    public void Resume()
    {
        targetRenderer.material.mainTexture = _contentCur;
    }

    // On Finish Recieve
    public void SetStyleTransferResults(List<Texture2D> stResults, Texture2D sdResult)
    {
        if (sdResult != null)
            _contentBase = sdResult;
        else
            _contentBase = _contentOri;
        _contentCur = _contentBase;

        _styleList = stResults;
        _style = _contentBase;

        targetRenderer.material.mainTexture = _contentCur;
        ChangeStyle(-1);
    }

    public void ChangeStyle(int idx)
    {
        SendUpdateRequest();
        if (idx == -1)
        {
            _style = _contentBase;
        }
        else
        {
            _style = _styleList[idx];
        }
        targetRenderer.material.SetTexture("_StyleTex", _style);
    }

    public void SendUpdateRequest()
    {
        _contentCur = maskManager.SaveOutput(_contentCur, _style);
        targetRenderer.material.mainTexture = _contentCur;
        maskManager.ResetMask();
    }
}
