using UnityEngine;

public class GhostFrightenedState : GhostBaseState
{
    public override void EnterState(GhostStateManager GhostManager)
    {
        Debug.Log("Ghost Frightened");
        GhostManager.FrightenedTimer = GhostManager.FrightenedSeconds;
        GhostManager.ApplyFrightenedVisual();
    }

    public override void UpdateState(GhostStateManager GhostManager)
    {
        if (GhostManager.IsEaten)
        {
            GhostManager.SwitchState(GhostManager.EatenState);
            return;
        }

        if (GhostManager.PlayerTransform != null)
        {
            Vector3 away = GhostManager.transform.position - GhostManager.PlayerTransform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
                away = GhostManager.transform.forward;

            Vector3 retreatTarget = GhostManager.transform.position
                                  + away.normalized * GhostManager.FrightenedRetreatDistance;
            GhostManager.MoveToward(retreatTarget, GhostManager.FrightenedSpeed);
        }

        GhostManager.FrightenedTimer -= Time.deltaTime;
        if (GhostManager.FrightenedTimer <= 0f)
        {
            GhostManager.IsFrightened = false;
            GhostManager.SwitchState(GhostManager.GetPostFrightenedStateAndSyncMode());
            GhostManager.ClearPostFrightenedState();
        }
    }

    public override void ExitState(GhostStateManager GhostManager)
    {
        // Intentionally empty for skeleton.
    }
}
