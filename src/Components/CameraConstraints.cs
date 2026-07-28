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

    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    public CameraConstraints() : base(true, false) { }

    public override void Update() {
        base.Update();

        if (TargetConstraints.Smoothing <= 0f)
            return;

        float t = (float) Math.Exp(-Engine.DeltaTime / TargetConstraints.Smoothing);

        if (TargetConstraints.HasMinX)
            MinX = MathHelper.Lerp(TargetConstraints.MinX, MinX, t);

        if (TargetConstraints.HasMaxX)
            MaxX = MathHelper.Lerp(TargetConstraints.MaxX, MaxX, t);

        if (TargetConstraints.HasMinY)
            MinY = MathHelper.Lerp(TargetConstraints.MinY, MinY, t);

        if (TargetConstraints.HasMaxY)
            MaxY = MathHelper.Lerp(TargetConstraints.MaxY, MaxY, t);
    }
}