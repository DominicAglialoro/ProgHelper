using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/setPlayerSpeedTrigger")]
public class SetPlayerSpeedTrigger : Trigger {
    private readonly Vector2 speed;

    public SetPlayerSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset)
        => speed = new Vector2(data.Float("speedX"), data.Float("speedY"));

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        player.Speed = speed;
    }
}