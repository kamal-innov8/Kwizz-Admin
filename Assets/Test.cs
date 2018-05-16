using System.Collections;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Image image;

    [ContextMenu("open")]
    public void OpenFile()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open Images", "", "", true);
        StartCoroutine(DownloadImage(image, "file://" + paths[0]));
    }

    public IEnumerator DownloadImage(Image image, string downloadUrl)
    {
        var www = new WWW(downloadUrl);

        yield return www;

        var tex = new Texture2D(200, 200);

        www.LoadImageIntoTexture(tex);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }
}
