using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI를 다루기 위해 필수
public class Turorial1 : MonoBehaviour
{
    [Header("UI 연결")]
    public Image displayImage;      // 중앙에 이미지가 뜰 Image 컴포넌트
    public List<Sprite> imageList;  // 1번~12번 이미지를 담을 리스트

    private int currentIndex = 0;   // 현재 보고 있는 이미지 번호 (0부터 시작)

    void Start()
    {
        // 시작하자마자 첫 번째 이미지 보여주기
        UpdateUI();
    }

    // 왼쪽 버튼(Previous)에 연결할 함수
    public void OnPrevClick()
    {
        currentIndex--;

        // 0번(첫번째)보다 작아지면, 맨 마지막 번호로 보냄 (순환)
        if (currentIndex < 0)
        {
            currentIndex = imageList.Count - 1;
        }

        UpdateUI();
    }

    // 오른쪽 버튼(Next)에 연결할 함수
    public void OnNextClick()
    {
        currentIndex++;

        // 리스트 개수(마지막)를 넘어가면, 다시 0번으로 보냄 (순환)
        if (currentIndex >= imageList.Count)
        {
            currentIndex = 0;
        }

        UpdateUI();
    }

    // 실제 화면의 이미지를 교체하는 함수
    private void UpdateUI()
    {
        if (imageList.Count > 0 && displayImage != null)
        {
            displayImage.sprite = imageList[currentIndex];

            // (선택사항) 디버깅용 로그
            // Debug.Log($"현재 이미지: {currentIndex + 1} / {imageList.Count}");
        }
    }
}