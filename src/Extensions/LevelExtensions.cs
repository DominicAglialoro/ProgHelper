using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

public static class LevelExtensions {
    public static void Load() {
        On.Celeste.Level.TeleportTo += Level_TeleportTo;
    }

    public static void Unload() {
        On.Celeste.Level.TeleportTo -= Level_TeleportTo;
    }

    private static void Level_TeleportTo(On.Celeste.Level.orig_TeleportTo teleportTo, Level level, Player player, string nextlevel, Player.IntroTypes introtype, Vector2? nearestspawn) {
        teleportTo(level, player, nextlevel, introtype, nearestspawn);

        if (level.Tracker.GetEntity<Player>() is not { } newPlayer)
            return;

        var teleportFacingTrigger = newPlayer.CollideFirst<TeleportFacingTrigger>();

        if (teleportFacingTrigger == null)
            return;

        newPlayer.Facing = teleportFacingTrigger.Facing;
    }
}