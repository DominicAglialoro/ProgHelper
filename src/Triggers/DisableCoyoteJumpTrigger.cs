using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/disableCoyoteJumpTrigger"), Tracked]
public class DisableCoyoteJumpTrigger : Trigger {
    public DisableCoyoteJumpTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Collider = new Hitbox(data.Width, data.Height);
        Visible = false;
    }
}