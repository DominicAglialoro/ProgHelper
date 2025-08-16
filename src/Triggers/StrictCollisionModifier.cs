using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/strictCollisionModifier")]
public class StrictCollisionModifier : Trigger {
    private readonly HashSet<string> entityTypes;
    private readonly float width;
    private readonly float height;

    public StrictCollisionModifier(EntityData data, Vector2 offset) : base(data, offset) {
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

        foreach (var entity in scene.Entities) {
            if (entity is Solid or Trigger)
                continue;

            var relativePosition = entity.Position - Position;

            if (relativePosition.X < 0f || relativePosition.X > width || relativePosition.Y < 0f || relativePosition.Y > height
                || entityTypes != null && !entityTypes.Contains(entity.GetType().FullName))
                continue;

            var playerCollider = entity.Components.Get<PlayerCollider>();
            var onCollide = playerCollider?.OnCollide;

            if (onCollide == null)
                continue;

            entity.Remove(playerCollider);
            entity.Add(new StrictPlayerCollider(player => {
                onCollide(player);

                return false;
            }));
        }

        RemoveSelf();
    }
}