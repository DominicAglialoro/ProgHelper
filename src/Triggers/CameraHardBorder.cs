﻿using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/cameraHardBorder"), Tracked]
public class CameraHardBorder : Trigger {
    private readonly bool left;
    private readonly bool right;
    private readonly bool top;
    private readonly bool bottom;
    private readonly string flag;
    private readonly bool inverted;

    public CameraHardBorder(EntityData data, Vector2 offset) : base(data, offset) {
        left = data.Bool("left");
        right = data.Bool("right");
        top = data.Bool("top");
        bottom = data.Bool("bottom");
        flag = data.Attr("flag");
        inverted = data.Bool("inverted");

        Collider = new Hitbox(data.Width, data.Height);
        Visible = false;
    }

    public Vector2 Constrain(Vector2 cameraPosition, Player player) {
        if (!Util.CheckFlag(flag, SceneAs<Level>().Session, inverted))
            return cameraPosition;

        if (left && player.Right <= Left && player.Bottom > Top && player.Top < Bottom)
            cameraPosition.X = Math.Min(cameraPosition.X, Left - 320f);

        if (right && player.Left >= Right && player.Bottom > Top && player.Top < Bottom)
            cameraPosition.X = Math.Max(cameraPosition.X, Right);

        if (top && player.Bottom <= Top && player.Right > Left && player.Left < Right)
            cameraPosition.Y = Math.Min(cameraPosition.Y, Top - 180f);

        if (bottom && player.Top >= Bottom && player.Right > Left && player.Left < Right)
            cameraPosition.Y = Math.Max(cameraPosition.Y, Bottom);

        return cameraPosition;
    }
}