using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper; 

public static class PlayerExtensions {
    public static void Load() {
        On.Celeste.Player.Added += Player_Added;
        On.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    public static void Unload() {
        On.Celeste.Player.Added -= Player_Added;
        On.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }

    private static void CheckForLiftboost(this Player player, Vector2 dir) {
        if (player.LiftSpeed != Vector2.Zero)
            return;

        var solid = player.CollideFirst<Solid>(player.Position + dir);

        if (solid != null)
            player.LiftSpeed = solid.LiftSpeed;
    }

    private static void Player_Added(On.Celeste.Player.orig_Added added, Player player, Scene scene) {
        added(player, scene);
        Input.Grab.BufferTime = ProgHelperModule.Session.BufferableGrab ? 0.08f : 0f;
    }

    private static bool Player_WallJumpCheck(On.Celeste.Player.orig_WallJumpCheck wallJumpCheck, Player player, int dir) =>
        wallJumpCheck(player, dir)
        && (player.StateMachine.State != 2 || player.DashDir.Y <= 0f || Math.Sign(player.DashDir.X) != Math.Sign(dir) || !player.CollideCheck<WavedashProtectionTrigger>());

    private static void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
        if (ProgHelperModule.Session.LiftboostProtection)
            player.CheckForLiftboost(Vector2.UnitY);

        if (ProgHelperModule.Session.UltraProtection
            && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
            && DynamicData.For(player).Get<bool>("onGround")) {
            player.DashDir.X = Math.Sign(player.DashDir.X);
            player.DashDir.Y = 0f;
            player.Speed.X *= 1.2f;
        }
        
        jump(player, particles, playsfx);
    }

    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump superJump, Player player) {
        if (ProgHelperModule.Session.LiftboostProtection)
            player.CheckForLiftboost(Vector2.UnitY);
        
        superJump(player);
    }

    private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump superWallJump, Player player, int dir) {
        if (ProgHelperModule.Session.LiftboostProtection)
            player.CheckForLiftboost(-dir * (player.DashAttacking && player.DashDir.X == 0f && player.DashDir.Y == -1f ? 5 : 3) * Vector2.UnitX);
        
        superWallJump(player, dir);
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin dashBegin, Player player) {
        if (ProgHelperModule.Session.UltraProtection
            && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
            && DynamicData.For(player).Get<bool>("onGround"))
            player.Speed.X *= 1.2f;

        dashBegin(player);
    }
}