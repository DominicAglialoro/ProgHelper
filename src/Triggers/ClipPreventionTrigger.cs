using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/clipPreventionTrigger")]
public class ClipPreventionTrigger : Trigger {
    private readonly bool right;
    private readonly bool left;
    private readonly bool up;
    private readonly bool down;
    private readonly bool applyToEntities;
    private readonly bool applyToPickupColliders;
    private readonly HashSet<string> entityTypes;
    private readonly float width;
    private readonly float height;

    public ClipPreventionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        right = data.Bool("right");
        left = data.Bool("left");
        up = data.Bool("up");
        down = data.Bool("down");
        applyToEntities = data.Bool("applyToEntities");
        applyToPickupColliders = data.Bool("applyToPickupColliders");
        width = data.Width;
        height = data.Height;

        if (!applyToEntities && !applyToPickupColliders) {
            Collider = new Hitbox(width, height);
            Add(new ClipPrevention(right, left, up, down, Collider));
        }

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

        if (!applyToEntities && !applyToPickupColliders)
            return;

        foreach (var entity in scene.Entities) {
            if (entity is Solid or Trigger)
                continue;

            var relativePosition = entity.Position - Position;

            if (relativePosition.X < 0f || relativePosition.X > width || relativePosition.Y < 0f || relativePosition.Y > height
                || entityTypes != null && !entityTypes.Contains(entity.GetType().FullName))
                continue;

            if (applyToEntities)
                ApplyToCollider(entity, entity.Collider);

            if (applyToPickupColliders)
                ApplyToCollider(entity, entity.Components.Get<Holdable>()?.PickupCollider);
        }

        RemoveSelf();
    }

    private void ApplyToCollider(Entity entity, Collider collider) {
        if (collider != null)
            entity.Add(new ClipPrevention(right, left, up, down, collider));
    }
}