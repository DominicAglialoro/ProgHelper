using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class ActorExtensions {
    public static void Load() {
        On.Celeste.Actor.NaiveMove += Actor_NaiveMove;
        IL.Celeste.Actor.MoveHExact += Il_Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Il_Actor_MoveVExact;
    }

    private static void Actor_NaiveMove(On.Celeste.Actor.orig_NaiveMove naiveMove, Actor actor, Vector2 amount) {
        naiveMove(actor, amount);

        if (actor is not Player player || player.Scene.Tracker.GetEntity<NegativeSpaceController>()?.CheckForSwap(true) is not true
            || !Input.GrabCheck || player.DashDir.X == 0f)
            return;

        var dynamicData = DynamicData.For(player);
        int moveX = dynamicData.Get<int>("moveX");

        if (moveX != -Math.Sign(player.DashDir.X) || !player.ClimbCheck(moveX))
            return;

        dynamicData.Set("dreamDashCanEndTimer", 0f);
        dynamicData.Set("communalHelperDreamTunnelDashCanEndTimer", 0f);
    }

    public static void Unload() {
        IL.Celeste.Actor.MoveHExact -= Il_Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Il_Actor_MoveVExact;
    }

    private static void Il_Actor_MoveHExact(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.Index = -1;
        cursor.GotoPrev(MoveType.AfterLabel, instr => instr.OpCode == OpCodes.Ldloc_2);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitCall(ClipPrevention.CheckH);

        var label = cursor.DefineLabel();

        cursor.Emit(OpCodes.Brfalse_S, label);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
    }

    private static void Il_Actor_MoveVExact(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.Index = -1;
        cursor.GotoPrev(MoveType.AfterLabel, instr => instr.OpCode == OpCodes.Ldloc_2);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitCall(ClipPrevention.CheckV);

        var label = cursor.DefineLabel();

        cursor.Emit(OpCodes.Brfalse_S, label);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
    }
}