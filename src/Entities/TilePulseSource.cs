using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/tilePulseSource")]
public class TilePulseSource : Entity {
    public TilePulseSource(EntityData data, Vector2 offset) : base(data.Position + offset) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        RemoveSelf();
    }
}