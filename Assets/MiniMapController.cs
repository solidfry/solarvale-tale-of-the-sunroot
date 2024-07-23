using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] GameObject[] mapObjects;

    public void Initialise()
    {
        foreach (var mapObject in mapObjects)
        {
            mapObject.SetActive(true);
        }
    }
    public GameObject GetMapObject(int index)
    {
        if (index >= 0 && index < mapObjects.Length)
        {
            return mapObjects[index];
        }
        return null;
    }
}
