using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

public class CameraConstraints : Component {
    public struct Constraints {
        public bool HasMinX;
        public bool HasMaxX;
        public bool HasMinY;
        public bool HasMaxY;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
        public float Smoothing;
    }

    public Constraints TargetConstraints;

    private float smoothMinX;
    private float smoothMaxX;
    private float smoothMinY;
    private float smoothMaxY;

    public CameraConstraints() : base(false, false) { }

    public void UpdateSmoothConstraints(Vector2 cameraPosition, Vector2 playerPosition) {
        var relativePosition = cameraPosition - playerPosition;

        smoothMinX = Math.Min(relativePosition.X, TargetConstraints.MinX);
        smoothMaxX = Math.Max(relativePosition.X, TargetConstraints.MaxX);
        smoothMinY = Math.Min(relativePosition.Y, TargetConstraints.MinY);
        smoothMaxY = Math.Max(relativePosition.Y, TargetConstraints.MaxY);
    }

    public Vector2 Constrain(Vector2 cameraPosition, Vector2 playerPosition, Vector2 cameraTarget) {
        float t = TargetConstraints.Smoothing > 0f ? (float) Math.Exp(-Engine.DeltaTime / TargetConstraints.Smoothing) : 0f;

        if (TargetConstraints.HasMinX)
            cameraPosition.X = Math.Max(cameraPosition.X, Math.Min(playerPosition.X + MathHelper.Lerp(TargetConstraints.MinX, smoothMinX, t), cameraTarget.X));

        if (TargetConstraints.HasMaxX)
            cameraPosition.X = Math.Min(cameraPosition.X, Math.Max(playerPosition.X + MathHelper.Lerp(TargetConstraints.MaxX, smoothMaxX, t), cameraTarget.X));

        if (TargetConstraints.HasMinY)
            cameraPosition.Y = Math.Max(cameraPosition.Y, Math.Min(playerPosition.Y + MathHelper.Lerp(TargetConstraints.MinY, smoothMinY, t), cameraTarget.Y));

        if (TargetConstraints.HasMaxY)
            cameraPosition.Y = Math.Min(cameraPosition.Y, Math.Max(playerPosition.Y + MathHelper.Lerp(TargetConstraints.MaxY, smoothMaxY, t), cameraTarget.Y));

        return cameraPosition;
    }
}