using UnityEngine;
using Game.Controller;

namespace Game
{
    public interface IPhysicsObject
    {
        Rigidbody2D Body { get; }
        Vector2 Position { get; }
    }
}
