using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadImageButton : MonoBehaviour, IPointerClickHandler
{
    public Image image;
    public Texture2D texture;
    public string str;

    public void OnPointerClick(PointerEventData eventData)
    {
        string[] path = SFB.StandaloneFileBrowser.OpenFilePanel("Open Image", "", "", false);
        Load("file://" + path[0]);
    }

    public void Load(string url)
    {
        StartCoroutine(DownloadImage(url));
    }

    public IEnumerator DownloadImage(string downloadUrl)
    {
        var www = new WWW(downloadUrl);

        yield return www;

        texture = new Texture2D(200, 200);

        www.LoadImageIntoTexture(texture);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        str = Convert.ToBase64String(texture.EncodeToPNG());
        PlayerPrefs.SetString("img", str);
    }
}
