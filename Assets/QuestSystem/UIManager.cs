using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public QuestManager questManager;
    public TMP_Text questLogText;

    void Start()
    {
        questManager.questLogText = questLogText;
    }
}
