using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 기능을 위해 추가

public class Tutorial2 : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject menuCanvas;
    public Image displayImage;
    public List<Sprite> slideImages;

    [Header("VR Settings")]
    public Transform cameraTransform;
    public float spawnDistance = 1.0f;
    public float heightOffset = -0.2f;

    private int currentIndex = 0;

    void Start()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
            // Start에서는 초기화만 하고 위치는 Update에서 계속 잡습니다.
        }
        UpdateUI();
    }

    // ★ 매 프레임마다 실행되는 Update 함수 추가
    void Update()
    {
        // 메뉴가 켜져 있을 때만 카메라 앞을 따라다님
        if (menuCanvas != null && menuCanvas.activeSelf)
        {
            PositionMenuInFront();
        }
    }

    public void OnNextClick()
    {
        currentIndex++;
        if (currentIndex >= slideImages.Count)
        {
            CloseMenu();
        }
        else
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (displayImage != null && currentIndex < slideImages.Count)
        {
            displayImage.sprite = slideImages[currentIndex];
        }
    }

    private void CloseMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
    }

    private void PositionMenuInFront()
    {
        if (cameraTransform == null) return;

        Vector3 forwardDirection = cameraTransform.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        // 부드럽게 따라오게 하려면 Vector3.Lerp를 쓰지만, 지금은 즉시 따라오게 설정함
        Vector3 targetPosition = cameraTransform.position + (forwardDirection * spawnDistance) + new Vector3(0, heightOffset, 0);

        if (menuCanvas != null)
        {
            menuCanvas.transform.position = targetPosition;

            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
            menuCanvas.transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }
}