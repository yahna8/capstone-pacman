using UnityEngine;

public class GhostScatterState : GhostBaseState
{
    public override void EnterState(GhostStateManager GhostManager)
    {
        Debug.Log("Ghost Scatter");
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

        GhostManager.TryGetScatterTarget(out Vector3 target);
        GhostManager.MoveToward(target, GhostManager.ChaseSpeed);

    }

    public override void ExitState(GhostStateManager GhostManager)
    {
        // Intentionally empty for skeleton.
    }
}
