using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity(
     "progHelper/strictSpikesUp = LoadUp",
     "progHelper/strictSpikesDown = LoadDown",
     "progHelper/strictSpikesLeft = LoadLeft",
     "progHelper/strictSpikesRight = LoadRight"), TrackedAs(typeof(Spikes))]
public class StrictSpikes : Spikes {
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpikes(entityData, offset, Directions.Up);

    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpikes(entityData, offset, Directions.Down);

    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpikes(entityData, offset, Directions.Left);

    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        => new StrictSpikes(entityData, offset, Directions.Right);

    public StrictSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir) {
        Remove(pc);
        Add(new StrictPlayerCollider(OnCollide));
    }

    public new bool OnCollide(Player player) {
        switch (Direction) {
            case Directions.Up:
                if (player.Speed.Y < 0f || player.Bottom > Bottom)
                    return false;

                player.Die(new Vector2(0f, -1f));

                return true;
            case Directions.Down:
                if (player.Speed.Y > 0f)
                    return false;

                player.Die(new Vector2(0f, 1f));

                return true;
            case Directions.Left:
                if (player.Speed.X < 0f)
                    return false;

                player.Die(new Vector2(-1f, 0f));

                return true;
            case Directions.Right:
                if (player.Speed.X > 0f)
                    return false;

                player.Die(new Vector2(1f, 0f));

                return true;
            default:
                return false;
        }
    }
}