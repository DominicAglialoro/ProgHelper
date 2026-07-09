using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

public static class SolidTilesExtensions {
    public static void Load() => On.Celeste.SolidTiles.ctor += SolidTiles_ctor;

    public static void Unload() => On.Celeste.SolidTiles.ctor -= SolidTiles_ctor;

    private static void SolidTiles_ctor(On.Celeste.SolidTiles.orig_ctor ctor, SolidTiles solidTiles, Vector2 position, VirtualMap<char> data) {
        ctor(solidTiles, position, data);
        solidTiles.Add(new TilePulseData(data, false));
    }
}