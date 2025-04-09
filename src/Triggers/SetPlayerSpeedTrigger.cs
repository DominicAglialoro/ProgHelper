using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/setPlayerSpeedTrigger")]
public class SetPlayerSpeedTrigger : Trigger {
    private readonly float speedX;
    private readonly float speedY;
    private readonly SetPlayerSpeedMode modeX;
    private readonly SetPlayerSpeedMode modeY;
    private readonly float rangeXMin;
    private readonly float rangeXMax;
    private readonly float rangeYMin;
    private readonly float rangeYMax;

    public SetPlayerSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        modeX = data.Enum<SetPlayerSpeedMode>("modeX");
        modeY = data.Enum<SetPlayerSpeedMode>("modeY");
        speedX = data.Float("speedX");
        speedY = data.Float("speedY");
        rangeXMin = data.Float("rangeXMin");
        rangeXMax = data.Float("rangeXMax");
        rangeYMin = data.Float("rangeYMin");
        rangeYMax = data.Float("rangeYMax");
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);

        bool xInRange = player.Speed.X >= rangeXMin && player.Speed.X <= rangeXMax;
        bool yInRange = player.Speed.Y >= rangeYMin && player.Speed.Y <= rangeYMax;

        if (modeX == SetPlayerSpeedMode.Always || modeX == SetPlayerSpeedMode.InRange && xInRange || modeX == SetPlayerSpeedMode.BothInRange && xInRange && yInRange)
            player.Speed.X = speedX;

        if (modeY == SetPlayerSpeedMode.Always || modeY == SetPlayerSpeedMode.InRange && yInRange || modeY == SetPlayerSpeedMode.BothInRange && xInRange && yInRange)
            player.Speed.Y = speedY;
    }
}