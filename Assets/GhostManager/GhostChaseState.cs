using UnityEngine;

public class GhostChaseState : GhostBaseState
{
    public override void EnterState(GhostStateManager GhostManager)
    {
        Debug.Log("Ghost Chase");
        GhostManager.ApplyNormalVisual();
    }

    public override void UpdateState(GhostStateManager GhostManager)
    {
        if (GhostManager.IsEaten)
        {
            GhostManager.SwitchState(GhostManager.EatenState);
            return;
        }

        if (GhostManager.IsFrightened)
        {
            GhostManager.SwitchState(GhostManager.FrightenedState);
            return;
        }

        GhostManager.MoveToward(GhostManager.GetChaseTarget(), GhostManager.ChaseSpeed);

    }

    public override void ExitState(GhostStateManager GhostManager)
    {
        // Intentionally empty for skeleton.
    }
}
