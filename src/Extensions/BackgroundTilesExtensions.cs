using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

public static class BackgroundTilesExtensions {
    public static void Load() => On.Celeste.BackgroundTiles.ctor += BackgroundTiles_ctor;

    public static void Unload() => On.Celeste.BackgroundTiles.ctor -= BackgroundTiles_ctor;

    private static void BackgroundTiles_ctor(On.Celeste.BackgroundTiles.orig_ctor ctor, BackgroundTiles backgroundTiles, Vector2 position, VirtualMap<char> data) {
        ctor(backgroundTiles, position, data);
        backgroundTiles.Add(new TilePulseData(data, true));
    }
}