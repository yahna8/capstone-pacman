using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private SystemManager systemManager;

    public void Initialize(SystemManager manager)
    {
        // stores reference to allow LevelManager to communicate w/ SystemManager
        systemManager = manager;
    }
}
