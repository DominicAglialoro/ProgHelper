using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

[CustomEntity("progHelper/clipPreventionTrigger"), Tracked]
public class ClipPreventionTrigger : Entity {
    private static List<ClipPreventionTrigger> triggersToCheck = new();

    public static void BeginTestH(Player player) => BeginTest(player, false);

    public static void BeginTestV(Player player) => BeginTest(player, true);

    public static void EndTest() => triggersToCheck.Clear();

    public static bool CheckH(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitX);
    
    public static bool CheckV(Actor actor, int dir) => Check(actor, actor.Position + dir * Vector2.UnitY);

    private static void BeginTest(Player player, bool vertical) {
        var triggers = player.Scene.Tracker.GetEntities<ClipPreventionTrigger>();

        if (triggersToCheck.Capacity != triggers.Count)
            triggersToCheck = new List<ClipPreventionTrigger>(triggers.Count);

        foreach (var entity in triggers) {
            var trigger = (ClipPreventionTrigger) entity;
            bool shouldCheck;

            if (vertical)
                shouldCheck = trigger.up && player.Speed.Y < 0f || trigger.down && player.Speed.Y > 0f;
            else
                shouldCheck = trigger.right && player.Speed.X > 0f || trigger.left && player.Speed.X < 0f;

            if (shouldCheck && !player.CollideCheck(trigger))
                triggersToCheck.Add(trigger);
        }
    }

    private static bool Check(Actor actor, Vector2 at) {
        foreach (var trigger in triggersToCheck) {
            if (actor.CollideCheck(trigger) && !actor.CollideCheck(trigger, at))
                return true;
        }

        return false;
    }
    
    private bool right;
    private bool left;
    private bool up;
    private bool down;
    
    public ClipPreventionTrigger(EntityData data, Vector2 offset) : base(data.Position + offset) {
        right = data.Bool("right");
        left = data.Bool("left");
        up = data.Bool("up");
        down = data.Bool("down");
        
        Collider = new Hitbox(data.Width, data.Height);
        Visible = false;
    }
}