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

            if (trigger.Entity.Collidable && (trigger.right && player.Speed.X > 0f || trigger.left && player.Speed.X < 0f) && !trigger.CollideCheck(player))
                triggersToCheck.Add(trigger);
        }
    }

    public static void BeginTestV(Player player) {
        var triggers = player.Scene.Tracker.GetComponents<ClipPrevention>();

        if (triggersToCheck.Capacity != triggers.Count)
            triggersToCheck = new List<ClipPrevention>(triggers.Count);

        foreach (var component in triggers) {
            var trigger = (ClipPrevention) component;

            if (trigger.Entity.Collidable && (trigger.up && player.Speed.Y < 0f || trigger.down && player.Speed.Y > 0f) && !trigger.CollideCheck(player))
                triggersToCheck.Add(trigger);
        }
    }

    public static void EndTest() => triggersToCheck.Clear();

    public static bool CheckH(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitX);

    public static bool CheckV(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitY);

    private static bool Check(Actor actor, Vector2 at) {
        var position = actor.Position;

        foreach (var trigger in triggersToCheck) {
            if (!trigger.CollideCheck(actor))
                continue;

            actor.Position = at;

            if (!trigger.CollideCheck(actor)) {
                actor.Position = position;

                return true;
            }

            actor.Position = position;
        }

        return false;
    }

    private bool CollideCheck(Actor actor) {
        var entityCollider = Entity.Collider;

        Entity.Collider = collider;

        bool result = actor.CollideCheck(Entity);

        Entity.Collider = entityCollider;

        return result;
    }
}