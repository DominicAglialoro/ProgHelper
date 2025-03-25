using Monocle;

namespace Celeste.Mod.ProgHelper;

public static class SolidTilesExtensions {
    public static void Load() => On.Celeste.SolidTiles.Added += SolidTiles_Added;

    public static void Unload() => On.Celeste.SolidTiles.Added -= SolidTiles_Added;

    private static void SolidTiles_Added(On.Celeste.SolidTiles.orig_Added added, SolidTiles solidTiles, Scene scene) {
        added(solidTiles, scene);
        solidTiles.Tiles.GeneratePulseIndices(solidTiles.tileTypes, false);
    }
}