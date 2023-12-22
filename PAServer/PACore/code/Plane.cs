using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {

    // A plane defined such that (plane_normal * displacement + origin_point) 
    // is a point on the plane
    // TODO: Reduce position to distance from origin float
    public class Plane {

        public readonly Vector3 Normal;

        public readonly float Displacement;

        public Plane(Vector3 normalParam, Vector3 positionParam) {
            Normal = normalParam;
            Displacement = Vector3.Dot(positionParam, normalParam);
        }

        public Plane(Vector3 normalParam, float displacementParam) {
            Normal = normalParam;
            Displacement = displacementParam;
        }

        public Vector3 GetPlanePoint() {
            return Displacement * Normal;
        }

        public Plane Transform(Matrix4x4 transform) {
            var planePoint = Displacement * Normal;
            return new Plane(Vector3.TransformNormal(Normal, transform), Vector3.Transform(planePoint, transform));
        }

        public float PointSide(Vector3 queryPoint, float customEpsilon = -1.0f) {
            return PointSide(queryPoint, Normal, Displacement, customEpsilon);
        }

        public static float PointSide(Vector3 position, Vector3 planeNormal, float planeDisplacement, float customEpsilon = -1.0f) {
            const float BASE_EPSILON = 0.0001f;
            var epsilon = BASE_EPSILON;
            if (customEpsilon > 0) {
                epsilon = customEpsilon;
            }

            var sideNumber = Vector3.Dot(position, planeNormal) - planeDisplacement;

            if (Math.Abs(sideNumber) < epsilon) {
                return 0f;
            } else {
                return Math.Sign(sideNumber);
            }
        }


        public static Vector3? LineIntersection(Vector3 l0, Vector3 l1, Vector3 planePos, Vector3 planeNorm) {
            var EPSILON = 0.0001f;
            var lNorm = Vector3.Normalize(l1 - l0);
            var maxDist = (l1 - l0).Length();

            // line is inside the plane
            if (Math.Abs(Vector3.Dot(lNorm, planeNorm)) < EPSILON) {
                return null;
            }
            var distAlongLine = Vector3.Dot(planePos - l0, planeNorm) / Vector3.Dot(lNorm, planeNorm);
            if (distAlongLine < 0 || distAlongLine > maxDist) {
                return null;
            }
            var result = distAlongLine * lNorm + l0;
            return result;
        }


        public Vector3 LineIntersectionClamped(Vector3 l0, Vector3 l1) {
            const float EPSILON = 0.0001f;
            var lNorm = Vector3.Normalize(l1 - l0);
            // TODO: replace planepos using plane displacement
            var planePos = Normal * Displacement;
            var distAlongLine = Vector3.Dot(planePos - l0, Normal) / Vector3.Dot(lNorm, Normal);
            if (distAlongLine < EPSILON) {
                distAlongLine = EPSILON;
            }
            var maxDist = (l1 - l0).Length() - EPSILON;
            if (distAlongLine > maxDist) {
                distAlongLine = maxDist;
            }
            return distAlongLine * Vector3.Normalize(l1 - l0) + l0;
        }
    }
}