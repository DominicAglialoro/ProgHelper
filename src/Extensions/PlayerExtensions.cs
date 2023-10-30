using Monocle;

namespace Celeste.Mod.ProgHelper; 

public static class PlayerExtensions {
    public static void Load() => On.Celeste.Player.Added += Player_Added;

    public static void Unload() => On.Celeste.Player.Added -= Player_Added;

    private static void Player_Added(On.Celeste.Player.orig_Added added, Player player, Scene scene) {
        added(player, scene);
        Input.Grab.BufferTime = ProgHelperModule.Session.BufferableGrab ? 0.08f : 0f;
    }
}