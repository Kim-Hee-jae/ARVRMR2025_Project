using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class StageManager : MonoBehaviour
{
    [Header("References")]
    public Renderer sourceRenderer;
    public DrawingTextureManager1 textureManager;
    public MaskDrawingManager maskManager;
    public SDImg2ImgClient sdManager;

    public Button buttonS1;
    public Button buttonS2;
    public Button buttonS3;
    public Button buttonS4;

    public Button buttonFinish;
    public Toggle toggleUseSD;
    public GameObject loadingIcon;

    private DateTime startTime;
    private int stage = 1;


    public void OnStage1Click()
    {
        if (sourceRenderer != null & textureManager != null & maskManager != null)
        {
            // 1 -> 2
            if (textureManager.gameObject.activeSelf)
            {
                sourceRenderer.gameObject.SetActive(true);
                textureManager.gameObject.SetActive(false);

                buttonS3.interactable = false;
                buttonS4.interactable = false;
            }
            maskManager.hasStyle = false;
        }
        stage = 1;
    }

    public void OnStage2Click()
    {
        if (sourceRenderer != null & textureManager != null & maskManager != null)
        {
            // 1 -> 2
            if (!textureManager.gameObject.activeSelf)
            {
                sourceRenderer.gameObject.SetActive(false);
                textureManager.gameObject.SetActive(true);
                textureManager.SetUserDrawing(sourceRenderer.material.mainTexture);
            }
            // 2,3,4 -> 2
            else
            {
                if (stage==3)
                    textureManager.ChangeStyle(-1);
                textureManager.DisplayUserDrawing();
            }
            maskManager.hasStyle = false;
        }
        stage = 2;
    }

    public void OnStage3Click()
    {
        if (sourceRenderer != null & textureManager != null & maskManager != null)
        {
            maskManager.hasStyle = true;
            textureManager.Resume();
        }
        stage = 3;
    }

    public void OnStage4Click()
    {
        if (sourceRenderer != null & textureManager != null & maskManager != null)
        {
            if (stage == 3)
                textureManager.ChangeStyle(-1);
            maskManager.hasStyle = false;
        }
        stage = 4;
    }

    public void OnFinishClick()
    {

        // 1. 버튼 비활성화
        buttonS1.interactable = false;
        buttonS2.interactable = false;
        buttonS3.interactable = false;
        buttonS4.interactable = false;

        buttonFinish.interactable = false;
        toggleUseSD.interactable = false;

        // 2. 로딩 UI 표시
        if (loadingIcon != null)
            loadingIcon.SetActive(true);

        // 3. 클라이언트 전송 + 완료 시 버튼 다시 활성화
        StartCoroutine(SendAndWait());
    }

    private IEnumerator SendAndWait()
    {
        print("Response sent, button disabled.");
        startTime = DateTime.Now;

        // runStableDiffusion 플래그를 그대로 넘겨서
        // 서버 쪽 use_sd 를 true/false로 설정
        yield return StartCoroutine(
            sdManager.SendAndReceiveWithCallback(
                textureManager.GetUserDrawing(),
                OnResponseReceived,
                runSD: toggleUseSD.isOn
            )
        );
    }

    private void OnResponseReceived(List<Texture2D> stResults, Texture2D sdResult)
    {
        TimeSpan duration = DateTime.Now - startTime;

        // 4. 응답 수신 후 버튼 다시 활성화
        buttonS1.interactable = true;
        buttonS2.interactable = true;
        buttonS3.interactable = true;
        buttonS4.interactable = true;

        buttonFinish.interactable = true;
        toggleUseSD.interactable = true;

        // 5. 로딩 UI 숨김
        if (loadingIcon != null)
            loadingIcon.SetActive(false);

        textureManager.SetStyleTransferResults(stResults, sdResult);

        print(duration.TotalSeconds + "초");
        print("Response received, button re-enabled.");
    }
}