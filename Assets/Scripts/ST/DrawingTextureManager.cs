using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI.MessageBox;

[RequireComponent(typeof(Renderer))]
public class DrawingTextureManager : MonoBehaviour
{
    [Header("Target Renderer (optional)")]
    public Renderer targetRenderer;

    // ÅØ½ºÃ³
    private Texture _contentOri;
    private Texture _contentSD;
    private Texture _contentSel;

    private Texture _contentST;
    private Texture _contentSTp;

    public Texture _contentMat;

    private bool _hasStyle;
    public bool HasStyle => _hasStyle;
    private bool _contentChanged;
    private bool _contentChanged2;
    private bool _styleChanged;
    

    private void Awake()
    {
        _contentOri = null;
        _contentSD = null;
        _contentSel = null;
        _contentST = null;
        _contentSTp = null;
        _contentMat = null;

        _hasStyle = false;
        _styleChanged = false;
        _contentChanged = false;
        _contentChanged2 = false;
    }

    public void SetContentOri(Texture content)
    {
        _contentOri = content;
        _contentSel = _contentOri;

        SetMaterialContent(_contentSel);
    }

    public void SetContentSD(Texture content)
    {
        _contentSD = content;
        _contentSel = _contentSD;

        SetMaterialContent(_contentSel);
    }

    public void SetContentST(Texture content)
    {
        _contentSTp = _contentST;
        _contentST = content;

        if (_hasStyle)
            _styleChanged = true;
        else
            SetMaterialStyle(_contentST);

        _hasStyle = true;
    }

    public void SetMaterialContent_(Texture output)
    {
        SetMaterialContent(output);
        SetMaterialStyle(_contentST);
    }

    public void SetForErase()
    {
        
    }

    public void SetForNewContent()
    {
        SetMaterialContent(_contentSel);
        SetMaterialStyle(null);
        SetMaterialMask(null);

        _hasStyle = false;
        _styleChanged = false;
        _contentChanged = true;
        _contentChanged2 = true;
    }

    public void SetOutput(Texture output)
    {
        _contentMat = output;
        SetMaterialContent(_contentMat);
    }

    public Texture GetContentSel()
    {
        return _contentSel;
    }

    public Texture GetContentST()
    {
        return _contentST;
    }

    public Texture GetContentSTp()
    {
        return _contentSTp;
    }
    public Texture GetMaterialContent()
    {
        // return targetRenderer.material.GetTexture("_MainTex");
        return targetRenderer.material.mainTexture;
    }
    public Texture GetOutput()
    {
        return _contentMat;
    }
    // Set
    // Material Texture
    private void SetMaterialContent(Texture content)
    {
        targetRenderer.material.mainTexture = content;
    }

    private void SetMaterialStyle(Texture style)
    {
        targetRenderer.material.SetTexture("_StyleTex", style);
    }


    public void SetMaterialMask(Texture mask)
    {
        targetRenderer.material.SetTexture("_MaskTex", mask);
    }


    public bool StyleChanged()
    {
        bool value = _styleChanged;
        _styleChanged = false;
        return value;
    }

    public bool ContentChanged()
    {
        bool value = _contentChanged;
        _contentChanged = false;
        return value;
    }

    public bool ContentChanged2()
    {
        bool value = _contentChanged2;
        _contentChanged2 = false;
        return value;
    }
}
