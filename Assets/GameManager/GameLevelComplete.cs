using UnityEngine;

public class GameLevelCompleteState : GameBaseState
{
    private float timer;

    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Level Complete");

        timer = GameManager.LevelCompleteSeconds;

        // Eventually award bonus, load next level, reset pellets here
        // For now just rerstore pellets
        GameManager.RemainingPellets = 30;
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            GameManager.SwitchState(GameManager.CountdownState);
        }
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }   
}