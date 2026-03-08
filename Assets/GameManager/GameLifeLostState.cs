using UnityEngine;

public class GameLifeLostState : GameBaseState
{
    private float timer;

    public override void EnterState(GameStateManager GameManager)
    {
        Debug.Log("Life Lost");
        
        GameManager.PlayerIsDead = false;
        GameManager.Lives--;

        timer = GameManager.LifeLostSeconds;

        //Disable movement and take care of other death consequences here
    }

    public override void UpdateState(GameStateManager GameManager)
    {
        timer -= Time.deltaTime;
        if(timer > 0f) return;

        if(GameManager.Lives > 0)
            GameManager.SwitchState(GameManager.CountdownState);
        else
            GameManager.SwitchState(GameManager.OverState);
    }

    public override void OnCollisionEnter(GameStateManager GameManager)
    {
        
    }    
}