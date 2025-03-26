using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/linearIntroCrusher")]
public class LinearIntroCrusher : Solid {
    private readonly Vector2 start;
    private readonly Vector2 end;
    private readonly TileGrid tilegrid;
    private readonly SoundSource shakingSfx;
    private readonly float delay;
    private readonly float speed;
    private readonly float easingPeriod;
    private readonly string[] flags;

    private Vector2 shake;

    public LinearIntroCrusher(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true) {
        start = data.Position + offset;
        end = data.Nodes[0] + offset;
        Depth = -10501;
        SurfaceSoundIndex = 4;
        delay = data.Float("delay", 1.2f);
        speed = data.Float("speed", 2f);
        easingPeriod = data.Float("easingPeriod");

        string flagsAttr = data.Attr("flags");

        flags = string.IsNullOrWhiteSpace(flagsAttr) ? new string[0] : flagsAttr.Split(',');

        Add(tilegrid = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
        Add(shakingSfx = new SoundSource());
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        if (GetAllFlags())
            Position = end;
        else
            Add(new Coroutine(Sequence()));
    }

    public override void Update() {
        tilegrid.Position = shake;
        base.Update();
    }

    private bool GetAllFlags() {
        foreach (string flag in flags) {
            if (!((Level) Scene).Session.GetFlag(flag))
                return false;
        }

        return true;
    }

    private IEnumerator Sequence() {
        Player player;

        do {
            yield return null;

            player = Scene.Tracker.GetEntity<Player>();
        } while (flags.Length > 0 ? !GetAllFlags() : player == null || player.X < X + 30f || player.X > Right + 8f);

        shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");

        float time = delay;
        var shaker = new Shaker(delay, true, v => shake = v);

        if (delay > 0f)
            Add(shaker);

        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        while (time > 0f) {
            player = Scene.Tracker.GetEntity<Player>();

            if (flags.Length == 0 && player != null && (player.X >= X + Width - 8f || player.X < X + 28f)) {
                shaker.RemoveSelf();

                break;
            }

            yield return null;

            time -= Engine.DeltaTime;
        }

        for (int i = 2; i < Width; i += 4) {
            SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
            SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
        }

        shakingSfx.Param("release", 1f);
        time = 0f;

        float cubeCoeff = easingPeriod > 0f ? 1f / (easingPeriod * easingPeriod * (3f - 2f * easingPeriod)) : 0f;
        float linearCoeff = 3f / (3f - 2f * easingPeriod);
        float linearOffset = 2f * easingPeriod / (2f * easingPeriod - 3f);

        do {
            yield return null;

            time = Calc.Approach(time, 1f, speed * Engine.DeltaTime);
            MoveTo(Vector2.Lerp(start, end, MathHelper.Clamp(time < easingPeriod ? cubeCoeff * time * time * time : linearCoeff * time + linearOffset, 0f, 1f)));
        } while (time < 1f);

        for (int j = 0; j <= Width; j += 4) {
            SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + j, Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
            SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + j, Bottom), Vector2.One * 4f, j >= Width / 2f ? 0f : (float) Math.PI);
        }

        shakingSfx.Stop();
        Audio.Play("event:/game/00_prologue/fallblock_first_impact", Position);
        SceneAs<Level>().Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Add(new Shaker(0.25f, true, v => shake = v));
    }
}