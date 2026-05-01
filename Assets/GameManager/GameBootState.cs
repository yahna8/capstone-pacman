using UnityEngine;

public class GameBootState : GameBaseState
{
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Booting Up...");

        // Temporary lines to initialize defaults
        GameManager.ResetRun();

        //Clear flags
        GameManager.StartPressed = false;
        GameManager.ResumePressed = false;
        GameManager.QuitToMenuPressed = false;
        GameManager.RestartPressed = false;
        GameManager.PlayerIsDead = false;
 
        // Next
        GameManager.SwitchState(GameManager.MenuState);
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }
}