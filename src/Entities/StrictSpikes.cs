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
}