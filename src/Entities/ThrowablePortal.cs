using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using VivHelper.Entities;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/throwablePortal"), Tracked]
public class ThrowablePortal : Actor {
    private const float THROW_SPEED = 160f;
    private const float FRICTION = 290.08453f;

    private static readonly ParticleType PLAYER_VANISH_RED = new(BadelineOldsite.P_Vanish) {
        Color = Player.NormalHairColor,
        Color2 = Calc.HexToColor("e36b6b")
    };
    private static readonly ParticleType PLAYER_VANISH_BLUE = new(BadelineOldsite.P_Vanish) {
        Color = Player.UsedHairColor,
        Color2 = Calc.HexToColor("88d0fc")
    };
    private static readonly ParticleType FIZZLE_RED = new() {
        Color = Color.OrangeRed,
        Color2 = Color.White,
        ColorMode = ParticleType.ColorModes.Fade,
        FadeMode = ParticleType.FadeModes.Linear,
        Size = 1f,
        LifeMin = 0.3f,
        LifeMax = 0.5f,
        DirectionRange = MathHelper.PiOver4,
        SpeedMin = 60f,
        SpeedMax = 80f,
        SpeedMultiplier = 0.1f
    };
    private static readonly ParticleType FIZZLE_BLUE = new(FIZZLE_RED) { Color = Color.Cyan };

    public Vector2 Speed;
    public Holdable Hold;

    private bool warpReady = true;
    private bool enteredReceptacle;
    private float timeReady = 1f;
    private float time;
    private Vector2 previousLiftSpeed;
    private Level level;
    private readonly Image body;
    private readonly Particle[] particles;
    private bool waitForNewPress = true;
    private bool hasSpawned;

    public ThrowablePortal(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = 100;
        Collider = new Hitbox(8f, 11f, -4f, -11f);

        Add(Hold = new Holdable(0.1f));

        Hold.PickupCollider = new Hitbox(24f, 24f, -12f, -17f);
        Hold.OnPickup = OnPickup;
        Hold.OnRelease = OnRelease;
        Hold.OnHitSpring = HitSpring;
        Hold.SpeedGetter = () => Speed;

        body = new Image(GFX.Game["objects/progHelper/throwablePortal/body"]);
        body.Color = Color.OrangeRed;

        var edge = new Image(GFX.Game["objects/progHelper/throwablePortal/edge"]);

        body.JustifyOrigin(0.5f, 1f);
        edge.JustifyOrigin(0.5f, 1f);

        Add(body, edge);
        Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
        Add(new MirrorReflection());

        particles = new Particle[3];

        for (int i = 0; i < particles.Length; i++) {
            float phase = (float) i / particles.Length;
            var up = Calc.AngleToVector(MathHelper.Pi * phase, 12f) * (i % 2 * 2 - 1);
            var right = 0.125f * new Vector2(up.Y, -up.X);

            particles[i] = new Particle(phase, up, right);
        }

        LiftSpeedGraceTime = 0.1f;
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        level = (Level) scene;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        if (Hold.IsHeld)
            return;

        foreach (ThrowablePortal portal in scene.Tracker.GetEntities<ThrowablePortal>()) {
            if (portal == this || !portal.Hold.IsHeld)
                continue;

            RemoveSelf();

            return;
        }

        var respawnPoint = level.Session.RespawnPoint ?? Vector2.Zero;
        float distance = Vector2.Distance(Position, respawnPoint);

        foreach (ThrowablePortal portal in scene.Tracker.GetEntities<ThrowablePortal>()) {
            if (portal == this || portal.hasSpawned || Vector2.Distance(portal.Position, respawnPoint) >= distance)
                continue;

            RemoveSelf();

            return;
        }
    }

    public override void Update() {
        base.Update();

        hasSpawned = true;
        time += Engine.DeltaTime;

        if (warpReady)
            timeReady += Engine.DeltaTime;

        if (enteredReceptacle)
            return;

        if (!Input.GrabCheck)
            waitForNewPress = false;

        if (!Hold.IsHeld || Hold.Holder.StateMachine.State != Player.StPickup) {
            foreach (var entity in Scene.Tracker.GetEntities<SeekerBarrier>()) {
                entity.Collidable = true;

                if (!CollideCheck(entity)) {
                    entity.Collidable = false;

                    continue;
                }

                entity.Collidable = false;

                Vector2 fizzleDirection;

                if (Hold.IsHeld) {
                    fizzleDirection = Hold.Holder.Speed;
                    Hold.Holder.Drop();
                }
                else
                    fizzleDirection = Speed;

                if (fizzleDirection == Vector2.Zero)
                    fizzleDirection = -Vector2.UnitY;

                Audio.Play(SFX.game_10_glider_emancipate, Position);
                level.ParticlesFG.Emit(warpReady ? FIZZLE_RED : FIZZLE_BLUE, 20, Center, 4f * Vector2.One, fizzleDirection.Angle());
                RemoveSelf();

                return;
            }
        }

        if (Hold.IsHeld) {
            previousLiftSpeed = Vector2.Zero;
            ResetLiftSpeed();

            return;
        }

        var receptacle = CollideFirst<ThrowablePortalReceptacle>();

        if (receptacle != null && receptacle.Trigger()) {
            RestoreWarp();
            enteredReceptacle = true;
            Collidable = false;
            Hold.RemoveSelf();

            var startPosition = Position;
            var endPosition = receptacle.Position + 5f * Vector2.UnitY;
            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.QuadOut, 0.1f, true);

            tween.OnUpdate = t => Position = Vector2.Lerp(startPosition, endPosition, t.Percent);
            Add(tween);

            return;
        }

