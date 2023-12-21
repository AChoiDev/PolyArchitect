using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {
    // A basic AABB implementation to speed up collision tests
    public class AxisAlignedBoundingBox {
        private readonly Vector3 minimum;
        private readonly Vector3 maximum;

        public AxisAlignedBoundingBox(HashSet<Vector3> posSet) {
            minimum = posSet.First();
            maximum = minimum;
            foreach (var pos in posSet) {
                minimum.X = Math.Min(pos.X, minimum.X);
                minimum.Y = Math.Min(pos.Y, minimum.Y);
                minimum.Z = Math.Min(pos.Z, minimum.Z);

                maximum.X = Math.Max(pos.X, maximum.X);
                maximum.Y = Math.Max(pos.Y, maximum.Y);
                maximum.Z = Math.Max(pos.Z, maximum.Z);
            }
        }

        public static bool DoesCollide(float a1, float a2, float b1, float b2) {
            return (a1 > b1 || a2 > b1) && (b1 > a1 || b2 > a1);
        }

        public static bool DoesCollide(AxisAlignedBoundingBox aabbOne, AxisAlignedBoundingBox aabbTwo) {
            return
                DoesCollide(aabbOne.minimum.X, aabbOne.maximum.X, aabbTwo.minimum.X, aabbTwo.maximum.X)
                && DoesCollide(aabbOne.minimum.Y, aabbOne.maximum.Y, aabbTwo.minimum.Y, aabbTwo.maximum.Y)
                && DoesCollide(aabbOne.minimum.Z, aabbOne.maximum.Z, aabbTwo.minimum.Z, aabbTwo.maximum.Z);
        }
    }
}