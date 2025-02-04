using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/clipPreventionTrigger"), Tracked]
public class ClipPreventionTrigger : Entity {
    private bool right;
    private bool left;
    private bool up;
    private bool down;
    private bool applyToEntities;
    private float width;
    private float height;

    public ClipPreventionTrigger(EntityData data, Vector2 offset) : base(data.Position + offset) {
        right = data.Bool("right");
        left = data.Bool("left");
        up = data.Bool("up");
        down = data.Bool("down");
        applyToEntities = data.Bool("applyToEntities");
        width = data.Width;
        height = data.Height;

        if (!applyToEntities) {
            Collider = new Hitbox(width, height);
            Add(new ClipPrevention(right, left, up, down, Collider));
        }

        Visible = false;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        if (!applyToEntities)
            return;

        foreach (var entity in scene.Entities) {
            if (entity is Solid)
                continue;

            var relativePosition = entity.Position - Position;

            if (relativePosition.X >= 0f && relativePosition.X <= width && relativePosition.Y >= 0f && relativePosition.Y <= height)
                entity.Add(new ClipPrevention(right, left, up, down, entity.Components.Get<Holdable>()?.PickupCollider ?? entity.Collider));
        }

        RemoveSelf();
    }
}