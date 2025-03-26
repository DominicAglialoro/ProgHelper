using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/adjustableFallingBlock"), Tracked]
public class AdjustableFallingBlock : Solid {
    public bool Triggered;

    private readonly char tileType;
    private readonly bool climbFall;
    private readonly bool checkAttached;
    private readonly float delay;
    private readonly float playerWait;
    private readonly TileGrid tiles;
    private readonly List<Solid> solids = new();
    private readonly List<JumpThru> jumpThrus = new();

    public AdjustableFallingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false) {
        tileType = data.Char("tiletype", '3');
        climbFall = data.Bool("climbFall");
        checkAttached = data.Bool("checkAttached");
        delay = data.Float("delay");
        playerWait = data.Float("playerWait");

        int newSeed = Calc.Random.Next();
        int width = data.Width;
        int height = data.Height;

        Calc.PushRandom(newSeed);
        Add(tiles = GFX.FGAutotiler.GenerateBox(tileType, width / 8, height / 8).TileGrid);
        Calc.PopRandom();

        Add(new Coroutine(Sequence()));
        Add(new LightOcclude());
        Add(new TileInterceptor(tiles, false));

        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        Depth = data.Bool("behind") ? 5000 : 0;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        solids.Add(this);

        foreach (var staticMover in staticMovers) {
            var entity = staticMover.Entity;

            if (entity is Solid solid)
                solids.Add(solid);
            else if (entity is JumpThru jumpThru)
                jumpThrus.Add(jumpThru);
        }
    }

    public override void OnShake(Vector2 amount) {
        base.OnShake(amount);
        tiles.Position += amount;
    }

    public override void OnStaticMoverTrigger(StaticMover sm) {
        if (!checkAttached)
            Triggered = true;
    }

    private bool PlayerFallCheck() {
        if (!checkAttached)
            return climbFall ? HasPlayerRider() : HasPlayerOnTop();

        foreach (var solid in solids) {
            if (climbFall ? solid.HasPlayerRider() : solid.HasPlayerOnTop())
                return true;
        }

        foreach (var jumpThru in jumpThrus) {
            if (jumpThru.HasPlayerRider())
                return true;
        }

        return false;
    }

    private bool PlayerWaitCheck() {
        if (Triggered || PlayerFallCheck())
            return true;

        if (!climbFall)
            return false;

        if (!checkAttached)
            return CollideCheck<Player>(Position - Vector2.UnitX) || CollideCheck<Player>(Position + Vector2.UnitX);

        foreach (var solid in solids) {
            if (solid.CollideCheck<Player>(solid.Position - Vector2.UnitX) || solid.CollideCheck<Player>(solid.Position + Vector2.UnitX))
                return true;
        }

        return false;
    }

    private IEnumerator Sequence() {
        var level = SceneAs<Level>();

        while (!Triggered && !PlayerFallCheck())
            yield return null;

        while (true) {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            yield return delay;

            float timer = playerWait;

            while (timer > 0f && PlayerWaitCheck()) {
                yield return null;

                timer -= Engine.DeltaTime;
            }

            StopShaking();

            for (int i = 2; i < Width; i += 4) {
                if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    level.Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float) Math.PI / 2f);

                level.Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
            }

            float speed = 0f;

            while (true) {
                speed = Calc.Approach(speed, 160f, 500f * Engine.DeltaTime);

                if (MoveVCollideSolids(speed * Engine.DeltaTime, true))
                    break;

                if (Top > level.Bounds.Bottom + 16 || Top > level.Bounds.Bottom - 1 && CollideCheck<Solid>(Position + Vector2.UnitY)) {
                    Collidable = false;
                    Visible = false;

                    yield return 0.2f;

                    if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f))) {
                        yield return 0.2f;

                        level.Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }

                    RemoveSelf();
                    DestroyStaticMovers();

                    yield break;
                }

                yield return null;
            }

            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(Vector2.UnitY);
            StartShaking();
            LandParticles();

            yield return 0.2f;

            StopShaking();

            if (CollideCheck<SolidTiles>(Position + Vector2.UnitY))
                break;

            while (CollideCheck<Platform>(Position + Vector2.UnitY))
                yield return 0.1f;
        }

        Safe = true;
    }

    private void LandParticles() {
      for (int x = 2; x <= (double) Width; x += 4) {
          if (!Scene.CollideCheck<Solid>(BottomLeft + new Vector2(x, 3f)))
              continue;

          float direction = x >= Width / 2.0 ? 0.0f : 3.1415927f;

          SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + x, Bottom), Vector2.One * 4f, -1.5707964f);
          SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + x, Bottom), Vector2.One * 4f, direction);
      }
    }

    private void ShakeSfx() {
      if (tileType == '3')
        Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", Center);
      else if (tileType == '9')
        Audio.Play("event:/game/03_resort/fallblock_wood_shake", Center);
      else if (tileType == 'g')
        Audio.Play("event:/game/06_reflection/fallblock_boss_shake", Center);
      else
        Audio.Play("event:/game/general/fallblock_shake", Center);
    }

    private void ImpactSfx() {
      if (tileType == '3')
        Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", BottomCenter);
      else if (tileType == '9')
        Audio.Play("event:/game/03_resort/fallblock_wood_impact", BottomCenter);
      else if (tileType == 'g')
        Audio.Play("event:/game/06_reflection/fallblock_boss_impact", BottomCenter);
      else
        Audio.Play("event:/game/general/fallblock_impact", BottomCenter);
    }
}