using System;
using System.Collections;

namespace Game.Controller
{
    public interface ICooldown
    {
        float RemainingCooldown { get; }
        float RelativeCooldown { get; }

        IEnumerator DoCooldown();
    }
}
