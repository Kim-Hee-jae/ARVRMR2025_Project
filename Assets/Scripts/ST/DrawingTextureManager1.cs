using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class DrawingTextureManager1 : MonoBehaviour
{
    [Header("Target Renderer (optional)")]
    public Renderer targetRenderer;

    // 텍스처
       
    
    private Texture _content;
    private Texture _sdContent;
    private Texture _stOutput;
    private Texture _mask;

    private Texture _contentCurrent;
    private Texture _stOutputLast;

    private bool _hasStyle;
    public bool HasStyle => _hasStyle;
    private bool _styleChanged;


    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
        _contentCurrent = null;
        _stOutput = null;

        _hasStyle = false;
        _styleChanged = false;
    }

    public void SetContentTexture(Texture content)
    {
        _content = content;
        _contentCurrent = content;
        targetRenderer.material.mainTexture = content;
    }

    public void SetSDContentTexture(Texture sdContent)
    {
        _sdContent = sdContent;
        _contentCurrent = sdContent;
        targetRenderer.material.mainTexture = sdContent;
    }

    // 일단 diffusion -> style transfer 단방향만 허용
    public void SetSTOutputTexture(Texture stOutput)
    {
        if (_hasStyle)
            _styleChanged = true;
        _hasStyle = true;
        
        _stOutputLast = _stOutput;
        _stOutput = stOutput;
        // _contentCurrent = stOutput;
        targetRenderer.material.SetTexture("_StyleTex", stOutput);
    }

    public void SetMaskTexture(Texture mask)
    {
        targetRenderer.material.SetTexture("_MaskTex", mask);
    }

    public Texture GetCurrentContentTexture()
    {
        if (_contentCurrent == null)
        {
            _contentCurrent = targetRenderer.material.mainTexture;
        }
        return _contentCurrent;
    }

    public Texture GetSDContentTexture()
    {
        return _sdContent;
    }

    public Texture GetSTOutputTexture()
    {
        if (_stOutput == null)
        {
            return null;
        }
        return _stOutput;
    }

    public Texture GetLastSTOutputTexture()
    {
        if (_stOutputLast == null)
            print("is nulllllllll");

        return _stOutputLast;
    }

    public bool StyleChanged()
    {
        bool value = _styleChanged;
        _styleChanged = false;
        return value;
    }
}

    //public void SetTexture(Texture texture, int mode, bool apply=false)
    //{
    //    if (mode == 0)
    //    {
    //        _content = texture;
    //        _sdOutput = texture;
    //    }
    //    else if (mode == 1)
    //        _sdOutput = texture;
    //    else if (mode == 2)
    //        _stOutput = texture;

    //    if (apply)
    //    {
    //        targetRenderer.material.mainTexture = texture;
    //        print("Texture Changed");
    //    }
    //}

    //public Texture GetTexture(int mode=-1)
    //{
    //    if (mode == 0)
    //    {
    //        SetTexture(targetRenderer.material.mainTexture, 0);
    //        return _content;
    //    }
    //    else if (mode == 1)
    //        return _sdOutput;
    //    else if (mode == 2)
    //        return _stOutput;
    //    else
    //        return targetRenderer.material.mainTexture;
    //}
