public abstract class PlayerState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;
    protected int animHash;

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine, string animName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animHash = UnityEngine.Animator.StringToHash(animName);
    }

    public virtual void Enter() => player.animator.Play(animHash);
    public virtual void HandleInput() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}