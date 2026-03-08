using UnityEngine;

public abstract class GameBaseState
{
    public abstract void EnterState(GameStateManager GameManager);

    public abstract void UpdateState(GameStateManager GameManager);

    public abstract void OnCollisionEnter(GameStateManager GameManager);

}
