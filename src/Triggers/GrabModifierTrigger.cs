using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

[CustomEntity("progHelper/grabModifierTrigger")]
public class GrabModifierTrigger : Trigger {
    private bool disableGrab;
    private bool bufferableGrab;
    private bool coversScreen;
    
    public GrabModifierTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        disableGrab = data.Bool("disableGrab");
        bufferableGrab = data.Bool("bufferableGrab");
        coversScreen = data.Bool("coversScreen");
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        
        if (!coversScreen)
            return;

        var bounds = SceneAs<Level>().Bounds;
        
        Position = new Vector2(bounds.X, bounds.Y - 24f);
        Collider.Width = bounds.Width;
        Collider.Height = bounds.Height + 32f;
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        ProgHelperModule.Session.DisableGrab = disableGrab;
        ProgHelperModule.Session.BufferableGrab = bufferableGrab;
        Input.Grab.BufferTime = bufferableGrab ? 0.08f : 0f;
    }
}