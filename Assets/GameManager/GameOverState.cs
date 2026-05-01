using UnityEngine;

public class GameOverState : GameBaseState
{
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Game Over");

        GameManager.RestartPressed = false;
        GameManager.QuitToMenuPressed = false;

        // Show game over UI here
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        if (GameManager.RestartPressed)
        {
            GameManager.ResetRun();
            GameManager.SwitchState(GameManager.CountdownState);
        }
        else if (GameManager.QuitToMenuPressed)
        {
            GameManager.ResetRun();
            GameManager.SwitchState(GameManager.MenuState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }    
}