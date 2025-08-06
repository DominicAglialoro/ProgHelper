using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.ProgHelper;

public static class ActorExtensions {
    public static void Load() {
        IL.Celeste.Actor.MoveHExact += Il_Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Il_Actor_MoveVExact;
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

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall<Entity>("set_X"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(StrictPlayerCollider.Check);
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

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall<Entity>("set_Y"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(StrictPlayerCollider.Check);
    }
}