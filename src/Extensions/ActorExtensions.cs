using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.ProgHelper;

public static class ActorExtensions {
    public static void Load() {
        On.Celeste.Actor.NaiveMove += Actor_NaiveMove;
        IL.Celeste.Actor.MoveHExact += Il_Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Il_Actor_MoveVExact;
    }

    public static void Unload() {
        On.Celeste.Actor.NaiveMove -= Actor_NaiveMove;
        IL.Celeste.Actor.MoveHExact -= Il_Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Il_Actor_MoveVExact;
    }

    private static void Actor_NaiveMove(On.Celeste.Actor.orig_NaiveMove naiveMove, Actor actor, Vector2 amount) {
        if (actor is not Player player || player.StateMachine.State != Player.StDreamDash) {
            naiveMove(actor, amount);

            return;
        }

        naiveMove(actor, amount);

        var controller = player.Scene.Tracker.GetEntity<NegativeSpaceController>();

        if (controller == null || !controller.CheckForSwap())
            return;

        if (controller.FlipsGravity)
            GravityHelperImports.SetPlayerGravity?.Invoke(2, 1f);
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