using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.ProgHelper;

public static class TileGridExtensions {
    public static void Load() => IL.Monocle.TileGrid.RenderAt += TileGrid_RenderAt_il;

    public static void Unload() => IL.Monocle.TileGrid.RenderAt -= TileGrid_RenderAt_il;

    private static int[,] GetPulseIndices(TileGrid tileGrid)
        => tileGrid.Entity.Components.Get<TilePulseData>()?.TilePulseIndices;

    private static Color GetColor(Color color, TileGrid tileGrid, int x, int y, int[,] pulseIndices) {
        if (pulseIndices == null)
            return color;

        int pulseIndex = pulseIndices[x, y];

        if (pulseIndex < 0)
            return color;

        var modSession = ProgHelperModule.Session;
        float interp = 1f - MathHelper.Clamp(Util.Mod(tileGrid.Scene.TimeActive - pulseIndex * modSession.TilePulseStep, modSession.TilePulseInterval) / modSession.TilePulseLength, 0f, 1f);

        return Util.MultiplyKeepAlpha(color, MathHelper.Lerp(modSession.TilePulseBaseBrightness, 1f, interp * interp));
    }

    private static void TileGrid_RenderAt_il(ILContext il) {
        var cursor = new ILCursor(il);
        var pulseIndices = new VariableDefinition(il.Import(typeof(int[,])));

        il.Body.Variables.Add(pulseIndices);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(GetPulseIndices);
        cursor.Emit(OpCodes.Stloc, pulseIndices);

        cursor.GotoNext(instr => instr.MatchCallvirt<SpriteBatch>("Draw"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, il.Body.Variables[6]);
        cursor.Emit(OpCodes.Ldloc_S, il.Body.Variables[7]);
        cursor.Emit(OpCodes.Ldloc, pulseIndices);
        cursor.EmitCall(GetColor);
    }
}