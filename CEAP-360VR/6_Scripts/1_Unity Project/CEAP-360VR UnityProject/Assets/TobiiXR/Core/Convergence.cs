using Vector3 = UnityEngine.Vector3;

namespace Tobii.XR
{
    public static class Convergence
    {
        private const float _minimumDistance_mm = 100;
        private const float _maximumDistance_mm = 10000;
        private static float _lastDistance_mm = _maximumDistance_mm;

        public static float CalculateDistance(Vector3 leftOriginLocal_mm, Vector3 leftDirection, Vector3 rightOriginLocal_mm, Vector3 rightDirection)
        {
            Vector3 p1;
            Vector3 p2;

            var linesAreNotParallel = ClosestPointsOnTwoLines(out p1, out p2, ref rightOriginLocal_mm, ref rightDirection, ref leftOriginLocal_mm, ref leftDirection);

            var combinedGazeOrigin_mm = (leftOriginLocal_mm + rightOriginLocal_mm) / 2f;
            var convergencePoint = (p1 + p2) / 2f;

            var distance_mm = Vector3.Distance(convergencePoint, combinedGazeOrigin_mm);

            if (float.IsNaN(distance_mm))
            {
                return _lastDistance_mm;
            }

            if (!linesAreNotParallel || distance_mm > _maximumDistance_mm || float.IsInfinity(distance_mm))
            {
                distance_mm = _maximumDistance_mm;
            }

            if (distance_mm < _minimumDistance_mm)
            {
                distance_mm = _minimumDistance_mm;
            }

            _lastDistance_mm = distance_mm;

            return distance_mm;
        }

        /// <summary>
        /// Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        /// to each other. This function finds those two points. If the lines are not parallel, the function
        /// outputs true, otherwise false.
        /// </summary>
        private static bool ClosestPointsOnTwoLines(
            out Vector3 closestPointLine1,
            out Vector3 closestPointLine2,
            ref Vector3 linePoint1,
            ref Vector3 lineVec1,
            ref Vector3 linePoint2,
            ref Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            var a = Vector3.Dot(lineVec1, lineVec1);
            var b = Vector3.Dot(lineVec1, lineVec2);
            var e = Vector3.Dot(lineVec2, lineVec2);

            var d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {
                var r = linePoint1 - linePoint2;
                var c = Vector3.Dot(lineVec1, r);
                var f = Vector3.Dot(lineVec2, r);

                var s = (b * f - c * e) / d;
                var t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}