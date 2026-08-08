using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

public static class LevelExtensions {
    public static void Load() {
        On.Celeste.Level.Update += Level_Update;
        On.Celeste.Level.TeleportTo += Level_TeleportTo;
    }

    public static void Unload() {
        On.Celeste.Level.Update -= Level_Update;
        On.Celeste.Level.TeleportTo -= Level_TeleportTo;
    }

    private static void Level_Update(On.Celeste.Level.orig_Update update, Level level) {
        update(level);

        if (level.FrozenOrPaused || level.SkippingCutscene || level.Transitioning)
            return;

        var player = level.Tracker.GetEntity<Player>();

        if (player == null)
            return;

        var cameraPosition = level.Camera.Position;
        var cameraConstraints = player.Components.Get<CameraConstraints>();

        if ((player.InControl || player.ForceCameraUpdate) && player.StateMachine.State != Player.StReflectionFall) {
            if (cameraConstraints != null) {
                var cameraTarget = player.CameraTarget;

                cameraPosition = cameraConstraints.Constrain(cameraPosition, player.Position, cameraTarget);

                if (player.EnforceLevelBounds) {
                    var bounds = level.Bounds;

                    cameraPosition.X = MathHelper.Clamp(cameraPosition.X, Math.Min(bounds.Left, cameraTarget.X), Math.Max(bounds.Right - 320f, cameraTarget.X));
                    cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, Math.Min(bounds.Top, cameraTarget.Y), Math.Max(bounds.Bottom - 180f, cameraTarget.Y));
                }
            }

            foreach (CameraHardBorder border in level.Tracker.GetEntities<CameraHardBorder>())
                cameraPosition = border.Constrain(cameraPosition, player);

            level.Camera.Position = cameraPosition;
        }

        cameraConstraints?.UpdateSmoothConstraints(cameraPosition, player.Position);
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