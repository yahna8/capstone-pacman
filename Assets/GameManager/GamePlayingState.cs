using UnityEngine;

public class GamePlayingState : GameBaseState
{    
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Playing");

        // Clear UI-driven flags
        GameManager.ResumePressed = false;
        GameManager.QuitToMenuPressed = false;
        GameManager.RestartPressed = false;
        GameManager.PlayerIsDead = false;

        //Enable input/movement here later
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        // Win condition
        if (GameManager.RemainingPellets <= 0)
        {
            GameManager.SwitchState(GameManager.LevelCompleteState);
            return;
        }

        // Death condition
        if (GameManager.PlayerIsDead)
        {
            GameManager.SwitchState(GameManager.LifeLostState);
            return;
        }

        // Pause condition (for now Esc is used)
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }    
}