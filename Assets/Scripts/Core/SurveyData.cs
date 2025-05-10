using System.Collections.Generic;

[System.Serializable]
public class SurveyData
{
    public List<SurveyEntry> surveys = new List<SurveyEntry>();
}

[System.Serializable]
public class SurveyEntry
{
    public string adventureName;
    public string userName;
    public Dictionary<string, bool> toggleAnswers;
    public Dictionary<string, string> textAnswers;
}