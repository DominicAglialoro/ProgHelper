using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/adjustableBumper")]
public class AdjustableBumper : Entity {
    private float respawnTime;
    private bool cardinal;
    private bool snapUp;
    private bool snapDown;
    private BumperBoostType bumperBoost;
    private DashRestores dashRestores;
    private BumperFireModeType fireModeType;
    private Vector2 anchor;
    private Sprite sprite;
    private Sprite spriteEvil;
    private SineWave sine;
    private VertexLight light;
    private BloomPoint bloom;
    private Wiggler hitWiggler;
    private float respawnTimer;
    private bool fireMode;
    private bool goBack;
    private Vector2 hitDir;
    private bool ignoreHoldableWhenHot;

    public AdjustableBumper(EntityData data, Vector2 offset) : base(data.Position + offset) {
        respawnTime = data.Float("respawnTime");
        cardinal = data.Bool("cardinal");
        snapUp = data.Bool("snapUp");
        snapDown = data.Bool("snapDown");
        bumperBoost = data.Enum<BumperBoostType>("bumperBoost");
        dashRestores = data.Enum<DashRestores>("dashRestores");
        fireModeType = data.Enum<BumperFireModeType>("fireMode");

        if(data.Bool("boostHoldables"))
            Add(new HoldableCollider(OnHoldable));

        ignoreHoldableWhenHot = data.Bool("ignoreHoldableWhenHot");

        anchor = Position;

        var node = data.FirstNodeNullable();

        if (node.HasValue) {
            var start = Position;
            var end = node.Value;
            var tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeInOut, data.Float("moveTime"), true);

            tween.OnUpdate = t => {
                if (goBack)
                    anchor = Vector2.Lerp(end, start, t.Eased);
                else
                    anchor = Vector2.Lerp(start, end, t.Eased);
            };
            tween.OnComplete = _ => goBack = !goBack;
            Add(tween);
        }

        Collider = new Circle(12f);
        Add(new PlayerCollider(OnPlayer));

        string spritePath = data.Attr("sprite");
        string evilPath = $"{spritePath}_evil";

        Add(sprite = GFX.SpriteBank.Create(spritePath));
        Add(spriteEvil = GFX.SpriteBank.Create(GFX.SpriteBank.Has(evilPath) ? evilPath : "bumper_evil"));

