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
        On.Celeste.Player.Added += Player_Added;
        il_Celeste_Player_Orig_Update = new ILHook(typeof(Player).GetMethodUnconstrained(nameof(Player.orig_Update)), Player_Orig_Update_il);
        On.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    public static void Unload() {
        On.Celeste.Player.Added -= Player_Added;
        il_Celeste_Player_Orig_Update.Dispose();
        On.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }

    private static void CheckForDisableCoyoteJump(Player player) {
        if (player.CollideCheck<DisableCoyoteJumpTrigger>())
            DynamicData.For(player).Set("jumpGraceTimer", 0f);
    }

    private static Vector2 ApplyCameraConstraints(Vector2 value, Player player) {
        if (!DynamicData.For(player).TryGet("cameraConstraints", out CameraConstraints cameraConstraints))
            return value;

        if (cameraConstraints.HasMinX)
            value.X = Math.Max(value.X, player.Position.X + cameraConstraints.MinX - 160f);
        
        if (cameraConstraints.HasMaxX)
            value.X = Math.Min(value.X, player.Position.X + cameraConstraints.MaxX - 160f);
        
        if (cameraConstraints.HasMinY)
            value.Y = Math.Max(value.Y, player.Position.Y + cameraConstraints.MinY - 90f);
        
        if (cameraConstraints.HasMaxY)
            value.Y = Math.Min(value.Y, player.Position.Y + cameraConstraints.MaxY - 90f);

        if (!player.EnforceLevelBounds)
            return value;

        var bounds = player.SceneAs<Level>().Bounds;
        
        value.X = MathHelper.Clamp(value.X, bounds.Left, bounds.Right - 320f);
        value.Y = MathHelper.Clamp(value.Y, bounds.Top, bounds.Bottom - 180f);

        return value;
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

    private static void Player_Orig_Update_il(ILContext il) {
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
            player.CheckForLiftboost(-5 * dir * Vector2.UnitX);
        
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