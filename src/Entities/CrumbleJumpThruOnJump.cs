using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/crumbleJumpThruOnJump"), Tracked]
public class CrumbleJumpThruOnJump : JumpthruPlatform {
    public bool Triggered;

    private float delay;
    private bool permanent;
    private EntityID id;

    public CrumbleJumpThruOnJump(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
        delay = data.Float("delay");
        permanent = data.Bool("permanent");
        this.id = id;

        if (delay >= 0f)
            Add(new Coroutine(Sequence()));
    }

    public override void OnStaticMoverTrigger(StaticMover sm) => Triggered = true;

    public void Break() {
        if (!Collidable)
            return;

        Audio.Play(SFX.game_10_quake_rockbreak, Position);
        Collidable = false;

        for (int i = 0; i < Width; i += 8) {
            if (!Scene.CollideCheck<Solid>(new Rectangle((int) X + i, (int) Y, 8, 8)))
                Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(i + 4, 4), '9', true).BlastFrom(TopCenter));
        }

        if (permanent)
            SceneAs<Level>().Session.DoNotLoad.Add(id);

        RemoveSelf();
    }

    private IEnumerator Sequence() {
        while (!Triggered && !HasPlayerRider())
            yield return null;

        for (float time = 0f; time < delay; time += Engine.DeltaTime)
            yield return null;

        Break();
    }
}