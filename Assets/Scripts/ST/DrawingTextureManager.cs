using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class DrawingTextureManager : MonoBehaviour
{
    [Header("Target Renderer (optional)")]
    public Renderer targetRenderer;

    // 텍스처 히스토리 (런타임 중간 결과들)
    private Texture _content; // 0
    private Texture _sdOutput; // 1
    private Texture _stOutput; // 2


    private readonly List<Texture> _history = new List<Texture>();
    private int _currentIndex = -1;
    private int _contentIndex = -1;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // GetCurrentTexture()가 요청될 때 최초 저장
    }


    public void SetTexture(Texture texture, int mode, bool apply=false)
    {
        if (mode == 0)
        {
            _content = texture;
            _sdOutput = texture;
        }
        else if (mode == 1)
            _sdOutput = texture;
        else if (mode == 2)
            _stOutput = texture;

        if (apply)
        {
            targetRenderer.material.mainTexture = texture;
            print("Texture Changed");
        }
    }

    public Texture GetTexture(int mode=-1)
    {
        if (mode == 0)
        {
            SetTexture(targetRenderer.material.mainTexture, 0);
            return _content;
        }
        else if (mode == 1)
            return _sdOutput;
        else if (mode == 2)
            return _stOutput;
        else
            return targetRenderer.material.mainTexture;
    }

    /// <summary>
    /// 현재 텍스처를 반환.
    /// - 최초 호출 시, renderer.material.mainTexture를 history[0]으로 스냅샷하고 반환.
    /// - 이후 호출 시, history[currentIndex] 반환.
    /// </summary>
    public Texture GetCurrentTexture(bool isStyleTransfer=false)
    {
        EnsureInitialSnapshot();

        if (_currentIndex < 0 || _currentIndex >= _history.Count)
        {
            print("Unexpected Call");
            return null;
        }
        print(targetRenderer.material.mainTexture != _history[_currentIndex]);
        return targetRenderer.material.mainTexture;
        return _history[_currentIndex];
        if (isStyleTransfer)
        {
            print("1");
            if(_contentIndex == -1)
            {
                _contentIndex = _history.Count - 1; ;
            }
            return _history[_contentIndex];
        }
        else
            return _history[_currentIndex];
    }

    // 새 텍스처를 적용 + 히스토리에 추가.
    // - Undo 이후 새로운 브랜치가 생기면 그 뒤 히스토리는 제거.
    public void ApplyNewTexture(Texture tex)
    {
        if (tex == null)
        {
            Debug.LogWarning("ApplyNewTexture called with null texture.");
            return;
        }

        // 아직 한번도 history를 쓰지 않았다면,
        // 현재 material.mainTexture를 초기 상태로 캡처
        EnsureInitialSnapshot();

        // Undo 이후 새로 그리는 경우: 현재 인덱스 뒤의 히스토리는 버림
        if (_currentIndex < _history.Count - 1)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
        }

        // 새 텍스처를 history에 추가하고
        _history.Add(tex);
        _currentIndex = _history.Count - 1;

        // 실제 Renderer에 적용
        ApplyTextureToRenderer(tex);
    }

    // 현재 renderer 상태를 처음으로 history에 캡처해야 할 때만 한 번 수행.
    private void EnsureInitialSnapshot()
    {
        if (_history.Count > 0)
            return; // 이미 스냅샷 있음

        if (targetRenderer == null || targetRenderer.material == null)
            return;

        var tex = targetRenderer.material.mainTexture;
        if (tex == null)
            return; // 초기 텍스처가 없으면 히스토리 시작 안 함

        _history.Add(tex);
        _currentIndex = 0;
    }

    private void ApplyTextureToRenderer(Texture tex)
    {
        if (targetRenderer != null && targetRenderer.material != null)
        {
            targetRenderer.material.mainTexture = tex;
        }
    }

    // 필요 시 사용

    public bool CanUndo => _currentIndex > 0;
    public bool CanRedo => _currentIndex >= 0 && _currentIndex < _history.Count - 1;

    public void Undo()
    {
        if (!CanUndo)
        {
            Debug.Log("No more history to undo.");
            return;
        }

        _currentIndex--;
        ApplyTextureToRenderer(_history[_currentIndex]);
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            Debug.Log("No more history to redo.");
            return;
        }

        _currentIndex++;
        ApplyTextureToRenderer(_history[_currentIndex]);
    }
}
