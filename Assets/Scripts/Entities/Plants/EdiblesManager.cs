using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;

namespace Entities.Plants
{
    public class EdiblesManager : MonoBehaviour
    {
        readonly Dictionary<PlantEntityData, List<IEdible>> _ediblesDictionary = new();
        [SerializeField] int dictionaryCount = 0;
        
        private void Awake()
        {
            PlantEntityData[] plantsEntityData = Resources.LoadAll<PlantEntityData>("Data/Entities/Edibles");
            foreach (PlantEntityData plantEntityData in plantsEntityData)
            {
                _ediblesDictionary.Add(plantEntityData, new List<IEdible>());
                // Debug.Log(this + " added " + plantEntityData + " to the dictionary");
            }
        }

        private void OnEnable()
        {
            GlobalEvents.OnRegisterEdibleEvent += RegisterEdible;
            GlobalEvents.OnSendEdibleEatenEvent += (IEdible edible) => StartCoroutine(EdibleEaten(edible.GetEntityData));
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnRegisterEdibleEvent -= RegisterEdible;
        }

        private void RegisterEdible(IEdible edible)
        {
            if (edible.GetEntityData is { } plantEntityData)
            {
                _ediblesDictionary[plantEntityData].Add(edible);
                // Debug.Log(this + " registered " + edible + " as an edible of " + plantEntityData);
                dictionaryCount++;
            }
        }
        
        IEnumerator EdibleEaten(PlantEntityData plantEntityData)
        {
            yield return new WaitForSeconds(plantEntityData.GetStats().GrowthTime);
            foreach (IEdible edible in _ediblesDictionary[plantEntityData])
            {
                edible.Reset();
            }
        }
    }
}
