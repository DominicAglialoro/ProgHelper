using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

[CustomEntity("progHelper/wavedashProtectionTrigger"), Tracked]
public class WavedashProtectionTrigger : Entity {
    public WavedashProtectionTrigger(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Collider = new Hitbox(data.Width, data.Height);
        Visible = false;
    }
}