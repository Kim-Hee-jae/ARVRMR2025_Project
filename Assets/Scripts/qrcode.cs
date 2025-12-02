using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class SendTextureToFlask : MonoBehaviour
{
    public DrawingTextureManager textureManager;
    public GameObject sketchObject;
    public string flaskUrl = "http://gawon.store/upload";

    public void OnButtonClick()
    {
        StartCoroutine(SendImage());
    }

    IEnumerator SendImage()
    {
        Texture2D texture = (Texture2D)textureManager.GetOutput();
        byte[] bytes = texture.EncodeToPNG();

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
            Debug.Log("이미지 전송 성공");
        }
    }
}