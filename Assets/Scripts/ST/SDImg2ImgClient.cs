using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SDImg2ImgClient : MonoBehaviour
{
    [Header("Apply Result")]
    public DrawingTextureManager textureManager;

    [Header("Server")]
    public string inferUrl = "http://127.0.0.1:5000/infer";
    public int timeoutSec = 180;

    [Header("Prompts")]
    [TextArea] public string staticPrompt = "ultra-detailed, high quality";
    [TextArea] public string dynamicPrompt = "a cute robot on a desk";

    [Header("Params")]
    [Range(0f, 1f)] public float strength = 0.5f; // 원본 보존(낮음) ↔ 변형(높음)
    public float guidanceScale = 0.0f;           // turbo는 낮게
    [Range(1, 8)] public int steps = 4;          // turbo는 저스텝 가능
    public int seed = -1;                        // <0 이면 무시
    public int width = 512, height = 512;            // 0이면 원본 해상도 유지


    public void Send(Texture source)
    {
        StartCoroutine(SendAndReceive(source));
    }

    IEnumerator SendAndReceive(Texture source)
    {
        // Texture -> PNG
        Texture2D readable = ToReadableTexture2D(source);
        byte[] pngBytes = readable.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddField("static_prompt", staticPrompt);
        form.AddField("dynamic_prompt", dynamicPrompt);
        form.AddField("strength", strength.ToString(System.Globalization.CultureInfo.InvariantCulture));
        form.AddField("guidance_scale", guidanceScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
        form.AddField("steps", steps.ToString());
        if (seed >= 0) form.AddField("seed", seed.ToString());
        if (width > 0) form.AddField("width", width.ToString());
        if (height > 0) form.AddField("height", height.ToString());

        form.AddBinaryData("image", pngBytes, "input.png", "image/png");

        using (UnityWebRequest req = UnityWebRequest.Post(inferUrl, form))
        {
            req.timeout = timeoutSec;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[infer] {req.responseCode} {req.error}\n{req.downloadHandler.text}");
                yield break;
            }

            byte[] outBytes = req.downloadHandler.data;
            Texture2D outTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!outTex.LoadImage(outBytes))
            {
                Debug.LogError("[infer] Failed to decode image");
                yield break;
            }

            textureManager.SetContentSD(outTex);

            Debug.Log("[infer] Success");
            req.Dispose();
        }
    }

    // 읽기 불가 텍스처/RenderTexture -> 읽기 가능한 Texture2D
    Texture2D ToReadableTexture2D(Texture src)
    {
        if (src is Texture2D t2d && t2d.isReadable) return t2d;

        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);

        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return readable;
    }

    public IEnumerator SendAndReceiveWithCallback(Texture source, System.Action onDone = null)
    {
        Texture2D readable = ToReadableTexture2D(source);
        byte[] pngBytes = readable.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddField("static_prompt", staticPrompt);
        form.AddField("dynamic_prompt", dynamicPrompt);
        form.AddField("strength", strength.ToString(System.Globalization.CultureInfo.InvariantCulture));
        form.AddField("guidance_scale", guidanceScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
        form.AddField("steps", steps.ToString());
        form.AddBinaryData("image", pngBytes, "input.png", "image/png");

        using (UnityWebRequest req = UnityWebRequest.Post(inferUrl, form))
        {
            req.timeout = timeoutSec;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[infer] {req.responseCode} {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                byte[] outBytes = req.downloadHandler.data;
                Texture2D outTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (outTex.LoadImage(outBytes))
                    textureManager.SetContentSD(outTex);

                Debug.Log("[infer] Success");
            }
            req.Dispose();
        }

        // 요청이 끝난 후 콜백 호출
        onDone?.Invoke();
    }

}
