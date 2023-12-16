using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

[CustomEntity("progHelper/qolModifierTrigger")]
public class QolModifierTrigger : Trigger {
    private bool liftboostProtection;
    private bool ultraProtection;
    private bool coversScreen;
    
    public QolModifierTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        liftboostProtection = data.Bool("liftboostProtection");
        ultraProtection = data.Bool("ultraProtection");
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
        ProgHelperModule.Session.LiftboostProtection = liftboostProtection;
        ProgHelperModule.Session.UltraProtection = ultraProtection;
    }
}