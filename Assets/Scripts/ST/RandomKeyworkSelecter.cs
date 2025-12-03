using TMPro;
using UnityEngine;

public class DynamicKeywordPicker : MonoBehaviour
{
    public TextMeshPro text;
    public SDImg2ImgClient sdManager;

    [Header("후보 단어 리스트")]
    public string[] keywords =
    {
        "sunflower",
        "house",
        "tree",
        "lake",
        "cat",
        "cow",
    };

    [Header("현재 선택된 랜덤 키워드")]
    [SerializeField] private string currentKeyword;

    private void Start()
    {
        PickRandomKeyword();
        text.text = GetCurrentKeyword();
        sdManager.dynamicPrompt = GetCurrentKeyword();
    }

    public void PickRandomKeyword()
    {
        if (keywords == null || keywords.Length == 0)
        {
            Debug.LogWarning("키워드 리스트가 비어있습니다.");
            currentKeyword = "";
            return;
        }

        int index = Random.Range(0, keywords.Length);
        currentKeyword = keywords[index];

        Debug.Log($"[DynamicKeywordPicker] 새로운 랜덤 키워드 선택됨: {currentKeyword}");
    }

    public string GetCurrentKeyword()
    {
        return currentKeyword;
    }
}
