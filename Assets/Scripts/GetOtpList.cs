using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetOtpList : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var codes = QuestionsManager.Instance.questions.OrderBy(a => a.index).Select(a => a.code).ToList();
        var answers = QuestionsManager.Instance.questions.OrderBy(a => a.index).Select(a => a.correctIndex + 1).ToList();
        string text = string.Empty;
        for (int i = 0; i < codes.Count; i++)
        {
            text += codes[i] + "     " + answers[i] + Environment.NewLine;
        }
        DirectoryInfo info = new DirectoryInfo(Application.dataPath);
        File.WriteAllText(info.Parent + "/" + @QuestionsManager.Instance.date.text.Replace("/", " ") + " OTP.txt", text);
        Popup.Instance.DisplayMessage("OTP list created!");
    }
}