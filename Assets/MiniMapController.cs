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
}
