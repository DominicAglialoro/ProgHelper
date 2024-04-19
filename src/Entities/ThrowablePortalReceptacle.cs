using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/throwablePortalReceptacle"), Tracked]
public class ThrowablePortalReceptacle : Entity {
    private bool triggered;
    private Sprite sprite;
    private EntityID id;

    public ThrowablePortalReceptacle(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        this.id = id;

        sprite = new Sprite(GFX.Game, "objects/progHelper/throwablePortalReceptacle/");
        sprite.Add("idle", "idle");
        sprite.AddLoop("triggered", "triggered", 0.05f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(sprite);
        Add(new VertexLight(Color.White, 0.8f, 16, 32));

        Collider = new Hitbox(16f, 16f, -8f, -8f);

        Depth = 2000;
    }

    public bool Trigger() {
        if (triggered)
            return false;

        triggered = true;
        Audio.Play(SFX.game_05_gatebutton_activate, Position);
        sprite.Play("triggered");

        TempleGate nearest = null;
        float nearestDistance = 0f;

        foreach (TempleGate gate in Scene.Tracker.GetEntities<TempleGate>()) {
            if (gate.Type != TempleGate.Types.NearestSwitch || gate.ClaimedByASwitch || gate.LevelID != id.Level)
                continue;

            float distance = Vector2.DistanceSquared(Position, gate.Position);

            if (nearest != null && distance > nearestDistance)
                continue;

            nearest = gate;
            nearestDistance = distance;
        }

        if (nearest == null)
            return true;

        nearest.ClaimedByASwitch = true;
        nearest.SwitchOpen();

        return true;
    }
}