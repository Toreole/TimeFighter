namespace Game.Controller
{
    public interface IHookable : IPhysicsObject
    {
        HookInteraction HookInteract { get; }
        UnityEngine.Transform M_Transform { get; }
        bool HasFlag(HookInteraction interaction);
        bool CanBeDragged { get; }
        bool UseCenterPoint { get; }
    }
}
