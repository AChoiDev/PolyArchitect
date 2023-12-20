using System;
using System.Collections.Generic;
using System.Numerics;

namespace PolyArchitect.Core {
    // A set of helper methods for creating specific types of convex polyhedrons
    // Each returns a list of oriented planes
    // an oriented plane is a plane with a tangent direction included
    // TODO: make an oriented plane class and give it a proper name
    // rename this class to not mention brush
    public partial class BrushBuilders {

        // TODO: Move this elsewhere as it doesn't really fit here
        private static Vector3 TangentFromPlane(Plane plane) {
            var normal = plane.Normal;

            var a = Vector3.Cross(normal, Vector3.UnitX);
            var b = Vector3.Cross(normal, Vector3.UnitY);

            if (a.Length() > b.Length()) {
                return Vector3.Normalize(a);
            } else {
                return Vector3.Normalize(b);
            }
        }

        // TODO: make the return type a single list
        public static (List<Plane>, List<Vector3>) MakeCylinder(int sides, Vector3 upDir, Vector3 frontDir) {
            if (sides < 3) {
                throw new System.Exception("Sides should be 3 or more");
            }
            var size = Vector3.One;
            var sideDir = Vector3.Cross(upDir, frontDir);

            var planes = new List<Plane>() {
            new(upDir, upDir*size.Y),
            new(-upDir, -upDir*size.Y)
        };
        //    var tangentDirs = new List<Vector3>() {
        //    sideDir,
        //    -sideDir
        //};
            for (int i = 0; i < sides; i++) {
                var rad = (float)Math.PI * 2.0f * ((float)i / sides);
                // Math.Cos()
                var point = size.X * sideDir * (float)Math.Cos(rad) + size.Z * frontDir * (float)Math.Sin(rad);
                var normal = Vector3.Normalize(point - Vector3.Zero);
                planes.Add(new Plane(normal, point));
                //var tangent = Vector3.Cross(normal, upDir);
                //tangentDirs.Add(tangent);
            }

            var tangents = planes.ConvertAll(TangentFromPlane);

            return (planes, tangents);
        }

        public static (List<Plane>, List<Vector3>) MakeCone(int sides, Vector3 upDir, Vector3 frontDir) {
            if (sides < 3) {
                throw new System.Exception("Sides should be 3 or more");
            }

            // NOTE: This is a bit improper if the cone is a different size but that can be a problem when the time comes
            var verticalOffset = new Vector3(0.0f, -0.5f, 0.0f);
            var size = Vector3.One;
            var sideDir = Vector3.Cross(upDir, frontDir);
            var planes = new List<Plane>(){
            new(-upDir, verticalOffset)
        };

            var topPoint = upDir * size.Y;
            for (int i = 0; i < sides; i++) {
                var radOne = (float)Math.PI * 2.0f * ((float)i / sides);
                var iTwo = (i + 1) % sides;
                var radTwo = (float)Math.PI * 2.0f * ((float)iTwo / sides);
                var pointTwo = size.X * sideDir * (float)Math.Cos(radOne) + size.Z * frontDir * (float)Math.Sin(radOne);
                var pointOne = size.X * sideDir * (float)Math.Cos(radTwo) + size.Z * frontDir * (float)Math.Sin(radTwo);
                var pointThree = topPoint;
                var normal = MathUtilities.GetCWNormal(pointOne, pointTwo, pointThree);
                // var normal = Vector3.Normalize(point - Vector3.Zero);
                planes.Add(new Plane(normal, pointOne + verticalOffset));
            }

            var tangents = planes.ConvertAll(TangentFromPlane);

            return (planes, tangents);
        }



        public static (List<Plane>, List<Vector3>) MakeRectangularCuboid(Vector3 upDir, Vector3 frontDir) {
            var halfSize = Vector3.One * 0.5f;
            var sideDir = Vector3.Cross(upDir, frontDir);


            var planes = new List<Plane>(){
                new(frontDir, frontDir*halfSize.Z),
                new(-frontDir, -frontDir*halfSize.Z),
                new(sideDir, sideDir*halfSize.X),
                new(-sideDir, -sideDir*halfSize.X),
                new(upDir, upDir*halfSize.Y),
                new(-upDir, -upDir*halfSize.Y)
            };

            var tangents = planes.ConvertAll(TangentFromPlane);

            return (planes, tangents);
        }
    }
}