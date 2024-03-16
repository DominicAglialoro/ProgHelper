using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/tilePulseController"), Tracked]
public class TilePulseController : Entity {
    private float baseBrightness;
    private float pulseInterval;
    private float pulseLength;
    private float pulseSpeed;

    public TilePulseController(EntityData data, Vector2 offset) : base(data.Position + offset) {
        baseBrightness = data.Float("baseBrightness");
        pulseInterval = data.Float("pulseInterval");
        pulseLength = data.Float("pulseLength");
        pulseSpeed = data.Float("pulseSpeed");
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        var modSession = ProgHelperModule.Session;

        modSession.TilePulseBaseBrightness = baseBrightness;
        modSession.TilePulseInterval = pulseInterval;
        modSession.TilePulseLength = pulseLength;
        modSession.TilePulseStep = pulseSpeed > 0f ? 1f / pulseSpeed : 0f;
    }
}