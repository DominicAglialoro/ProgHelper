using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/crumbleJumpThruOnJump"), Tracked]
public class CrumbleJumpThruOnJump : JumpthruPlatform {
    public bool Triggered;

    private readonly float delay;
    private readonly bool attached;
    private readonly bool permanent;
    private readonly bool createDebris;
    private readonly StaticMover staticMover;
    private readonly EntityID id;

    private Vector2 shake;

    public CrumbleJumpThruOnJump(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
        delay = data.Float("delay");
        attached = data.Bool("attached");
        permanent = data.Bool("permanent");
        createDebris = data.Bool("createDebris", true);
        this.id = id;

        if (delay >= 0f)
            Add(new Coroutine(Sequence()));

        if (!attached)
            return;

        staticMover = new StaticMover {
            SolidChecker = solid => CollideCheck(solid, Position - Vector2.UnitX) || CollideCheck(solid, Position + Vector2.UnitX),
            OnAttach = platform => Depth = platform.Depth + 1,
            OnMove = OnMove,
            OnShake = OnShake
        };
        Add(staticMover);
    }

    public override void Update() {
        base.Update();

        if (!attached)
            return;

        var player = GetPlayerRider();

        if (player != null && player.Speed.Y >= 0f)
            staticMover.Platform?.OnStaticMoverTrigger(staticMover);
    }

    public override void Render() {
        var position = Position;

        Position += shake;
        base.Render();
        Position = position;
    }

    public override void OnStaticMoverTrigger(StaticMover sm) => Triggered = true;

    public override void OnShake(Vector2 amount) {
        shake = amount;
        ShakeStaticMovers(amount);
    }

    public override void MoveHExact(int move) {
        if (Collidable) {
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>()) {
                if (!actor.IsRiding(this))
                    continue;

                if (actor.TreatNaive)
                    actor.NaiveMove(Vector2.UnitX * move);
                else
                    actor.MoveHExact(move);

                actor.LiftSpeed = LiftSpeed;
            }
        }

        X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }

    public void Break() {
        if (!Collidable)
            return;

        Audio.Play(SFX.game_10_quake_rockbreak, Position);
        Collidable = false;

        if (createDebris) {
            for (int i = 0; i < Width; i += 8) {
                if (!Scene.CollideCheck<Solid>(new Rectangle((int) X + i, (int) Y, 8, 8)))
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(i + 4, 4), '9', true).BlastFrom(TopCenter));
            }
        }

        if (permanent)
            SceneAs<Level>().Session.DoNotLoad.Add(id);

        RemoveSelf();
    }

    private void OnMove(Vector2 amount) {
        MoveH(amount.X);
        MoveV(amount.Y);
    }

    private IEnumerator Sequence() {
        while (!Triggered && !HasPlayerRider())
            yield return null;

        for (float time = 0f; time < delay; time += Engine.DeltaTime)
            yield return null;

        Break();
    }
}