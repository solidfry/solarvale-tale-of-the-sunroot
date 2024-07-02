using QuestSystem;
using QuestSystem.Conditions;
using UnityEditor;

[CustomEditor(typeof(QuestUpdater))]
    public class QuestUpdaterEditor : Editor
    {
        QuestUpdater _questUpdater;
        
        private void OnEnable()
        {
            _questUpdater = (QuestUpdater) target;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            
            switch(_questUpdater.questAction)
            {
                case QuestAction.UpdateCondition:
                    EditorGUILayout.HelpBox("Updates the condition of the quest.", MessageType.Info);
                    // I want to show only the QuestConditionBase field here
                    _questUpdater.questCondition = (QuestConditionBase) EditorGUILayout.ObjectField("Quest Condition", _questUpdater.questCondition, typeof(QuestConditionBase), true);
                    break;
                case QuestAction.Complete:
                    EditorGUILayout.HelpBox("Completes the quest.", MessageType.Info);
                    // I want to show only the QuestData field here
                    _questUpdater.questData = (QuestData) EditorGUILayout.ObjectField("Quest Data", _questUpdater.questData, typeof(QuestData), true);
                    
                    break;
                case QuestAction.Add:
                    EditorGUILayout.HelpBox("Adds the quest to the quest list.", MessageType.Info);
                    // I want to show only the QuestData field here
                    _questUpdater.questData = (QuestData) EditorGUILayout.ObjectField("Quest Data", _questUpdater.questData, typeof(QuestData), true);
                    break;
            }
            
        }
    }
