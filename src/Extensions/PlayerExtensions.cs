using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class PlayerExtensions {
    private static ILHook il_Celeste_Player_Orig_Update;

    public static void Load() {
        il_Celeste_Player_Orig_Update = new ILHook(typeof(Player).GetMethodUnconstrained(nameof(Player.orig_Update)), Player_orig_Update_il);
        On.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.ClimbJump += Player_ClimbJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
    }

    public static void Unload() {
        il_Celeste_Player_Orig_Update.Dispose();
        On.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.WallJump -= Player_WallJump;
        On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
    }

    private static void DestroyCrumbleBlockOnJump(this Player player, Vector2 dir) {
        var solid = player.CollideFirst<Solid>(player.Position + dir);

        if (solid is CrumbleBlockOnJump block)
            block.Triggered = true;
    }

    private static void CheckForDisableCoyoteJump(Player player) {
        if (player.CollideCheck<DisableCoyoteJumpTrigger>())
            DynamicData.For(player).Set("jumpGraceTimer", 0f);
    }

    private static Vector2 ApplyCameraConstraints(Vector2 value, Player player) {
        if (DynamicData.For(player).TryGet("cameraConstraints", out CameraConstraints cameraConstraints)) {
            if (cameraConstraints.HasMinX)
                value.X = Math.Max(value.X, player.Position.X + cameraConstraints.MinX - 160f);

            if (cameraConstraints.HasMaxX)
                value.X = Math.Min(value.X, player.Position.X + cameraConstraints.MaxX - 160f);

            if (cameraConstraints.HasMinY)
                value.Y = Math.Max(value.Y, player.Position.Y + cameraConstraints.MinY - 90f);

            if (cameraConstraints.HasMaxY)
                value.Y = Math.Min(value.Y, player.Position.Y + cameraConstraints.MaxY - 90f);
        }

        foreach (var entity in player.Scene.Tracker.GetEntities<CameraHardBorder>())
            value = ((CameraHardBorder) entity).Constrain(value, player);

        if (!player.EnforceLevelBounds)
            return value;

        var bounds = player.SceneAs<Level>().Bounds;

        value.X = MathHelper.Clamp(value.X, bounds.Left, bounds.Right - 320f);
        value.Y = MathHelper.Clamp(value.Y, bounds.Top, bounds.Bottom - 180f);

        return value;
    }

    private static void Player_orig_Update_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Sub,
            instr => instr.MatchStfld<Player>("jumpGraceTimer"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(PlayerExtensions).GetMethodUnconstrained(nameof(CheckForDisableCoyoteJump)));

        cursor.Index = -1;
        cursor.GotoPrev(instr => instr.MatchCallvirt<Camera>("set_Position"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(PlayerExtensions).GetMethodUnconstrained(nameof(ApplyCameraConstraints)));

        cursor.GotoPrev(MoveType.After,
            instr => instr.MatchCall<Actor>(nameof(Actor.MoveH)),
            instr => instr.OpCode == OpCodes.Pop);

        cursor.Emit(OpCodes.Call, typeof(ClipPreventionTrigger).GetMethodUnconstrained(nameof(ClipPreventionTrigger.EndTest)));

        cursor.GotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Beq_S);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(ClipPreventionTrigger).GetMethodUnconstrained(nameof(ClipPreventionTrigger.BeginTestH)));

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchCall<Actor>(nameof(Actor.MoveV)),
            instr => instr.OpCode == OpCodes.Pop);

        cursor.Emit(OpCodes.Call, typeof(ClipPreventionTrigger).GetMethodUnconstrained(nameof(ClipPreventionTrigger.EndTest)));

        cursor.GotoPrev(MoveType.After, instr => instr.OpCode == OpCodes.Beq_S);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(ClipPreventionTrigger).GetMethodUnconstrained(nameof(ClipPreventionTrigger.BeginTestV)));
    }

    private static bool Player_WallJumpCheck(On.Celeste.Player.orig_WallJumpCheck wallJumpCheck, Player player, int dir) =>
        wallJumpCheck(player, dir)
        && (player.StateMachine.State != 2 || player.DashDir.Y <= 0f || !player.CollideCheck<WavedashProtectionTrigger>());

    private static void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
        jump(player, particles, playsfx);
        player.DestroyCrumbleBlockOnJump(Vector2.UnitY);
    }

    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump superJump, Player player) {
        superJump(player);
        player.DestroyCrumbleBlockOnJump(Vector2.UnitY);
    }

    private static void Player_WallJump(On.Celeste.Player.orig_WallJump wallJump, Player player, int dir) {
        wallJump(player, dir);
        player.DestroyCrumbleBlockOnJump(-3 * dir * Vector2.UnitX);
    }

    private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump climbJump, Player player) {
        climbJump(player);
        player.DestroyCrumbleBlockOnJump(3 * (int) Input.MoveX * Vector2.UnitX);
    }

    private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump superWallJump, Player player, int dir) {
        superWallJump(player, dir);
        player.DestroyCrumbleBlockOnJump(-5 * dir * Vector2.UnitX);
    }
}