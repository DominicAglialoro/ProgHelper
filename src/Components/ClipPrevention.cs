using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[Tracked]
public class ClipPrevention : Component {
    private bool right;
    private bool left;
    private bool up;
    private bool down;
    private Collider collider;

    public ClipPrevention(bool right, bool left, bool up, bool down, Collider collider) : base(true, false) {
        this.right = right;
        this.left = left;
        this.up = up;
        this.down = down;
        this.collider = collider;
    }

    private static List<ClipPrevention> triggersToCheck = new();

    public static void BeginTestH(Player player) => BeginTest(player, false);

    public static void BeginTestV(Player player) => BeginTest(player, true);

    public static void EndTest() => triggersToCheck.Clear();

    public static bool CheckH(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitX);

    public static bool CheckV(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitY);

    private static void BeginTest(Player player, bool vertical) {
        var triggers = player.Scene.Tracker.GetComponents<ClipPrevention>();

        if (triggersToCheck.Capacity != triggers.Count)
            triggersToCheck = new List<ClipPrevention>(triggers.Count);

        foreach (var component in triggers) {
            var trigger = (ClipPrevention) component;
            bool shouldCheck;

            if (vertical)
                shouldCheck = trigger.up && player.Speed.Y < 0f || trigger.down && player.Speed.Y > 0f;
            else
                shouldCheck = trigger.right && player.Speed.X > 0f || trigger.left && player.Speed.X < 0f;

            if (shouldCheck && !player.Collider.Collide(trigger.collider))
                triggersToCheck.Add(trigger);
        }
    }

    private static bool Check(Actor actor, Vector2 at) {
        var collider = actor.Collider;
        var position = actor.Position;

        foreach (var trigger in triggersToCheck) {
            var triggerCollider = trigger.collider;

            if (!collider.Collide(triggerCollider))
                continue;

            actor.Position = at;

            if (!collider.Collide(triggerCollider)) {
                actor.Position = position;

                return true;
            }

            actor.Position = position;
        }

        return false;
    }
}