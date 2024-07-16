using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameObject questUIIcon;
    void Update()
    {
        Vector3 directionToFace = playerCharacter.transform.position - questUIIcon.transform.position;

        directionToFace.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToFace);
        questUIIcon.transform.rotation = lookRotation;
        questUIIcon.transform.rotation = Quaternion.Euler(questUIIcon.transform.rotation.eulerAngles.x,
                                                          questUIIcon.transform.rotation.eulerAngles.y,
                                                          -45);
    }
}
