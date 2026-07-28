using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/cameraConstraintTrigger")]
public class CameraConstraintTrigger : Trigger {
    private readonly CameraConstraints.Constraints constraints;
    private readonly bool onlyOnce;
    private readonly string flag;
    private readonly bool inverted;

    public CameraConstraintTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        constraints = new CameraConstraints.Constraints {
            HasMinX = data.Bool("hasMinX"),
            HasMaxX = data.Bool("hasMaxX"),
            HasMinY = data.Bool("hasMinY"),
            HasMaxY = data.Bool("hasMaxY"),
            MinX = 48f * data.Float("minX") - 160f,
            MaxX = 48f * data.Float("maxX") - 160f,
            MinY = 32f * data.Float("minY") - 90f,
            MaxY = 32f * data.Float("maxY") - 90f,
            Smoothing = data.Float("smoothing")
        };

        onlyOnce = data.Bool("onlyOnce");
        flag = data.Attr("flag");
        inverted = data.Bool("inverted");
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        if (!Util.CheckFlag(flag, SceneAs<Level>().Session, inverted))
            return;

        var cameraConstraints = player.Components.Get<CameraConstraints>();

        if (cameraConstraints == null) {
            cameraConstraints = new CameraConstraints();
            player.Add(cameraConstraints);
        }

        cameraConstraints.TargetConstraints = constraints;
        cameraConstraints.MinX = constraints.MinX;
        cameraConstraints.MaxX = constraints.MaxX;
        cameraConstraints.MinY = constraints.MinY;
        cameraConstraints.MaxY = constraints.MaxY;

        if (constraints.Smoothing > 0f) {
            var camera = player.level.Camera;
            var relativePosition = camera.Position - player.Position;

            if (constraints.HasMinX && relativePosition.X < constraints.MinX)
                cameraConstraints.MinX = relativePosition.X;

            if (constraints.HasMaxX && relativePosition.X > constraints.MaxX)
                cameraConstraints.MaxX = relativePosition.X;

            if (constraints.HasMinY && relativePosition.Y < constraints.MinY)
                cameraConstraints.MinY = relativePosition.Y;

            if (constraints.HasMaxY && relativePosition.Y > constraints.MaxY)
                cameraConstraints.MaxY = relativePosition.Y;
        }

        if (onlyOnce)
            RemoveSelf();
    }
}