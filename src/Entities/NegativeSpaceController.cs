using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/negativeSpaceController"), Tracked]
public class NegativeSpaceController : Entity {
    public readonly bool FlipsGravity;

    public NegativeSpaceController(EntityData data, Vector2 offset) : base(data.Position + offset) => FlipsGravity = data.Bool("flipsGravity");

    public void InitSolids() {
        var solids = Scene.Tracker.GetEntities<NegativeSpaceSolid>();

        foreach (var solid in solids)
            solid.Collidable = true;

        var player = Scene.Tracker.GetEntity<Player>();

        foreach (var solid in solids) {
            if (!player.CollideCheck(solid))
                continue;

            solid.Collidable = false;

            return;
        }
    }

    public bool CheckForSwap() {
        var solids = Scene.Tracker.GetEntities<NegativeSpaceSolid>();
        var player = Scene.Tracker.GetEntity<Player>();
        Entity currentSolid = null;

        foreach (var solid in solids) {
            if (solid.Collidable)
                continue;

            currentSolid = solid;

            break;
        }

        Entity newCurrentSolid = null;

        foreach (var solid in solids) {
            if (!solid.CollideCheck(player))
                continue;

            if (newCurrentSolid != null || solid == currentSolid)
                return false;

            newCurrentSolid = solid;
        }

        foreach (var solid in solids)
            solid.Collidable = true;

        if (newCurrentSolid != null)
            newCurrentSolid.Collidable = false;

        return true;
    }
}