        if (data.Bool("wobble"))
            Add(sine = new SineWave(0.44f, 0f).Randomize());

        Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.5f, 16f));
        Add(hitWiggler = Wiggler.Create(1.2f, 2f, v => spriteEvil.Position = 8f * v * hitDir));
        Add(new CoreModeListener(OnCoreModeChange));
    }

    public override void Added(Scene scene) {
        base.Added(scene);

        fireMode = fireModeType switch {
            BumperFireModeType.CoreMode => SceneAs<Level>().CoreMode == Session.CoreModes.Hot,
            BumperFireModeType.Always => true,
            _ => false
        };
        sprite.Visible = !fireMode;
        spriteEvil.Visible = fireMode;
    }

    public override void Update() {
        base.Update();

        if (respawnTimer > 0f) {
            respawnTimer -= Engine.DeltaTime;

            if (respawnTimer <= 0f) {
                light.Visible = true;
                bloom.Visible = true;
                sprite.Play("on");
                spriteEvil.Play("on");

                if (!fireMode)
                    Audio.Play(SFX.game_06_pinballbumper_reset, Position);
            }
        }
        else if (Scene.OnInterval(0.05f)) {
            float angle = Calc.Random.NextAngle();

            SceneAs<Level>().Particles.Emit(
                fireMode ? Bumper.P_FireAmbience : Bumper.P_Ambience, 1,
                Center + Calc.AngleToVector(angle, fireMode ? 12f : 8f),
                2f * Vector2.One,
                fireMode ? -MathHelper.PiOver2 : angle);
        }

        Position = sine != null ? anchor + new Vector2(3f * sine.Value, 2f * sine.ValueOverTwo) : anchor;
    }

    private void OnPlayer(Player player) {
        if (fireMode) {
            if (SaveData.Instance.Assists.Invincible)
                return;

            var killDirection = (player.Center - Center).SafeNormalize();

            respawnTimer = respawnTime;
            hitDir = -killDirection;
            hitWiggler.Start();
            player.Die(killDirection);
            SceneAs<Level>().Particles.Emit(Bumper.P_FireHit, 12, Center + 12f * killDirection, 3f * Vector2.One, killDirection.Angle());
            Audio.Play(SFX.game_09_hotpinball_activate, Position);

            return;
        }

        if (respawnTimer > 0f)
            return;

        respawnTimer = respawnTime;

        var direction = Launch(player);

        sprite.Play("hit", true);
        spriteEvil.Play("hit", true);
        light.Visible = false;
        bloom.Visible = false;

        var level = SceneAs<Level>();

        level.DirectionalShake(direction, 0.15f);
        level.Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
        level.Particles.Emit(Bumper.P_Launch, 12, Center + 1f * direction, 3f * Vector2.One, direction.Angle());
        Audio.Play(SFX.game_06_pinballbumper_hit, Position);

        if (fireModeType != BumperFireModeType.AfterHit)
            return;

        fireMode = true;
        sprite.Visible = false;
        spriteEvil.Visible = true;
    }

    private void OnHoldable(Holdable hold)
    {
        if (respawnTimer > 0f || fireMode && ignoreHoldableWhenHot || hold.IsHeld)
            return;

        respawnTimer = respawnTime;

        var direction = HoldableLaunch(hold);

        sprite.Play("hit", true);
        spriteEvil.Play("hit", true);
        light.Visible = false;
        bloom.Visible = false;

        var level = SceneAs<Level>();

        level.DirectionalShake(direction, 0.15f);
        level.Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
        level.Particles.Emit(Bumper.P_Launch, 12, Center + 1f * direction, 3f * Vector2.One, direction.Angle());
        Audio.Play(SFX.game_06_pinballbumper_hit, Position);

        if (fireModeType != BumperFireModeType.AfterHit)
            return;

        fireMode = true;
        sprite.Visible = false;
        spriteEvil.Visible = true;
    }

    private void OnCoreModeChange(Session.CoreModes coreMode) {
        if (fireModeType != BumperFireModeType.CoreMode)
            return;

        fireMode = coreMode == Session.CoreModes.Hot;
        sprite.Visible = !fireMode;
        spriteEvil.Visible = fireMode;
    }

    private Vector2 Launch(Player player) {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Celeste.Freeze(0.1f);
        player.launchApproachX = new float?();

        var direction = (player.Center - Center).SafeNormalize(-Vector2.UnitY);
        float dot = direction.Y;

        if (cardinal)
            direction = direction.FourWayNormal();
        else if (snapUp && dot <= -0.8f)
            direction = -Vector2.UnitY;
        else if (snapDown && dot >= 0.8f)
            direction = Vector2.UnitY;
        else if (dot >= -0.55 || dot <= 0.65)
            direction = Math.Sign(direction.X) * Vector2.UnitX;

        var speed = 280f * direction;

        if (speed.Y <= 50f) {
            speed.Y = Math.Min(speed.Y, -150f);
            player.AutoJump = true;
        }

        if (speed.X != 0f && bumperBoost != BumperBoostType.Disable) {
            if (bumperBoost == BumperBoostType.Force || Input.MoveX.Value == Math.Sign(speed.X)) {
                speed.X *= 1.2f;
                player.explodeLaunchBoostTimer = 0f;
            }
            else {
                player.explodeLaunchBoostTimer = 0.01f;
                player.explodeLaunchBoostSpeed = 1.2f * speed.X;
            }
        }

        player.Speed = speed;

        if (dashRestores == DashRestores.Default)
            player.RefillDash();
        else if (dashRestores == DashRestores.Two)
            player.UseRefill(true);

        player.RefillStamina();

        player.dashCooldownTimer = 0.2f;
        player.StateMachine.State = 7;
        SlashFx.Burst(player.Center, player.Speed.Angle());

        return direction;
    }

    private Vector2 HoldableLaunch(Holdable hold)
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Celeste.Freeze(0.1f);

        var direction = (hold.Entity.Center - Center).SafeNormalize(-Vector2.UnitY);
        float dot = direction.Y;

        if (cardinal)
            direction = direction.FourWayNormal();
        else if (snapUp && dot <= -0.8f)
            direction = -Vector2.UnitY;
        else if (snapDown && dot >= 0.8f)
            direction = Vector2.UnitY;
        else if (dot >= -0.55 || dot <= 0.65)
            direction = Math.Sign(direction.X) * Vector2.UnitX;

        var speed = 280f * direction;

        if (speed.Y <= 50f)
            speed.Y = Math.Min(speed.Y, -150f);

        hold.SetSpeed(speed);

        SlashFx.Burst(hold.Entity.Center, hold.GetSpeed().Angle());

        return direction;
    }
}