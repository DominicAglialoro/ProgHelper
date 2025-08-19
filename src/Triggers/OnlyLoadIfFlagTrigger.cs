using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ProgHelper;

[CustomEntity("progHelper/onlyLoadIfFlagTrigger")]
public class OnlyLoadIfFlagTrigger : Trigger {
    private readonly string flag;
    private readonly bool invert;
    private readonly HashSet<string> entityTypes;

    public OnlyLoadIfFlagTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        flag = data.Attr("flag");
        invert = data.Bool("invert");
        entityTypes = data.Set("entityTypes");
        Depth = -1;
    }

    public override void Update() {
        base.Update();

        Scene.OnEndOfFrame += () => {
            if (Scene == null || SceneAs<Level>().Session.GetFlag(flag) != invert) {
                RemoveSelf();

                return;
            }

            foreach (var entity in Scene.Entities) {
                if (entity is not Trigger or SolidTiles or BackgroundTiles
                    && this.ContainsEntity(entity)
                    && (entityTypes == null || entityTypes.Contains(entity.GetType().FullName)))
                    entity.RemoveSelf();
            }

            RemoveSelf();
        };
    }
}