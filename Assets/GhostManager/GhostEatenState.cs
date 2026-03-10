using UnityEngine;

public class GhostEatenState : GhostBaseState
{
    public override void EnterState(GhostStateManager GhostManager)
    {
        Debug.Log("Ghost Eaten / Return Home");
        GhostManager.ReachedHome = false;
        GhostManager.IsFrightened = false;
        GhostManager.ClearPostFrightenedState();

        // Lock the movement target to home as soon as eaten starts.
        if (GhostManager.HomeTransform != null)
            GhostManager.SetMoveTarget(GhostManager.HomeTransform.position);
    }

    public override void UpdateState(GhostStateManager GhostManager)
    {
        if (GhostManager.HomeTransform == null)
        {
            GhostManager.IsEaten = false;
            GhostManager.SwitchState(GhostManager.SpawnState);
            return;
        }

        GhostManager.ReachedHome =
            GhostManager.MoveToward(GhostManager.HomeTransform.position, GhostManager.ReturnSpeed);

        if (GhostManager.ReachedHome)
        {
            GhostManager.ReachedHome = false;
            GhostManager.IsEaten = false;
            GhostManager.SwitchState(GhostManager.SpawnState);
        }
    }

    public override void ExitState(GhostStateManager GhostManager)
    {
        // Intentionally empty for skeleton.
    }
}
