using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Core;
using Michsky.MUIP;
using Newtonsoft.Json;
using UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
using SFB;

public class QuizCanvasManager : MonoBehaviour
{
    #region Singleton
    public static QuizCanvasManager Instance { get; private set; }
    #endregion

    #region UI References

    public GameObject surveyCanvas;
    public List<Toggle> toggles;
    public List<CustomInputField> inputFields;
    [SerializeField] private ButtonManager saveQuizButton;
    [SerializeField] private ButtonManager saveNameButton;
    [SerializeField] private GameObject userInputCanvas;
    [SerializeField] private TMP_InputField userNameInput;
    [SerializeField] private TMP_Text notificationText;
    [field: SerializeField] public ScrollRect ScrollRect { get; private set; }
    #endregion

    #region Data
    private string adventureName;
    private string userName;
    [SerializeField] private List<string> toggleQuestions = new List<string>();
    [SerializeField] private List<string> textQuestions = new List<string>();
    #endregion
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("UserName"))
        {
            userName = PlayerPrefs.GetString("UserName");
        }
        else
        {
            userName = "";
        }

        saveQuizButton.onClick.AddListener(HideSurvey);
        saveNameButton.onClick.AddListener(SaveUserName);
    }

    #region Survey Management
    public static void ShowSurvey(string adventureKey)
    {
        if (Instance == null)
        {
            Debug.LogError("QuizCanvasManager Instance is not found.");
            return;
        }

        Instance.adventureName = string.IsNullOrEmpty(adventureKey) ? "DefaultAdventure" : adventureKey;
        Instance.ClearSurveyAnswers();

        if (string.IsNullOrEmpty(Instance.userName))
        {
            Instance.userInputCanvas.SetActive(true);
        }
        else
        {
            Instance.surveyCanvas.SetActive(true);
            Instance.UpdateQuestions();
        }
        Time.timeScale = 0;

        GamepadCursor.DisplayCursor(true);
        GamepadCursor.SetCurrentScrollRect(Instance.ScrollRect, true);
    }

    private void ClearSurveyAnswers()
    {
        if(toggles != null && toggles.Count > 0)
            foreach (Toggle toggle in toggles)
            {
                if(toggle.gameObject.activeSelf)
                    toggle.isOn = false;
            }

        foreach (CustomInputField inputField in inputFields)
        {
            if (inputField != null)
            {
                inputField.inputText.text = "";
            }
        }
    }

    private void UpdateQuestions()
    {
        toggleQuestions.Clear();
        textQuestions.Clear();

        if(toggles != null && toggles.Count > 0)
        foreach (Toggle toggle in toggles)
        {
            if(toggle == null || !toggle.gameObject.activeSelf)
                return;
            
            TextMeshProUGUI questionText = toggle.GetComponentInParent<TextMeshProUGUI>();
            if (questionText != null && !string.IsNullOrEmpty(questionText.text))
            {
                toggleQuestions.Add(questionText.text);
            }
            else
            {
                toggleQuestions.Add("Unknown Question");
            }
        }

        if(inputFields != null && inputFields.Count > 0)
        foreach (CustomInputField inputField in inputFields)
        {
            if (inputField == null || !inputField.gameObject.activeSelf)
                continue;
            
            TextMeshProUGUI questionText = inputField.gameObject.GetComponentInParent<TextMeshProUGUI>();
            if (questionText != null && !string.IsNullOrEmpty(questionText.text))
            {
                textQuestions.Add(questionText.text);
            }
            else
            {
                textQuestions.Add("Unknown Question");
            }
        }
    }
    #endregion

    #region User Management
    private void SaveUserName()
    {
        if (userNameInput != null && !string.IsNullOrEmpty(userNameInput.text))
        {
            userName = userNameInput.text.Trim();
            PlayerPrefs.SetString("UserName", userName);
            PlayerPrefs.Save();

            userInputCanvas.SetActive(false);
            surveyCanvas.SetActive(true);
            UpdateQuestions();
        }
        else
        {
            Debug.LogWarning("User name cannot be empty.");
        }
    }
    #endregion

    #region Data Persistence
    private void SaveSurveyToPersistentData()
    {
        if (string.IsNullOrEmpty(adventureName))
        {
            adventureName = "DefaultAdventure";
        }

        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogError("User name is not set.");
            return;
        }

        string filePath = Path.Combine(Application.persistentDataPath, "surveys.json");
        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        SurveyEntry entry = new SurveyEntry
        {
            adventureName = adventureName,
            userName = userName,
            toggleAnswers = new Dictionary<string, bool>(),
            textAnswers = new Dictionary<string, string>()
        };

        while (toggleQuestions.Count < toggles.Count)
        {
            toggleQuestions.Add("New Toggle Question");
        }

        while (toggleQuestions.Count > toggles.Count)
        {
            toggleQuestions.RemoveAt(toggleQuestions.Count - 1);
        }

        while (textQuestions.Count < inputFields.Count)
        {
            textQuestions.Add("New Text Question");
        }

        while (textQuestions.Count > inputFields.Count)
        {
            textQuestions.RemoveAt(textQuestions.Count - 1);
        }

        for (int i = 0; i < toggles.Count; i++)
        {
            string question = i < toggleQuestions.Count ? toggleQuestions[i] : "Unknown Question";
            entry.toggleAnswers[question] = toggles[i].isOn;
        }

        for (int i = 0; i < inputFields.Count; i++)
        {
            if (inputFields[i] != null)
            {
                string question = i < textQuestions.Count ? textQuestions[i] : "Unknown Question";
                entry.textAnswers[question] = inputFields[i].inputText.text;
            }
        }

        SurveyData existingData = null;
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                existingData = JsonConvert.DeserializeObject<SurveyData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading existing data: {e.Message}");
            }
        }

        if (existingData == null)
        {
            existingData = new SurveyData();
        }

        existingData.surveys.RemoveAll(s => s.adventureName == adventureName);
        existingData.surveys.Add(entry);

        JSONParser.SaveToPersistent(filePath, existingData);
    }
    
    public void HideSurvey()
    {
        Time.timeScale = 1;
        surveyCanvas.SetActive(false);
        SaveSurveyToPersistentData();

        GamepadCursor.DisplayCursor(false);
        GamepadCursor.SetCurrentScrollRect(ScrollRect, false);
    }

    public static void SaveSurveyToFile()
    {
        string fileName = "surveys.json";
        string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(persistentPath))
        {
            Debug.LogError($"File {persistentPath} not found.");
            return;
        }

