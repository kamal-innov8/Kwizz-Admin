using System.Collections;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using UnityEngine;
using UnityEngine.UI;

public class QuestionElement : MonoBehaviour
{
    public Question Question;
    public InputField questionInput;
    public InputField[] inputFields;
    public Toggle[] toggleGroup;
    public LoadImageButton[] loadImageButtons;
    public CodeGenerator generator = new CodeGenerator();
    public Texture2D[] textures;
    private bool texturesLoaded;
    public string[] urls = new string[4];


    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            inputFields[i].onEndEdit.AddListener(OptionAdded);
        }
        foreach (var item in toggleGroup)
        {
            item.onValueChanged.AddListener(Toggled);
        }
        questionInput.onEndEdit.AddListener(QuestionAdded);
        UploadCompleteMessage.Listener += UploadComplete;
    }

    private void UploadComplete(UploadCompleteMessage obj)
    {
        var id = obj.BaseData.GetString("uploadId");
        print(id);
        for (int i = 0; i < urls.Length; i++)
        {
            if (urls[i].Contains(id))
            {
                Question.options[i] = id;
            }
        }
    }

    private void QuestionAdded(string arg0)
    {
        Question.question = arg0;
    }

    public void GenerateCode()
    {
        Question.code = generator.GenerateCode();
    }

    private void Toggled(bool arg0)
    {
        for (int i = 0; i < toggleGroup.Length; i++)
        {
            if (toggleGroup[i].isOn)
            {
                Question.correctIndex = i;
            }
        }
    }

    private void OptionAdded(string arg0)
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (Question.type == QuestionType.text)
            {
                Question.options[i] = inputFields[i].text;
            }
        }
    }

    public void Setup(Question question)
    {

        this.Question = question;
        questionInput.text = question.question;
        for (int i = 0; i < toggleGroup.Length; i++)
        {
            inputFields[i].text = question.options[i];
            if (question.correctIndex == i)
            {
                toggleGroup[i].isOn = true;
            }
        }
        for (int i = 0; i < loadImageButtons.Length; i++)
        {
            DownloadAFile(i);
        }
    }

    public bool UploadComplete()
    {
        foreach (var item in Question.options)
        {
            if (string.IsNullOrEmpty(item))
            {
                return false;
            }
        }
        return true;
    }

    public void LoadTextures()
    {
        for (int i = 0; i < loadImageButtons.Length; i++)
        {
            textures[i] = loadImageButtons[i].texture;
        }
        texturesLoaded = true;
    }

    public void UploadScreenShot()
    {
        if (Question.type != QuestionType.image)
        {
            return;
        }
        if (!texturesLoaded)
        {
            LoadTextures();
        }
        for (int i = 0; i < Question.options.Count; i++)
        {
            Request(i);
        }
    }

    void Request(int index)
    {
        new GetUploadUrlRequest().Send((response) =>
        {
            StartCoroutine(UploadAFile(index, response.Url));
            urls[index] = response.Url;
        });
    }

    public IEnumerator UploadAFile(int index, string url)
    {
        byte[] bytes = textures[index].EncodeToPNG();
        var form = new WWWForm();
        form.AddField("somefield", "somedata");
        form.AddBinaryData("file", bytes, "screenshot.png", "image/png");
        WWW w = new WWW(url, form);
        yield return w;

        if (w.error != null)
        {
            Debug.Log(w.error);
        }
        else
        {
            Debug.Log(w.text);
        }
    }

    public void DownloadAFile(int index)
    {
        new GetUploadedRequest().SetUploadId(Question.options[index]).Send((response) =>
        {
            loadImageButtons[index].Load(response.Url);
        });
    }
}
