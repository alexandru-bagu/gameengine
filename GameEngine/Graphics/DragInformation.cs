using OpenTK;

namespace GameEngine.Graphics
{
    public class DragInformation
    {
        public Vector2 Delta { get; private set; }
        public Vector2 RelativeMouseStart { get; private set; }
        public Vector2 AbsoluteMouseStart { get; private set; }
        public Vector2 ReferencePoint { get; private set; }

        public DragInformation(Vector2 absoluteMousePosition, Vector2 relativeMousePosition, Vector2 referencePoint)
        {
            RelativeMouseStart = relativeMousePosition;
            AbsoluteMouseStart = absoluteMousePosition;
            ReferencePoint = referencePoint;
            Delta = RelativeMouseStart - AbsoluteMouseStart;
        }

        public void UpdateAbsoluteStart(Vector2 absoluteMousePosition)
        {
            AbsoluteMouseStart = absoluteMousePosition;
            Delta = RelativeMouseStart - AbsoluteMouseStart;
        }

        public Vector2 Compute(Vector2 mouseNow)
        {
            return Delta + mouseNow;
        }
    }
}
