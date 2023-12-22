using System.Collections.Generic;

namespace PolyArchitect.Core {
    public partial class Brush {
        private class HCmdSetShape : IHCommand {
            public string cliCmdName => throw new System.NotImplementedException();

            public HCmdSetShape(Brush brush, IEnumerable<Plane> planes) {

            }

            public void Apply() {
                throw new System.NotImplementedException();
            }

            public void Undo() {
                throw new System.NotImplementedException();
            }
        }

        // private class HCmdMakeCuboidTemplate : IHCommand {
        //     public string cliCmdName => throw new System.NotImplementedException();
        //     private Scene.HCmdMakeEmptyBrush makeEmptyBrushCmd;
        //     private Brush.HCmdSetShape setShapeCmd;

        //     public HCmdMakeCuboidTemplate(Brush brush, float xSize, float ySize, float zSize) {

        //     }

        //     public void Apply() {
        //         throw new System.NotImplementedException();
        //     }

        //     public void Undo() {
        //         throw new System.NotImplementedException();
        //     }
        // }


        private class HCmdSetFaceAttribute<T> : IHCommand {
            public string cliCmdName => throw new System.NotImplementedException();


            public HCmdSetFaceAttribute(Brush brush, int faceID, T attributeValue) {
            }

            public void Apply() {
                throw new System.NotImplementedException();
            }

            public void Undo() {
                throw new System.NotImplementedException();
            }
        }
    }
}