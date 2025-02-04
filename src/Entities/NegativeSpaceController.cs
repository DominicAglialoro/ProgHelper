using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/negativeSpaceController"), Tracked]
public class NegativeSpaceController : Entity {
    public readonly bool BackgroundInvertsColor;

    private readonly bool flipsGravity;

    public NegativeSpaceController(EntityData data, Vector2 offset) : base(data.Position + offset) {
        BackgroundInvertsColor = data.Bool("backgroundInvertsColor");
        flipsGravity = data.Bool("flipsGravity");
    }

    public bool CheckForSwap(bool flipGravityIfSwapped) {
        var player = Scene.Tracker.GetEntity<Player>();
        var dynamicData = DynamicData.For(Scene);
        var bgSolid = dynamicData.Get<Solid>("bgSolid");
        var fgSolid = dynamicData.Get<Solid>("fgSolid");

        Solid currentSolid;
        Solid otherSolid;

        if (bgSolid.Collidable) {
            currentSolid = bgSolid;
            otherSolid = fgSolid;
        }
        else {
            currentSolid = fgSolid;
            otherSolid = bgSolid;
        }

        if (!player.CollideCheck(currentSolid))
            return false;

        otherSolid.Collidable = true;

        if (player.CollideCheck(otherSolid)) {
            otherSolid.Collidable = false;

            return false;
        }

        currentSolid.Collidable = false;

        if (flipGravityIfSwapped && flipsGravity)
            GravityHelperImports.SetPlayerGravity?.Invoke(2, 1f);

        return true;
    }
}