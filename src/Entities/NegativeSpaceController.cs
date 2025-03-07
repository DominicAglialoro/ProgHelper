using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/negativeSpaceController"), Tracked]
public class NegativeSpaceController : Entity {
    public readonly bool BackgroundInvertsColor;
    public readonly bool FlipsGravity;

    public NegativeSpaceController(EntityData data, Vector2 offset) : base(data.Position + offset) {
        BackgroundInvertsColor = data.Bool("backgroundInvertsColor");
        FlipsGravity = data.Bool("flipsGravity");
    }

    public bool CheckForSwap() {
        var solids = Scene.Tracker.GetEntities<NegativeSpaceSolid>();

        Solid currentSolid;
        Solid otherSolid;

        if (solids[0].Collidable) {
            currentSolid = (Solid) solids[0];
            otherSolid = (Solid) solids[1];
        }
        else {
            currentSolid = (Solid) solids[1];
            otherSolid = (Solid) solids[0];
        }

        var player = Scene.Tracker.GetEntity<Player>();

        if (!player.CollideCheck(currentSolid))
            return false;

        otherSolid.Collidable = true;

        if (player.CollideCheck(otherSolid)) {
            otherSolid.Collidable = false;

            return false;
        }

        currentSolid.Collidable = false;


        return true;
    }
}