using UnityEngine;

public class GameCountdownState : GameBaseState
{
    private float timer;
    
    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Countdown");

        timer = GameManager.CountdownSeconds;

        // Reset flags that shouldn't carry into play
        GameManager.PlayerIsDead = false;
        GameManager.ResumePressed = false;
        GameManager.QuitToMenuPressed = false;

        // Reset positions here later (player and ghosts)
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        timer -= Time.deltaTime;

        if(timer <= 0f)
        {
            GameManager.SwitchState(GameManager.PlayingState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }
}