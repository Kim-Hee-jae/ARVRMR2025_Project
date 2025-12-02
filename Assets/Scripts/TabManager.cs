using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


[System.Serializable]
public class Tab
{
    public Button tabButton;    // 탭 역할을 할 버튼
    public GameObject contentPanel; // 버튼 클릭 시 활성화될 패널
}

public class TabManager : MonoBehaviour
{
    // 인스펙터 창에서 관리할 탭 리스트
    public List<Tab> tabs;

    void Start()
    {
        // 각 버튼에 클릭 이벤트를 동적으로 추가합니다.
        foreach (var tab in tabs)
        {
            // 람다식(lambda expression)을 사용하여 각 버튼이 자신의 짝이 되는 패널을 기억하도록 합니다.
            // 이렇게 하지 않으면 모든 버튼이 마지막 탭의 패널만 참조하게 되는 문제가 발생할 수 있습니다.
            tab.tabButton.onClick.AddListener(() => OnTabSelected(tab.contentPanel));
        }

        OnTabSelected(tabs[0].contentPanel); //  0번 인덱스 무조건 활성화ㅣ tool tab으로 설정
    }

    public void OnTabSelected(GameObject selectedPanel)
    {
        // 모든 패널을 우선 비활성화합니다.
        foreach (var tab in tabs)
        {
            if (tab.contentPanel != null)
            {
                tab.contentPanel.SetActive(false);
            }
        }

        // 선택된 패널만 활성화합니다.
        if (selectedPanel != null)
        {
            selectedPanel.SetActive(true);
        }
    }
}