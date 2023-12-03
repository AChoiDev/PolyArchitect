using System.Collections.Generic;
using System.Linq;

using PolyArchitect.Core;
namespace PolyArchitect {

public static class CoreAPI {

    // expect a low number of scenes to be loaded
    public const int MAX_SCENES_LOADED_LIMIT = 300;
    private static Scene[] scenes = new Scene[MAX_SCENES_LOADED_LIMIT];
    private static int? currentSceneID = null;
    private static int findUnusedSceneID() 
        => Enumerable.Range(0, MAX_SCENES_LOADED_LIMIT).First((id) => scenes[id] == null);
    

    public static int MakeScene() {
        var sceneID = findUnusedSceneID();
        scenes[sceneID] = new Scene();
        return sceneID;
    }

    public static void UseScene(int sceneID) {
        currentSceneID = sceneID;
    }

    public static void DeleteScene(int sceneID) {
        scenes[sceneID] = null;
    }

    public static int LoadScene(string filePath) {
        throw new System.NotImplementedException();
    }
    public static void SaveScene(string filePath) {
        throw new System.NotImplementedException();
    }

}
}