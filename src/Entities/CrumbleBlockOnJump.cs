using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/crumbleBlockOnJump")]
public class CrumbleBlockOnJump : Solid {
    public bool Triggered;

    private float delay;
    private char tileType;
    private bool triggerOnLean;
    private bool permanent;
    private bool blendIn;
    private bool destroyStaticMovers;
    private EntityID id;

    public CrumbleBlockOnJump(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, false) {
        delay = data.Float("delay");
        tileType = data.Char("tiletype", '3');
        triggerOnLean = data.Bool("triggerOnLean");
        permanent = data.Bool("permanent");
        blendIn = data.Bool("blendIn");
        destroyStaticMovers = data.Bool("destroyStaticMovers");

        this.id = id;
        Depth = blendIn ? -10501 : -12999;

        Add(new Coroutine(Sequence()));
        Add(new LightOcclude());
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        TileGrid tileGrid;

        if (blendIn) {
            var level = SceneAs<Level>();
            var bounds = level.Session.MapData.TileBounds;

            tileGrid = GFX.FGAutotiler.GenerateOverlay(
                tileType,
                (int) (X / 8f) - bounds.Left,
                (int) (Y / 8f) - bounds.Top,
                (int) Width / 8,
                (int) Height / 8, level.SolidsData).TileGrid;
        }
        else
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) Width / 8, (int) Height / 8).TileGrid;

        Add(tileGrid);
        Add(new TileInterceptor(tileGrid, true));
    }

    public override void OnStaticMoverTrigger(StaticMover sm) => Triggered = true;

    private IEnumerator Sequence() {
        while (!Triggered && !HasPlayerRider() && (!triggerOnLean || Input.MoveX == 0 || !CollideCheck<Player>(Position - Input.MoveX * Vector2.UnitX)))
            yield return null;

        for (float time = delay; !Triggered && (time > 0f || delay < 0f); time -= Engine.DeltaTime)
            yield return null;

        Audio.Play(SFX.game_10_quake_rockbreak);
        Collidable = false;

        for (int i = 0; i < Width; i += 8) {
            for (int j = 0; j < Height; j += 8) {
                if (!Scene.CollideCheck<Solid>(new Rectangle((int) X + i, (int) Y + j, 8, 8)))
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(i + 4, j + 4), tileType, true).BlastFrom(TopCenter));
            }
        }

        if (permanent)
            SceneAs<Level>().Session.DoNotLoad.Add(id);

        if (destroyStaticMovers)
            DestroyStaticMovers();

        RemoveSelf();
    }
}