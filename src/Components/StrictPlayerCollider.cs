using System;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[Tracked]
public class StrictPlayerCollider : Component {
    private static List<StrictPlayerCollider> collidersToCheck = new();

    public static void BeginTest(Player player) {
        var colliders = player.Scene.Tracker.GetComponents<StrictPlayerCollider>();

        if (collidersToCheck.Capacity != colliders.Count)
            collidersToCheck = new List<StrictPlayerCollider>(colliders.Count);

        foreach (StrictPlayerCollider collider in player.Scene.Tracker.GetComponents<StrictPlayerCollider>())
            collidersToCheck.Add(collider);
    }

    public static void EndTest() => collidersToCheck.Clear();

    public static bool Check(Actor actor) {
        if (collidersToCheck.Count == 0 || actor is not Player player || player.Dead || player.StateMachine.State == Player.StCassetteFly)
            return false;

        var playerCollider = player.Collider;
        bool shouldStop = false;

        player.Collider = player.hurtbox;

        for (int i = collidersToCheck.Count - 1; i >= 0; i--) {
            var collider = collidersToCheck[i];

            if (!player.CollideCheck(collider.Entity))
                continue;

            shouldStop |= collider.OnCollide(player);
            collidersToCheck.RemoveAt(i);
        }

        player.Collider = playerCollider;

        return shouldStop;
    }

    public Func<Player, bool> OnCollide;

    public StrictPlayerCollider(Func<Player, bool> onCollide) : base(true, false) => OnCollide = onCollide;
}