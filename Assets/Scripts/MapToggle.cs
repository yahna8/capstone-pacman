using UnityEngine;

public class MapToggle : MonoBehaviour
{
    [Tooltip("Drag the MapDisplay UI object here.")]
    public GameObject mapUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapUI != null)
            {
                mapUI.SetActive(!mapUI.activeSelf);
            }
        }
    }
}