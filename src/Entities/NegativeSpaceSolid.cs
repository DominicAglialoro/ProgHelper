using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

[Tracked]
public class NegativeSpaceSolid : Solid {
    public NegativeSpaceSolid(Vector2 position, float width, float height, bool safe) : base(position, width, height, safe) { }
}