using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/cameraConstraintTrigger")]
public class CameraConstraintTrigger : Trigger {
    private CameraConstraints constraints;
    private bool onlyOnce;
    private string flag;
    private bool inverted;

    public CameraConstraintTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        constraints = new CameraConstraints {
            HasMinX = data.Bool("hasMinX"),
            HasMaxX = data.Bool("hasMaxX"),
            HasMinY = data.Bool("hasMinY"),
            HasMaxY = data.Bool("hasMaxY"),
            MinX = 48f * data.Float("minX"),
            MaxX = 48f * data.Float("maxX"),
            MinY = 32f * data.Float("minY"),
            MaxY = 32f * data.Float("maxY")
        };

        onlyOnce = data.Bool("onlyOnce");
        flag = data.Attr("flag");
        inverted = data.Bool("inverted");
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        if (!Util.CheckFlag(flag, SceneAs<Level>().Session, inverted))
            return;

        DynamicData.For(player).Set("programmatic.ProgHelper.CameraConstraints", constraints);

        if (onlyOnce)
            RemoveSelf();
    }
}