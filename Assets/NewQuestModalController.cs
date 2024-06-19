using QuestSystem;
using TMPro;
using UI.Utilities;
using UnityEngine;

public class NewQuestModalController : MonoBehaviour
{
    [SerializeField] TMP_Text questName;
    [SerializeField] QuestData questData;
    [SerializeField] DoTweenAnimationHandler doTweenAnimationHandler;

    private void Start()
    {
        SetQuestData(questData);
    }
    
    private void OnDisable()
    {
        ClearQuestName();
    }

    public void SetQuestName()
    {
        questName.text = name;
    }
    
    public void ClearQuestName()
    {
        questName.text = "";
    }
    
    public void SetQuestData(QuestData data)
    {
        if (data is null) return;
        questData = data;
        SetQuestName();
    }
    
}
