using Monocle;

namespace Celeste.Mod.ProgHelper;

public static class GameplayBuffersExtensions {
    public static VirtualRenderTarget InvertMask;

    public static void Load() {
        On.Celeste.GameplayBuffers.Create += GameplayBuffers_Create;
        On.Celeste.GameplayBuffers.Unload += GameplayBuffers_Unload;
    }

    public static void Unload() {
        On.Celeste.GameplayBuffers.Create -= GameplayBuffers_Create;
        On.Celeste.GameplayBuffers.Unload -= GameplayBuffers_Unload;
    }

    private static void GameplayBuffers_Create(On.Celeste.GameplayBuffers.orig_Create create) {
        create();

        InvertMask = VirtualContent.CreateRenderTarget("ProgHelper/InvertMask", 320, 180);
    }

    private static void GameplayBuffers_Unload(On.Celeste.GameplayBuffers.orig_Unload unload) {
        unload();

        InvertMask?.Dispose();
        InvertMask = null;
    }
}