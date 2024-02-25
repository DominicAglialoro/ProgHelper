using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/speedCameraOffsetTrigger")]
public class SpeedCameraOffsetTrigger : Trigger {
    private float speedFromX;
    private float speedToX;
    private float speedFromY;
    private float speedToY;
    private float offsetFromX;
    private float offsetToX;
    private float offsetFromY;
    private float offsetToY;
    private bool onlyOnce;
    private bool xOnly;
    private bool yOnly;
    private string flag;
    private bool inverted;

    public SpeedCameraOffsetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        speedFromX = data.Float("speedFromX");
        speedToX = data.Float("speedToX");
        speedFromY = data.Float("speedFromY");
        speedToY = data.Float("speedToY");
        offsetFromX = 48f * data.Float("offsetFromX");
        offsetToX = 48f * data.Float("offsetToX");
        offsetFromY = 32f * data.Float("offsetFromY");
        offsetToY = 32f * data.Float("offsetToY");
        onlyOnce = data.Bool("onlyOnce");
        xOnly = data.Bool("xOnly");
        yOnly = data.Bool("yOnly");
        flag = data.Attr("flag");
        inverted = data.Bool("inverted");
    }

    public override void OnStay(Player player) {
        base.OnStay(player);

        var level = SceneAs<Level>();

        if (!Util.CheckFlag(flag, level.Session, inverted))
            return;

        if (!yOnly)
            level.CameraOffset.X = speedFromX != speedToX ? Calc.ClampedMap(player.Speed.X, speedFromX, speedToX, offsetFromX, offsetToX) : offsetFromX;

        if (!xOnly)
            level.CameraOffset.Y = speedFromY != speedToY ? Calc.ClampedMap(player.Speed.Y, speedFromY, speedToY, offsetFromY, offsetToY) : offsetFromY;
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);

        if (!Util.CheckFlag(flag, SceneAs<Level>().Session, inverted))
            return;

        if (onlyOnce)
            RemoveSelf();
    }
}