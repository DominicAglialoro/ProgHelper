using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[Tracked]
public class ClipPrevention : Component {
    private readonly bool right;
    private readonly bool left;
    private readonly bool up;
    private readonly bool down;
    private readonly Collider collider;

    public ClipPrevention(bool right, bool left, bool up, bool down, Collider collider) : base(true, false) {
        this.right = right;
        this.left = left;
        this.up = up;
        this.down = down;
        this.collider = collider;
    }

    private static List<ClipPrevention> triggersToCheck = new();

    public static void BeginTestH(Player player) {
        var triggers = player.Scene.Tracker.GetComponents<ClipPrevention>();

        if (triggersToCheck.Capacity != triggers.Count)
            triggersToCheck = new List<ClipPrevention>(triggers.Count);

        foreach (var component in triggers) {
            var trigger = (ClipPrevention) component;

            if ((trigger.right && player.Speed.X > 0f || trigger.left && player.Speed.X < 0f) && !trigger.collider.Collide(player.Collider))
                triggersToCheck.Add(trigger);
        }
    }

    public static void BeginTestV(Player player) {
        var triggers = player.Scene.Tracker.GetComponents<ClipPrevention>();

        if (triggersToCheck.Capacity != triggers.Count)
            triggersToCheck = new List<ClipPrevention>(triggers.Count);

        foreach (var component in triggers) {
            var trigger = (ClipPrevention) component;

            if ((trigger.up && player.Speed.Y < 0f || trigger.down && player.Speed.Y > 0f) && !trigger.collider.Collide(player.Collider))
                triggersToCheck.Add(trigger);
        }
    }

    public static void EndTest() => triggersToCheck.Clear();

    public static bool CheckH(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitX);

    public static bool CheckV(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitY);

    private static bool Check(Actor actor, Vector2 at) {
        var collider = actor.Collider;
        var position = actor.Position;

        foreach (var trigger in triggersToCheck) {
            var triggerCollider = trigger.collider;

            if (!triggerCollider.Collide(collider))
                continue;

            actor.Position = at;

            if (!triggerCollider.Collide(collider)) {
                actor.Position = position;

                return true;
            }

            actor.Position = position;
        }

        return false;
    }
}