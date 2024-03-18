using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class TileGridExtensions {
    public static void Load() => IL.Monocle.TileGrid.RenderAt += TileGrid_RenderAt_il;

    public static void Unload() => IL.Monocle.TileGrid.RenderAt -= TileGrid_RenderAt_il;

    public static void GeneratePulseIndices(this TileGrid tileGrid, VirtualMap<char> tiles, bool bg) {
        var level = tileGrid.SceneAs<Level>();

        if (level == null)
            return;

        var sources = new List<IntVector>();
        int width = tiles.Columns;
        int height = tiles.Rows;

        foreach (var levelData in level.Session.MapData.Levels) {
            foreach (var entityData in levelData.Entities) {
                if (entityData.Name != "progHelper/tilePulseSource" || entityData.Bool("bg") != bg)
                    continue;

                var source = new IntVector(
                    (int) (entityData.Position.X + levelData.Bounds.X - tileGrid.Entity.X) / 8,
                    (int) (entityData.Position.Y + levelData.Bounds.Y - tileGrid.Entity.Y) / 8);

                if (source.X >= 0 && source.X < width && source.Y >= 0 && source.Y < height
                    && tiles[source.X, source.Y] != '0' && tiles[source.X, source.Y] != char.MinValue)
                    sources.Add(source);
            }
        }

        if (sources.Count == 0)
            return;

        int[,] pulseIndices = new int[width, height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
                pulseIndices[i, j] = -1;
        }

        var toCheck = new Queue<IntVector>();

        foreach (var source in sources) {
            pulseIndices[source.X, source.Y] = 0;
            toCheck.Enqueue(source);
        }

        while (toCheck.Count > 0) {
            var vector = toCheck.Dequeue();
            char texture = tiles[vector.X, vector.Y];
            int pulseIndex = pulseIndices[vector.X, vector.Y] + 1;

            Check(vector.X - 1, vector.Y);
            Check(vector.X + 1, vector.Y);
            Check(vector.X, vector.Y - 1);
            Check(vector.X, vector.Y + 1);

            void Check(int x, int y) {
                if (x < 0 || x >= width || y < 0 || y >= height || tiles[x, y] != texture || pulseIndices[x, y] >= 0)
                    return;

                pulseIndices[x, y] = pulseIndex;
                toCheck.Enqueue(new IntVector(x, y));
            }
        }

        DynamicData.For(tileGrid).Set("tilePulseIndices", pulseIndices);
    }

    private static int[,] GetPulseIndices(TileGrid tileGrid)
        => DynamicData.For(tileGrid).Get<int[,]>("tilePulseIndices");

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