        var player = Scene.Tracker.GetEntity<Player>();

        if (player != null && player.StateMachine.State == Player.StDash && player.DashDir == Vector2.Zero && Hold.Check(player)) {
            Hold.CheckAgainstColliders();

            return;
        }

        var frictionVector = FRICTION * Engine.DeltaTime * Speed.SafeNormalize();

        Speed.X = Calc.Approach(Speed.X, 0f, Math.Abs(frictionVector.X));
        Speed.Y = Calc.Approach(Speed.Y, 0f, Math.Abs(frictionVector.Y));

        if (LiftSpeed == Vector2.Zero && previousLiftSpeed != Vector2.Zero) {
            Speed = previousLiftSpeed;
            previousLiftSpeed = Vector2.Zero;
        }
        else {
            previousLiftSpeed = LiftSpeed;

            if (LiftSpeed.X < 0f && Speed.X < 0f || LiftSpeed.X > 0f && Speed.X > 0f)
                Speed.X = 0f;

            if (LiftSpeed.Y < 0f && Speed.Y < 0f || LiftSpeed.Y > 0f && Speed.Y > 0f)
                Speed.Y = 0f;
        }

        MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);

        if (Left < level.Bounds.Left) {
            Left = level.Bounds.Left;
            Audio.Play(SFX.game_10_glider_wallbounce_left, Position);
            Speed.X *= -0.5f;
        }
        else if (Right > level.Bounds.Right) {
            Right = level.Bounds.Right;
            Audio.Play(SFX.game_10_glider_wallbounce_right, Position);
            Speed.X *= -0.5f;
        }

        if (Top < level.Bounds.Top) {
            Top = level.Bounds.Top;
            Speed.Y *= -0.5f;
        }
        else if (Top > level.Bounds.Bottom) {
            RemoveSelf();

            return;
        }

        Hold.CheckAgainstColliders();
    }

    public override void Render() {
        body.Color = warpReady ? Color.Lerp(Color.White, Color.OrangeRed, Math.Min(8.34f * timeReady, 1f)) : Color.Cyan;

        if (!warpReady) {
            base.Render();

            return;
        }

        foreach (var particle in particles)
            particle.Render(Center, time, false, timeReady);

        base.Render();

        foreach (var particle in particles)
            particle.Render(Center, time, true, timeReady);
    }

    public bool TryUseWarp(Player player) {
        if (!warpReady || enteredReceptacle || waitForNewPress)
            return false;

        var restorePosition = Position;
        var playerPreviousPosition = player.Position;
        var playerPreviousCenter = player.Center;
        bool playerWasDucking = player.Ducking;

        SpikeNudge();
        player.Position = Position;
        player.Ducking = false;

        if (player.CollideCheck<Solid>() || !player.Pickup(Hold)) {
            player.Position = playerPreviousPosition;
            player.Ducking = playerWasDucking;
            Position = restorePosition;

            return false;
        }

        ZeroRemainderX();
        ZeroRemainderY();
        player.ZeroRemainderX();
        player.ZeroRemainderY();
        Audio.Play(SFX.char_bad_disappear);
        warpReady = false;
        level.Particles.Emit(player.Dashes > 0 ? PLAYER_VANISH_RED : PLAYER_VANISH_BLUE, 12, playerPreviousCenter, 6f * Vector2.One);
        level.Displacement.AddBurst(playerPreviousCenter, 0.4f, 8f, 64f, 1f, Ease.QuadOut, Ease.QuadOut);
        level.Displacement.AddBurst(player.Center, 0.4f, 8f, 64f, 1f, Ease.QuadOut, Ease.QuadOut);

        return true;
    }

    public bool RestoreWarp() {
        if (warpReady)
            return false;

        warpReady = true;
        timeReady = 0f;

        return true;
    }

    public override void OnSquish(CollisionData data) {
        if (!TrySquishWiggle(data))
            RemoveSelf();
    }

    private void OnCollideH(CollisionData data) {
        if (data.Hit is DashSwitch dashSwitch)
            dashSwitch.OnDashCollide(null, Math.Sign(Speed.X) * Vector2.UnitX);

        if (Speed.X < 0f)
            Audio.Play(SFX.game_10_glider_wallbounce_left, Position);
        else
            Audio.Play(SFX.game_10_glider_wallbounce_right, Position);

        Speed.X *= -0.5f;
    }

    private void OnCollideV(CollisionData data) {
        if (data.Hit is DashSwitch dashSwitch)
            dashSwitch.OnDashCollide(null, Math.Sign(Speed.Y) * Vector2.UnitY);

        Speed.Y *= -0.5f;
    }

    private bool HitSpring(Spring spring) {
        if (Hold.IsHeld)
            return false;

        switch (spring.Orientation) {
            case Spring.Orientations.Floor when Speed.Y >= 0: {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                RestoreWarp();

                return true;
            }

            case Spring.Orientations.WallLeft when Speed.X <= 0f: {
                Speed.X = 160f;
                Speed.Y = -80f;
                RestoreWarp();

                return true;
            }

            case Spring.Orientations.WallRight when Speed.X >= 0f: {
                Speed.X = -160f;
                Speed.Y = -80f;
                RestoreWarp();

                return true;
            }
        }

        return false;
    }

    private void OnPickup() {
        AddTag(Tags.Persistent);
        Speed = Vector2.Zero;
    }

    private void OnRelease(Vector2 force) {
        RemoveTag(Tags.Persistent);

        if (force.X != 0f && force.Y == 0f)
            force.Y = -0.5f;

        Speed = THROW_SPEED * force;
        ResetLiftSpeed();
        waitForNewPress = true;
    }

    private void SpikeNudge() {
        if (!SpikeCheck(Position, out var spike, out var spikeDirection))
            return;

        Vector2 checkOffset;

        switch (spikeDirection) {
            case Spikes.Directions.Up:
                if (Bottom > spike.Bottom)
                    return;

                checkOffset = (-3f + spike.Bottom - Bottom) * Vector2.UnitY;

                break;
            case Spikes.Directions.Down:
                if (Top < spike.Top)
                    return;

                checkOffset = (3f + spike.Top - Top) * Vector2.UnitY;

                break;
            case Spikes.Directions.Left:
                if (Right > spike.Right)
                    return;

                checkOffset = (-3f + spike.Right - Right) * Vector2.UnitX;

                break;
            case Spikes.Directions.Right:
                if (Left < spike.Left)
                    return;

                checkOffset = (3f + spike.Left - Left) * Vector2.UnitX;

                break;
            default:
                return;
        }

        if (SpikeCheck(Position + checkOffset, out _, out _))
            return;

        MoveHExact((int) checkOffset.X);
        MoveVExact((int) checkOffset.Y);
    }

    private bool SpikeCheck(Vector2 position, out Entity entity, out Spikes.Directions spikeDirection) {
        var spike = CollideFirst<Spikes>(position);

        entity = spike;

        if (spike != null) {
            spikeDirection = spike.Direction;

            return true;
        }

        if (ProgHelperModule.Instance.VivHelperLoaded)
            return VivSpikeCheck(position, out entity, out spikeDirection);

        spikeDirection = default;

        return false;
    }

    private bool VivSpikeCheck(Vector2 position, out Entity entity, out Spikes.Directions spikeDirection) {
        var spike = CollideFirst<CustomSpike>(position);

        entity = spike;

        if (spike == null) {
            spikeDirection = default;

            return false;
        }

        spikeDirection = spike.Direction switch {
            DirectionPlus.Up => Spikes.Directions.Up,
            DirectionPlus.Down => Spikes.Directions.Down,
            DirectionPlus.Left => Spikes.Directions.Left,
            DirectionPlus.Right => Spikes.Directions.Right,
            _ => default
        };

        return true;
    }

    private struct Particle {
        private const float OUTLINE_LENGTH = 0.5f;
        private const int SAMPLES = 32;

        private readonly float phase;
        private readonly Vector2 up;
        private readonly Vector2 right;

        public Particle(float phase, Vector2 up, Vector2 right) {
            this.phase = phase;
            this.up = up;
            this.right = right;
        }

        public void Render(Vector2 center, float time, bool top, float timeReady) {
            var previousPos = Vector2.Zero;
            var color = Color.Lerp(Color.White, Color.LimeGreen, Math.Min(8.34f * timeReady, 1f));

            for (int i = 0; i < SAMPLES; i++) {
                float normalizedLifetime = (float) i / SAMPLES;
                float lifetime = OUTLINE_LENGTH * normalizedLifetime;

                if (lifetime > timeReady)
                    continue;

                float localPhase = time - lifetime + phase;

                localPhase -= (int) localPhase;

                if ((localPhase >= 0.5f) == top)
                    continue;

                var vect = Calc.AngleToVector(MathHelper.TwoPi * localPhase, 1f);
                var pos = vect.X * up + vect.Y * right;

                pos = new Vector2((int) pos.X, (int) pos.Y);

                if (i > 0 && pos == previousPos)
                    continue;

                float alpha = 1f - normalizedLifetime;

                alpha *= alpha;
                Draw.Rect(center + pos, 1f, 1f, color * alpha);
                previousPos = pos;
            }
        }
    }
}