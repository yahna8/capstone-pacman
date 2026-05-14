using UnityEngine;

public abstract class GhostBaseState
{
    public abstract void EnterState(GhostStateManager GhostManager);

    public abstract void UpdateState(GhostStateManager GhostManager);

    public abstract void ExitState(GhostStateManager GhostManager);
}
