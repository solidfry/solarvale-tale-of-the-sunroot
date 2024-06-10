using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public List<Quest> quests = new List<Quest>();
    public TMP_Text questLogText;

    void Start()
    {
        AddQuest("Clean up Whale Rock", "Description");
        AddQuest("Bring the sick gecko to Grandma", "Description");
        AddQuest("Name the Gecko", "Description");
        AddQuest("Explore Solarvale", "Use camera to find clues on the gecko origin");
        AddQuest("Take a photo of the painting", "Description");

        UpdateQuestLog();
    }

    public void AddQuest(string title, string description)
    {
        quests.Add(new Quest(title, description));
        Debug.Log("Quest added: " + title);
        UpdateQuestLog();
    }

    public void CompleteQuest(string title)
    {
        Debug.Log("Attempting to complete quest: " + title);
        Quest quest = quests.Find(q => q.title == title);
        if (quest != null)
        {
            quest.CompleteQuest();
            Debug.Log(title + " completed");
            UpdateQuestLog();
        }
        else
        {
            Debug.LogWarning("Quest not found: " + title);
            Debug.LogWarning("Available quests are:");
            foreach (var q in quests)
            {
                Debug.LogWarning(" - " + q.title);
            }
        }
    }

    void UpdateQuestLog()
    {
        questLogText.text = "";
        foreach (Quest quest in quests)
        {
            string color = quest.isCompleted ? "green" : "red";
            string status = quest.isCompleted ? " (Completed)" : "";
            questLogText.text += $"<color={color}>{quest.title}:</color> {quest.description}{status}\n";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Key 1 pressed");
            CompleteQuest("Clean up Whale Rock");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Key 2 pressed");
            CompleteQuest("Bring the sick gecko to Grandma");
        }
        // Add more code for other quests as needed
    }
}