using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class LevelExtensions {
    private static readonly BlendState INVERT_BLEND = new() {
        ColorSourceBlend = Blend.Zero,
        ColorDestinationBlend = Blend.InverseSourceColor,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };

    private static readonly BlendState INVERT_MASKED_BLEND = new() {
        ColorSourceBlend = Blend.InverseDestinationColor,
        ColorDestinationBlend = Blend.InverseSourceColor,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };

    public static void Load() {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
        IL.Celeste.Level.Render += Level_Render_il;
    }

    public static void Unload() {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        IL.Celeste.Level.Render -= Level_Render_il;
    }

    private static void RenderInvertMask(Level level) {
        if (level.Tracker.GetEntity<NegativeSpaceController>()?.BackgroundInvertsColor is not true)
            return;

        Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
        Engine.Instance.GraphicsDevice.Clear(Color.White);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, INVERT_BLEND, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
        Draw.SpriteBatch.Draw(GameplayBuffersExtensions.InvertMask, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();

        Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, INVERT_MASKED_BLEND, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
        Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
    }

    private static void GenerateSolidTiles(this Level level) {
        var bgTiles = level.BgTiles;
        int width = bgTiles.Tiles.Tiles.Columns;
        int height = bgTiles.Tiles.Tiles.Rows;
        bool[,] data = new bool[width, height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
                data[i, j] = level.BgData[i, j] != '0';
        }

        var bgSolid = new Solid(bgTiles.Position, 1f, 1f, true);

        bgSolid.Tag = Tags.Global;
        bgSolid.Collider = new Grid(8f, 8f, data);
        level.Add(bgSolid);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
                data[i, j] = !data[i, j];
        }

        var fgSolid = new Solid(bgTiles.Position, 1f, 1f, true);

        fgSolid.Tag = Tags.Global;
        fgSolid.Collider = new Grid(8f, 8f, data);
        fgSolid.Collidable = false;
        level.Add(fgSolid);

        var dynamicData = DynamicData.For(level);

        dynamicData.Set("bgSolid", bgSolid);
        dynamicData.Set("fgSolid", fgSolid);
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel loadLevel, Level level, Player.IntroTypes playerintro, bool isfromloader) {
        var dynamicData = DynamicData.For(level);

        if (dynamicData.TryGet<Solid>("bgSolid", out _)) {
            loadLevel(level, playerintro, isfromloader);
            level.Tracker.GetEntity<NegativeSpaceController>()?.CheckForSwap(false);

            return;
        }

        var levelData = level.Session.LevelData;

        foreach (var entity in levelData.Entities) {
            if (entity.Name != "progHelper/negativeSpaceController")
                continue;

            level.GenerateSolidTiles();

            break;
        }

        loadLevel(level, playerintro, isfromloader);
        level.Tracker.GetEntity<NegativeSpaceController>()?.CheckForSwap(false);
    }

    private static void Level_Render_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall(typeof(Distort), nameof(Distort.Render)));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitCall(RenderInvertMask);
    }
}