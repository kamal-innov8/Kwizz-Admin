using System;
using UnityEngine;
using UnityEngine.UI;

public class User : MonoBehaviour
{
    public InputField userName, pwd, phoneNumber, code;
    public GameObject loginPage, savePhoneNumberPage, appStartPage, enterCodePage, questionPage;
    public QuestionsPage questionsPage;
    public static User instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        GameSparksManager.authenticated += OnGameSparksLogin;
    }

    private void OnGameSparksLogin()
    {
        CheckPhoneNumber();
    }

    private void CheckPhoneNumber()
    {
        new GameSparkRequests("GetUserData").Request(GetUserDataCallback);
    }

    public void OpenGame()
    {
        loginPage.SetActive(false);
        appStartPage.SetActive(false);
        questionPage.SetActive(false);
        enterCodePage.SetActive(true);
        code.text = string.Empty;
        if (Application.isEditor)
        {
            TestHelper.Instance.SetCode();
        }
    }

    private void GetUserDataCallback(string str)
    {
        if (!str.Contains("Error"))
        {
            OpenGame();
        }
        else
        {
            savePhoneNumberPage.SetActive(true);
        }
    }

    public void UserNameLogin()
    {
        GameSparksManager.instance.Login(userName.text, pwd.text);
    }

    public void SavePhoneNumber()
    {
        UserData userData = new UserData();
        userData.phone = phoneNumber.text;
        new GameSparkRequests("SaveUserData").Add("data", JsonUtility.ToJson(userData)).Request(SavePhoneNumberCallback);
    }

    private void SavePhoneNumberCallback(string str)
    {
        if (!str.Contains("Error"))
        {
            savePhoneNumberPage.SetActive(false);
            appStartPage.SetActive(true);
        }
        else
        {
            print("Save failed!");
        }
    }

    public void GetQuestions()
    {
        new GameSparkRequests("GetQuestion").Add("code", code.text).Add("date", DateTime.Today.AddDays(ErrorCodes.daysOffset).ToString("dd/MM/yyyy")).Request(GetQuestionsSuccessCallback, GetQuestionsFailedCallback);
    }

    private void GetQuestionsFailedCallback(string str)
    {
        GSError result = JsonUtility.FromJson<GSError>(str);
        if (result.error.Status == "already answered")
        {
            Popup.Instance.DisplayMessage("Already answered. Wait for next question.");
        }
        if (result.error.Status == ErrorCodes.noteligible)
        {
            Popup.Instance.DisplayMessage("Contest already halfway. Please try again tomorrow.");
        }
        if (result.error.Status == "already lost")
        {
            Popup.Instance.DisplayMessage("Already lost. Please try again tomorrow.");
        }
        if (result.error.Status == ErrorCodes.close)
        {
            print("question is already closed!");
        }
        if (result.error.Status == ErrorCodes.failed)
        {
            Popup.Instance.DisplayMessage("Incorrect code. Please check again.");
        }
    }

    private void GetQuestionsSuccessCallback(string str)
    {
        enterCodePage.SetActive(false);
        questionPage.SetActive(true);
        questionsPage.Setup(JsonUtility.FromJson<Question>(JsonUtility.FromJson<GSResult>(str).scriptData.result));
    }

    public void Logout()
    {
        loginPage.SetActive(true);
        questionPage.SetActive(false);
    }
}