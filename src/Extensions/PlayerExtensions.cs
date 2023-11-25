using System;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

public static class PlayerExtensions {
    public static void Load() {
        On.Celeste.Player.Added += Player_Added;
        On.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
    }

    public static void Unload() {
        On.Celeste.Player.Added -= Player_Added;
        On.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
    }

    private static void Player_Added(On.Celeste.Player.orig_Added added, Player player, Scene scene) {
        added(player, scene);
        Input.Grab.BufferTime = ProgHelperModule.Session.BufferableGrab ? 0.08f : 0f;
    }

    private static bool Player_WallJumpCheck(On.Celeste.Player.orig_WallJumpCheck wallJumpCheck, Player player, int dir) {
        if (player.StateMachine.State == 2 && player.DashDir.Y > 0f && Math.Sign(player.DashDir.X) == Math.Sign(dir) && player.CollideCheck<WavedashProtectionTrigger>())
            return false;

        return wallJumpCheck(player, dir);
    }
}