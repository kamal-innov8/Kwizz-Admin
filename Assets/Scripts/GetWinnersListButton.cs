using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GetWinnersListButton : MonoBehaviour, IPointerClickHandler
{
    public InputField date;
    public void OnPointerClick(PointerEventData eventData)
    {
        new GameSparkRequests("GetWinners").Add("date", date.text).Request(GotWinnersList);
    }

    private void GotWinnersList(string str)
    {
        GSWinnersListResult result = JsonUtility.FromJson<GSWinnersListResult>(str);
        string text = string.Empty;
        foreach (var item in result.scriptData.result)
        {
            text += item.id.ToString() + "     " + item.phone + Environment.NewLine;
        }
        DirectoryInfo info = new DirectoryInfo(Application.dataPath);
        File.WriteAllText(info.Parent + "/" + date.text.Replace("/", " ") + " winners" + ".txt", text);
        Popup.Instance.DisplayMessage("Winners list created!");
    }
}