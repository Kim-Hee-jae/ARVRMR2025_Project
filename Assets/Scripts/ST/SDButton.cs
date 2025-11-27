using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class SDButton : MonoBehaviour
{
    [Header("References")]
    public Renderer sourceRenderer;
    public SDImg2ImgClient client; // 실제 전송 담당 스크립트
    public Button button;          // 연결된 UI 버튼 (Inspector에서 드래그)
    public Button buttonST;          // 화풍변환 UI 버튼
    public GameObject loadingIcon; // (선택) 로딩 스피너/텍스트 오브젝트

    private Texture source;         // 보낼 이미지 texture
    private DateTime startTime;


    public void OnClickSendWithoutSD()
    {
        // source 첫 세팅
        if (sourceRenderer != null & client != null)
        {
            if (!client.textureManager.gameObject.activeSelf)
            {
                sourceRenderer.gameObject.SetActive(false);
                client.textureManager.gameObject.SetActive(true);
                source = sourceRenderer.material.mainTexture;
                client.textureManager.SetContentOri(source);
            }
            else
            {
                client.textureManager.SetForNewContent();
            }
        }
    }


    public void OnClickSend()
    {

        // 1. 버튼 비활성화
        if (button != null)
        {
            button.interactable = false;
        }
        if (buttonST != null)
        {
            buttonST.interactable = false;
        }

        // 2. 로딩 UI 표시
        if (loadingIcon != null)
            loadingIcon.SetActive(true);

        // source 첫 세팅
        if (sourceRenderer != null & client != null)
        {
            if (!client.textureManager.gameObject.activeSelf)
            {
                sourceRenderer.gameObject.SetActive(false);
                client.textureManager.gameObject.SetActive(true);
                source = sourceRenderer.material.mainTexture;
                client.textureManager.SetContentOri(source);
            }
            else
            {
                client.textureManager.SetForNewContent();
            }
        }

        // 3. 클라이언트 전송 + 완료 시 버튼 다시 활성화
        StartCoroutine(SendAndWait());
    }

    private IEnumerator SendAndWait()
    {
        print("Response sent, button disabled.");
        startTime = DateTime.Now;
        // 코루틴으로 요청 완료를 기다림
        yield return StartCoroutine(client.SendAndReceiveWithCallback(source, OnResponseReceived));
    }

    private void OnResponseReceived()
    {
        TimeSpan duration = DateTime.Now - startTime;

        // 4. 응답 수신 후 버튼 다시 활성화
        if (button != null)
            button.interactable = true;
        if (buttonST != null)
            buttonST.interactable = true;

        // 5. 로딩 UI 숨김
        if (loadingIcon != null)
            loadingIcon.SetActive(false);

        print(duration.TotalSeconds + "초");
        print("Response received, button re-enabled.");
    }
}
