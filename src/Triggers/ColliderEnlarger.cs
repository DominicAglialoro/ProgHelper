using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper; 

[CustomEntity("progHelper/colliderEnlarger")]
public class ColliderEnlarger : Entity {
    private float hitboxLeft;
    private float hitboxRight;
    private float hitboxTop;
    private float hitboxBottom;
    private float circleRadius;
    private float width;
    private float height;
    
    public ColliderEnlarger(EntityData data, Vector2 offset) : base(data.Position + offset) {
        hitboxLeft = data.Float("hitboxLeft");
        hitboxRight = data.Float("hitboxRight");
        hitboxTop = data.Float("hitboxTop");
        hitboxBottom = data.Float("hitboxBottom");
        circleRadius = data.Float("circleRadius");
        width = data.Width;
        height = data.Height;

        Visible = false;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        foreach (var entity in scene.Entities) {
            var relativePosition = entity.Position - Position;
            
            if (relativePosition.X >= 0f && relativePosition.X <= width && relativePosition.Y >= 0f && relativePosition.Y <= height)
                Enlarge(entity.Collider);
        }
        
        RemoveSelf();
    }

    private void Enlarge(Collider collider) {
        switch (collider) {
            case Hitbox hitbox: {
                hitbox.Position -= new Vector2(hitboxLeft, hitboxTop);
                hitbox.Width += hitboxLeft + hitboxRight;
                hitbox.Height += hitboxTop + hitboxBottom;
                
                break;
            }
            case Circle circle: {
                circle.Radius += circleRadius;
                
                break;
            }
            case ColliderList colliderList: {
                foreach (var subCollider in colliderList.colliders)
                    Enlarge(subCollider);
                
                break;
            }
        }
    }
}