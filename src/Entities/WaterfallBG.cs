using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/waterfallBg")]
public class WaterfallBG : Entity {
    private float height;
    private Sprite[] sprites;
    private SoundSource loopingSfx;
    private SoundSource enteringSfx;
    private Solid solid;
    private Water water;

    public WaterfallBG(Vector2 position) : base(position) { }

    public WaterfallBG(EntityData data, Vector2 offset) : base(data.Position + offset) { }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        var level = SceneAs<Level>();
        bool deep = false;

        for (height = 8f; Y + height < level.Bounds.Bottom
                          && (water = Scene.CollideFirst<Water>(new Rectangle((int) X, (int) (Y + height), 8, 8))) == null
                          && ((solid = Scene.CollideFirst<Solid>(new Rectangle((int) X, (int) (Y + height), 8, 8))) == null || !solid.BlockWaterfalls); solid = null)
            height += 8f;

        if (water != null && !Scene.CollideCheck<Solid>(new Rectangle((int) X, (int) (Y + height), 8, 16)))
            deep = true;

        Add(loopingSfx = new SoundSource());
        loopingSfx.Play("event:/env/local/waterfall_small_main");
        Add(enteringSfx = new SoundSource());
        enteringSfx.Play(deep ? "event:/env/local/waterfall_small_in_deep" : "event:/env/local/waterfall_small_in_shallow");
        enteringSfx.Position.Y = height;

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

    public override void Update() {
        var level = SceneAs<Level>();

        loopingSfx.Position.Y = Calc.Clamp(level.Camera.Position.Y + 90f, Y, height);

        if (water != null && Scene.OnInterval(0.3f))
            water.TopSurface.DoRipple(new Vector2(X + 4f, water.Y), 0.75f);

        if (water != null || solid != null) {
            level.ParticlesBG.Emit(Water.P_Splash, 1,
                new Vector2(X + 4f, Y + height + 2f),
                new Vector2(8f, 2f),
                new Vector2(0.0f, -1f).Angle());
        }

        base.Update();
    }
}