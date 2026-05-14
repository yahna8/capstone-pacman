using UnityEngine;

public class GamePausedState : GameBaseState
{
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Paused");

        GameManager.ResumePressed = false;
        GameManager.QuitToMenuPressed = false;

        Time.timeScale = 0f;
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        if (GameManager.ResumePressed)
        {
            Time.timeScale = 1f;
            GameManager.SwitchState(GameManager.PlayingState);
        }
        else if (GameManager.QuitToMenuPressed)
        {
            Time.timeScale = 1f;
            GameManager.SwitchState(GameManager.MenuState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }    
}