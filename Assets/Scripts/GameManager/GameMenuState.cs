using UnityEngine;

public class GameMenuState : GameBaseState
{
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Menu");

        GameManager.StartPressed = false;
        GameManager.QuitToMenuPressed = false;
        GameManager.RestartPressed = false;
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        if (GameManager.StartPressed)
        {
            // Time.timeScale = 1f;
            GameManager.SwitchState(GameManager.CountdownState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }    
}