using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/waterfallBg"), TrackedAs(typeof(WaterFall))]
public class WaterfallBG : WaterFall {
    private Sprite[] sprites;

    public WaterfallBG(Vector2 position) : base(position) { }

    public WaterfallBG(EntityData data, Vector2 offset) : base(data, offset) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        Components.RemoveAll<DisplacementRenderHook>();
        Depth = 9000;
        sprites = new Sprite[(int) height / 8];

        for (int i = 0; i < sprites.Length; i++) {
            var sprite = new Sprite(GFX.Game, "objects/progHelper/waterfallBg/idle");

            sprite.AddLoop("idle", "", 0.025f);
            sprite.Play("idle");
            sprite.CurrentAnimationFrame = 19 - 8 * i % 20;
            sprite.Position = new Vector2(0f, 8f * i);
            Add(sprite);
            sprites[i] = sprite;
        }
    }

    public override void Render() {
        foreach (var sprite in sprites)
            sprite.Render();
    }
}