#if UNITY_EDITOR
        string filePath = EditorUtility.SaveFilePanel("Save Survey Data", Application.persistentDataPath, fileName, "json");
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            string json = File.ReadAllText(persistentPath);
            File.WriteAllText(filePath, json);
            Debug.Log($"Survey data saved to: {filePath}");
            if (Instance != null && Instance.notificationText != null)
            {
                Instance.ShowNotification($"Survey saved to: {filePath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving file: {e.Message}");
        }
#else
        StandaloneFileBrowser.SaveFilePanelAsync("Save Survey Data", Application.persistentDataPath, fileName, "json", (filePath) =>
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    string json = File.ReadAllText(persistentPath);
                    File.WriteAllText(filePath, json);
                    Debug.Log($"Survey data saved to: {filePath}");
                    if (Instance != null && Instance.notificationText != null)
                    {
                        Instance.ShowNotification($"Survey saved to: {filePath}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error saving file: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Save file dialog cancelled.");
            }
        });
#endif
    }

    public static void ClearSurveyData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "surveys.json");

        try
        {
            SurveyData emptyData = new SurveyData();
            JSONParser.SaveToPersistent(filePath, emptyData);
            Debug.Log($"Survey data cleared at: {filePath}");

 
            PlayerPrefs.DeleteKey("UserName");
            PlayerPrefs.Save();
            Debug.Log("User name cleared from PlayerPrefs");
            
            if (Instance != null)
            {
                Instance.userName = ""; 
                Debug.Log("Instance userName reset to empty string.");
            }
            
            if (Instance != null && Instance.notificationText != null)
            {
                Instance.ShowNotification("Survey data and user name cleared.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error clearing survey data: {e.Message}");
            if (Instance != null && Instance.notificationText != null)
            {
                Instance.ShowNotification($"Error: {e.Message}");
            }
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            Invoke("HideNotification", 5f);
        }
    }

    private void HideNotification()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }
    #endregion
}

