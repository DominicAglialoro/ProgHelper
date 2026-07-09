using Monocle;

namespace Celeste.Mod.ProgHelper;

public class CameraConstraints : Component {
    public struct Parameters {
        public bool HasMinX;
        public bool HasMaxX;
        public bool HasMinY;
        public bool HasMaxY;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }

    public Parameters Constraints;

    public CameraConstraints() : base(false, false) { }
}