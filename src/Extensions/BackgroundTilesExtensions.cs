using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper;

public static class BackgroundTilesExtensions {
    public static void Load() {
        On.Celeste.BackgroundTiles.ctor += BackgroundTiles_ctor;
        On.Celeste.BackgroundTiles.Added += BackgroundTiles_Added;
    }

    public static void Unload() {
        On.Celeste.BackgroundTiles.ctor -= BackgroundTiles_ctor;
        On.Celeste.BackgroundTiles.Added -= BackgroundTiles_Added;
    }

    private static void BackgroundTiles_ctor(On.Celeste.BackgroundTiles.orig_ctor ctor, BackgroundTiles backgroundTiles, Vector2 position, VirtualMap<char> data) {
        ctor(backgroundTiles, position, data);
        DynamicData.For(backgroundTiles).Set("tileTypes", data);
    }

    private static void BackgroundTiles_Added(On.Celeste.BackgroundTiles.orig_Added added, BackgroundTiles backgroundTiles, Scene scene) {
        added(backgroundTiles, scene);

        var tileTypes = DynamicData.For(backgroundTiles).Get<VirtualMap<char>>("tileTypes");

        backgroundTiles.Tiles.GeneratePulseIndices(tileTypes, true);
        // backgroundTiles.Tiles.GenerateInvertMask(tileTypes);
    }
}