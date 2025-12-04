using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class SendTextureToFlask_origin : MonoBehaviour
{
    public GameObject sketchObject;     // 텍스처를 보낼 대상
    public string flaskUrl = "https://gawon.store/origin_upload";  // Flask 서버 URL

    public void OnButtonClick()
    {
        StartCoroutine(SendImage());
    }

    IEnumerator SendImage()
    {
        Texture2D texture = (Texture2D)sketchObject.GetComponent<Renderer>().material.mainTexture;


        //Texture2D texture = (Texture2D)sketchObject.GetComponent<Renderer>().material.mainTexture;

        Texture2D flipped = new Texture2D(texture.width, texture.height);
        for (int y = 0; y < texture.height; y++)
        {
            flipped.SetPixels(0, y, texture.width, 1,
                texture.GetPixels(0, texture.height - y - 1, texture.width, 1));
        }
        flipped.Apply();

        byte[] bytes = flipped.EncodeToPNG();


        //byte[] bytes = texture.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes, "upload.png", "image/png");

        UnityWebRequest request = UnityWebRequest.Post(flaskUrl, form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("원본 이미지 전송 성공");
        }
    }
}