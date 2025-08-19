using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity(
     "progHelper/strictSpring = LoadFloor",
     "progHelper/strictWallSpringLeft = LoadWallLeft",
     "progHelper/strictWallSpringRight = LoadWallRight")]
public class StrictSpring : Spring {
    public static Entity LoadFloor(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpring(entityData, offset, Orientations.Floor);

    public static Entity LoadWallLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpring(entityData, offset, Orientations.WallLeft);

    public static Entity LoadWallRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpring(entityData, offset, Orientations.WallRight);

    public StrictSpring(EntityData data, Vector2 offset, Orientations orientation) : base(data, offset, orientation) {
        Remove(Components.Get<PlayerCollider>());
        Add(new StrictPlayerCollider(OnCollide));
    }

    public new bool OnCollide(Player player) {
        if (player.StateMachine.State == Player.StDreamDash || !playerCanUse)
            return false;

        switch (Orientation) {
            case Orientations.Floor: {
                if (player.Speed.Y < 0.0)
                    return false;

                BounceAnimate();
                player.SuperBounce(Top);

                return true;
            }
            case Orientations.WallLeft: {
                if (!player.SideBounce(1, Right, CenterY))
                    return false;

                BounceAnimate();

                return true;
            }
            case Orientations.WallRight: {
                if (!player.SideBounce(-1, Left, CenterY))
                    return false;

                BounceAnimate();

                return true;
            }
            default:
                return false;
        }
    }
}