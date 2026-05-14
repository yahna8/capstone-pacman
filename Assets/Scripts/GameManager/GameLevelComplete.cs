using UnityEngine;

public class GameLevelCompleteState : GameBaseState
{
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Level Complete");

        GameManager.RestartPressed = false;
        GameManager.QuitToMenuPressed = false;

        Time.timeScale = 0f;
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        if (GameManager.RestartPressed)
        {
            Time.timeScale = 1f;
            GameManager.ResetRun();
            GameManager.SwitchState(GameManager.CountdownState);
        }
        else if (GameManager.QuitToMenuPressed)
        {
            Time.timeScale = 1f;
            GameManager.ResetRun();
            GameManager.SwitchState(GameManager.MenuState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {

    }
}