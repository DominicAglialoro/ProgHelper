using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class PlayerExtensions {
    private static ILHook il_Celeste_Player_orig_Update;

    public static void Load() {
        il_Celeste_Player_orig_Update = typeof(Player).CreateHook(nameof(Player.orig_Update), Player_orig_Update_il);
        On.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.ClimbJump += Player_ClimbJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        On.Celeste.Player.RefillDash += Player_RefillDash;
        On.Celeste.Player.UseRefill += Player_UseRefill;
        IL.Celeste.Player.NormalUpdate += Player_NormalUpdate_il;
        IL.Celeste.Player.DashUpdate += Player_DashUpdate_il;
        IL.Celeste.Player.LaunchUpdate += Player_LaunchUpdate_il;
    }

    public static void Unload() {
        il_Celeste_Player_orig_Update.Dispose();
        On.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.WallJump -= Player_WallJump;
        On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        On.Celeste.Player.RefillDash -= Player_RefillDash;
        On.Celeste.Player.UseRefill -= Player_UseRefill;
        IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate_il;
        IL.Celeste.Player.DashUpdate -= Player_DashUpdate_il;
        IL.Celeste.Player.LaunchUpdate -= Player_LaunchUpdate_il;
    }

    private static Vector2 GetGravityDirection() {
        if (GravityHelperImports.IsPlayerInverted == null)
            return Vector2.UnitY;

        return GravityHelperImports.IsPlayerInverted.Invoke() ? -Vector2.UnitY : Vector2.UnitY;
    }

    private static void DestroyCrumbleBlockOnJump(this Player player, Vector2 dir)
        => player.CollideFirst<CrumbleBlockOnJump>(player.Position + dir)?.Break();

    private static void DestroyCrumbleJumpThruOnJump(this Player player)
        => player.CollideFirstOutside<CrumbleJumpThruOnJump>(player.Position + Vector2.UnitY)?.Break();

    private static void CheckForDisableCoyoteJump(Player player) {
        if (player.CollideCheck<DisableCoyoteJumpTrigger>())
            player.jumpGraceTimer = 0f;
    }

    private static Vector2 ApplyCameraConstraints(Vector2 value, Player player, Vector2 cameraTarget) {
        var bounds = player.SceneAs<Level>().Bounds;

        if (DynamicData.For(player).TryGet("programmatic.ProgHelper.CameraConstraints", out CameraConstraints cameraConstraints)) {
            if (cameraConstraints.HasMinX) {
                value.X = Math.Max(value.X, player.Position.X + cameraConstraints.MinX - 160f);

                if (player.EnforceLevelBounds)
                    value.X = Math.Min(value.X, Math.Max(bounds.Right - 320f, cameraTarget.X));
            }

            if (cameraConstraints.HasMaxX) {
                value.X = Math.Min(value.X, player.Position.X + cameraConstraints.MaxX - 160f);

                if (player.EnforceLevelBounds)
                    value.X = Math.Max(value.X, Math.Min(bounds.Left, cameraTarget.X));
            }

            if (cameraConstraints.HasMinY) {
                value.Y = Math.Max(value.Y, player.Position.Y + cameraConstraints.MinY - 90f);

                if (player.EnforceLevelBounds)
                    value.Y = Math.Min(value.Y, Math.Max(bounds.Bottom - 180f, cameraTarget.Y));
            }

            if (cameraConstraints.HasMaxY) {
                value.Y = Math.Min(value.Y, player.Position.Y + cameraConstraints.MaxY - 90f);

                if (player.EnforceLevelBounds)
                    value.Y = Math.Max(value.Y, Math.Min(bounds.Top, cameraTarget.Y));
            }
        }

        foreach (CameraHardBorder entity in player.Scene.Tracker.GetEntities<CameraHardBorder>())
            value = entity.Constrain(value, player);

        return value;
    }

    private static bool TryWarpToThrowablePortal(Player player) {
        int state = player.StateMachine.State;

        if (state == 2 && player.DashDir == Vector2.Zero || player.Holding != null || !Input.GrabCheck || player.IsTired)
            return false;

        var portal = player.Scene.Tracker.GetEntity<ThrowablePortal>();

        return portal != null && portal.TryUseWarp(player);
    }

    private static void Player_orig_Update_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchCall<Engine>("get_DeltaTime"),
            instr => instr.OpCode == OpCodes.Sub,
            instr => instr.MatchStfld<Player>("jumpGraceTimer"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(CheckForDisableCoyoteJump);

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchLdfld<Player>("onCollideH"),
            instr => instr.OpCode == OpCodes.Ldnull);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(ClipPrevention.BeginTestH);

        cursor.Index += 2;

        cursor.EmitCall(ClipPrevention.EndTest);

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchLdfld<Player>("onCollideV"),
            instr => instr.OpCode == OpCodes.Ldnull);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(ClipPrevention.BeginTestV);

        cursor.Index += 2;

        cursor.EmitCall(ClipPrevention.EndTest);

        ILLabel label = null;

        cursor.GotoNext(instr => instr.MatchLdfld<Player>("ForceCameraUpdate"));
        cursor.GotoNext(
            instr => instr.MatchCallvirt<StateMachine>("get_State"),
            instr => instr.MatchLdcI4(18));
        cursor.GotoNext(instr => instr.MatchBneUn(out label));
        cursor.GotoLabel(label);
        cursor.GotoNext(instr => instr.MatchCallvirt<Camera>("set_Position"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, il.Body.Variables[12]);
        cursor.EmitCall(ApplyCameraConstraints);
    }

    private static bool Player_WallJumpCheck(On.Celeste.Player.orig_WallJumpCheck wallJumpCheck, Player player, int dir) =>
        wallJumpCheck(player, dir)
        && (player.StateMachine.State != 2 || player.DashDir.X == 0f || player.DashDir.Y <= 0f || !player.CollideCheck<WavedashProtectionTrigger>());

    private static void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
        jump(player, particles, playsfx);
        player.DestroyCrumbleBlockOnJump(GetGravityDirection());
        player.DestroyCrumbleJumpThruOnJump();
    }

    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump superJump, Player player) {
        superJump(player);
        player.DestroyCrumbleBlockOnJump(GetGravityDirection());
        player.DestroyCrumbleJumpThruOnJump();
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

    private static bool Player_RefillDash(On.Celeste.Player.orig_RefillDash refillDash, Player player)
        => refillDash(player) | (player.Holding?.Entity is ThrowablePortal portal && portal.RestoreWarp());

    private static bool Player_UseRefill(On.Celeste.Player.orig_UseRefill useRefill, Player player, bool twodashes)
        => useRefill(player, twodashes) | (player.Holding?.Entity is ThrowablePortal portal && portal.RestoreWarp());

    private static void Player_NormalUpdate_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchCallvirt<Player>("StartDash"),
            instr => instr.OpCode == OpCodes.Ret);
        cursor.MoveAfterLabels();

        var label = cursor.DefineLabel();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(TryWarpToThrowablePortal);
        cursor.Emit(OpCodes.Brfalse_S, label);
        cursor.Emit(OpCodes.Ldc_I4_8);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
    }

    private static void Player_DashUpdate_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(
            instr => instr.OpCode == OpCodes.Ldc_I4_8,
            instr => instr.OpCode == OpCodes.Stloc_S);
        cursor.GotoNext(instr => instr.MatchLdflda<Player>("DashDir"));

        var label = cursor.DefineLabel();

        cursor.EmitCall(TryWarpToThrowablePortal);
        cursor.Emit(OpCodes.Brfalse, label);
        cursor.Emit(OpCodes.Ldc_I4_8);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
        cursor.Emit(OpCodes.Ldarg_0);
    }

    private static void Player_LaunchUpdate_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(
            instr => instr.OpCode == OpCodes.Ldc_I4_8,
            instr => instr.OpCode == OpCodes.Stloc_2);
        cursor.GotoNext(instr => instr.MatchLdflda<Player>("Speed"));

        var label = cursor.DefineLabel();

        cursor.EmitCall(TryWarpToThrowablePortal);
        cursor.Emit(OpCodes.Brfalse, label);
        cursor.Emit(OpCodes.Ldc_I4_8);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
        cursor.Emit(OpCodes.Ldarg_0);
    }
}