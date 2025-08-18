using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/colliderEnlarger", "progHelper/colliderResizer")]
public class ColliderEnlarger : Trigger {
    private readonly float hitboxLeft;
    private readonly float hitboxRight;
    private readonly float hitboxTop;
    private readonly float hitboxBottom;
    private readonly float circleRadius;
    private readonly HashSet<string> entityTypes;
    private readonly bool resizePickupCollider;

    public ColliderEnlarger(EntityData data, Vector2 offset) : base(data, offset) {
        hitboxLeft = data.Float("hitboxLeft");
        hitboxRight = data.Float("hitboxRight");
        hitboxTop = data.Float("hitboxTop");
        hitboxBottom = data.Float("hitboxBottom");
        circleRadius = data.Float("circleRadius");
        resizePickupCollider = data.Bool("resizePickupCollider");
        entityTypes = data.Set("entityTypes");
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        foreach (var entity in scene.Entities) {
            if (entity is Trigger or SolidTiles or BackgroundTiles
                || !this.ContainsEntity(entity)
                || entityTypes != null && !entityTypes.Contains(entity.GetType().FullName))
                continue;

            if (resizePickupCollider)
                Enlarge(entity.Components.Get<Holdable>()?.PickupCollider);
            else
                Enlarge(entity.Collider);
        }

        RemoveSelf();
    }

    private void Enlarge(Collider collider) {
        switch (collider) {
            case Hitbox hitbox: {
                hitbox.Position -= new Vector2(hitboxLeft, hitboxTop);
                hitbox.Width += hitboxLeft + hitboxRight;
                hitbox.Height += hitboxTop + hitboxBottom;

                break;
            }
            case Circle circle: {
                circle.Radius += circleRadius;

                break;
            }
            case ColliderList colliderList: {
                foreach (var subCollider in colliderList.colliders)
                    Enlarge(subCollider);

                break;
            }
        }
    }
}