using UnityEngine;

public class GhostSpawnState : GhostBaseState
{
    public override void EnterState(GhostStateManager GhostManager)
    {
        Debug.Log("Ghost Spawn");
        GhostManager.SpawnReached = false;
        GhostManager.ApplyNormalVisual();
    }

    public override void UpdateState(GhostStateManager GhostManager)
    {
        if (GhostManager.SpawnTransform == null)
        {
            GhostManager.SwitchState(GhostManager.GetTimedPatrolState());
            return;
        }

        Vector3 spawnTarget = GhostManager.GetSpawnTargetPosition();
        GhostManager.SpawnReached =
            GhostManager.MoveToward(spawnTarget, GhostManager.ChaseSpeed);

        if (GhostManager.SpawnReached)
        {
            GhostManager.SpawnReached = false;
            GhostManager.ShouldEnterChase = false;
            GhostManager.ShouldEnterScatter = false;
            GhostManager.SwitchState(GhostManager.GetTimedPatrolState());
        }
    }

    public override void ExitState(GhostStateManager GhostManager)
    {
        // Intentionally empty for skeleton.
    }
}
