namespace Game.Controller
{
    [System.Flags]
    public enum HookInteraction
    {
        None = 1,
        StaticAnchor = 2,
        DynamicBody = 4,
        Draggable = 8,
        Grapling = 16,
        Swing = 32,
        
        StaticSwing = StaticAnchor | Swing,
        DynamicSwing = DynamicBody | Swing,
        DynamicGrapling = DynamicBody | Grapling,
        StaticGrapling = StaticAnchor | Grapling,
        LightweightObject = DynamicBody | Grapling | Draggable,
        AnyStatic = StaticAnchor | Grapling | Swing,
        AnyDynamic = DynamicBody | Grapling | Swing,
    }
}
