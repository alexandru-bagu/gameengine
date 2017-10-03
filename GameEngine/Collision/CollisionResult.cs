using OpenTK;

namespace GameEngine.Collision
{
    // Structure that stores the results of the PolygonCollision function
    public struct CollisionResult
    {
        public bool WillIntersect; // Are the polygons going to intersect forward in time?
        public bool Intersect; // Are the polygons currently intersecting
        public Vector2 MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
        public Vector2 TranslationAxis; //The collision plane
    }
}
