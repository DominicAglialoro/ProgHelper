using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/strictCollisionModifier")]
public class StrictCollisionModifier : Trigger {
    private readonly HashSet<string> entityTypes;

    public StrictCollisionModifier(EntityData data, Vector2 offset) : base(data, offset) => entityTypes = data.Set("entityTypes");

    public override void Awake(Scene scene) {
        base.Awake(scene);

        foreach (var entity in scene.Entities) {
            if (entity is Platform or Trigger or BackgroundTiles
                || !this.ContainsEntity(entity)
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