using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/onlyLoadIfFlagTrigger")]
public class OnlyLoadIfFlagTrigger : Trigger {
    private readonly string flag;
    private readonly bool invert;
    private readonly HashSet<string> entityTypes;
    private readonly float width;
    private readonly float height;

    public OnlyLoadIfFlagTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        flag = data.Attr("flag");
        invert = data.Bool("invert");
        width = data.Width;
        height = data.Height;

        string entityTypesAttr = data.Attr("entityTypes");

        if (!string.IsNullOrWhiteSpace(entityTypesAttr)) {
            entityTypes = new HashSet<string>();

            foreach (string sub in entityTypesAttr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                entityTypes.Add(sub);
        }

        Visible = false;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        if (SceneAs<Level>().Session.GetFlag(flag) == invert) {
            RemoveSelf();

            return;
        }

        foreach (var entity in scene.Entities) {
            if (entity is Trigger)
                continue;

            var relativePosition = entity.Position - Position;

            if (relativePosition.X >= 0f && relativePosition.X <= width && relativePosition.Y >= 0f && relativePosition.Y <= height
                && (entityTypes == null || entityTypes.Contains(entity.GetType().FullName)))
                entity.RemoveSelf();
        }

        RemoveSelf();
    }
}