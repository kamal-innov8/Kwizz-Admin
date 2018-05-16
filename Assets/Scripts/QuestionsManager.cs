using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsManager : Singleton<QuestionsManager>
{
    public InputField date;
    public Dropdown questionType;
    public List<QuestionElement> questionElementInstances;
    public QuestionElement[] questionPrefabs;
    public Transform questionsContainer;
    public List<Question> questions;
    public bool dontReset;
    int savedQuestions;
    public GameObject loading;

    private void Start()
    {
        GS.GameSparksAuthenticated += OnGameSparksLogin;
        if (date != null)
        {
            date.onEndEdit.AddListener(DateChanged);
        }
        EventManager.Instance.OnQuestionClose += CloseQuestion;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnQuestionClose -= CloseQuestion;
    }

    private void OnGameSparksLogin(string obj)
    {
        if (date != null)
        {
            date.text = DateTime.Today.ToString("dd/MM/yyyy");
            DateChanged(date.text);
        }
    }

    private void DateChanged(string arg0)
    {
        if (!dontReset)
            ResetQuestions();
        GetQuestions();
    }

    private void GetQuestions()
    {
        new GameSparkRequests("GetAllQuestions").Add("date", date.text).Request(GotQuestionsCallback);
    }

    private void GotQuestionsCallback(string str)
    {
        ResetQuestions();
        GSListResult result = JsonUtility.FromJson<GSListResult>(str);
        foreach (var item in result.scriptData.result)
        {
            Question qn = JsonUtility.FromJson<Question>(item);
            questions.Add(qn);
        }
        questions = questions.OrderBy(a => a.index).ToList();
        foreach (var item in questions)
        {
            QuestionElement instance = CreateQuestion((int)item.type);
            instance.Setup(item);
        }
    }

    public void CreateQuestion()
    {
        var instance = CreateQuestion(questionType.value);
        instance.GenerateCode();
    }

    private QuestionElement CreateQuestion(int type)
    {
        QuestionElement instance = Instantiate(questionPrefabs[type]);
        instance.transform.SetParent(questionsContainer);
        instance.Question.index = questionElementInstances.Count;
        questionElementInstances.Add(instance);
        return instance;
    }

    public void RemoveQuestion(QuestionElement obj)
    {
        questionElementInstances.Remove(obj);
        Destroy(obj.gameObject);
    }

    public void SaveQuestions()
    {
        savedQuestions = 0;
        loading.SetActive(true);
        questions = questionElementInstances.Select(a => a.Question).ToList();
        var textQuestion = questionElementInstances.Where(a => (a.Question.type == QuestionType.text)).ToList().Select(a => a.Question).ToList();
        foreach (var item in textQuestion)
        {
            string json = JsonUtility.ToJson(item);
            new GameSparkRequests("SaveQuestion").Add("index", item.index).Add("code", item.code).Add("data", json).Add("answer", item.correctIndex).Add("date", date.text).Request(SaveQuestionsSuccess);
        }
        var imageQuestion = questionElementInstances.Where(a => a.Question.type == QuestionType.image).ToList();
        foreach (var item in imageQuestion)
        {
            StartCoroutine(WaitAndUpload(item));
        }
    }

    IEnumerator WaitAndUpload(QuestionElement item)
    {
        item.UploadScreenShot();
        while (!item.UploadComplete())
        {
            yield return null;
        }
        string json = JsonUtility.ToJson(item.Question);
        new GameSparkRequests("SaveQuestion").Add("index", item.Question.index).Add("code", item.Question.code).Add("data", json).Add("answer", item.Question.correctIndex).Add("date", date.text).Request(SaveQuestionsSuccess);
    }

    private void SaveQuestionsSuccess(string str)
    {
        savedQuestions++;
        if (savedQuestions == questions.Count)
        {
            Popup.Instance.DisplayMessage("Questions saved.");
            loading.SetActive(false);
        }

    }

    public void ResetQuestions()
    {
        foreach (var item in questionElementInstances.ToList())
        {
            Destroy(item.gameObject);
        }
        questionElementInstances.Clear();
        questions.Clear();
    }

    public void CloseQuestion(Question question)
    {
        new GameSparkRequests("CloseQuestion").Add("code", question.code).Request(CloseClosedSuccessfully);
    }

    private void CloseClosedSuccessfully(string str)
    {
        GSResult result = JsonUtility.FromJson<GSResult>(str);
        if (result.scriptData.result == "open")
        {
            Popup.Instance.DisplayMessage("Question opened.");
        }
        else if (result.scriptData.result == "closed")
        {
            Popup.Instance.DisplayMessage("Question closed.");
        }
    }
}
