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

        bool shouldStop = false;

        for (int i = 0; i < collidersToCheck.Count; i++) {
            var collider = collidersToCheck[i];

            if (collider == null)
                continue;

            var playerCollider = player.Collider;

            player.Collider = player.hurtbox;

            if (!player.CollideCheck(collider.Entity)) {
                player.Collider = playerCollider;

                continue;
            }

            player.Collider = playerCollider;
            collidersToCheck[i] = null;

            if (collider.OnCollide(player))
                shouldStop = true;
        }

        return shouldStop;
    }

    public Func<Player, bool> OnCollide;

    public StrictPlayerCollider(Func<Player, bool> onCollide) : base(true, false) => OnCollide = onCollide;
}