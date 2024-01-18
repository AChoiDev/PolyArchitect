using System;
using System.Collections.Generic;

namespace PolyArchitect.Core {
    using SceneID = int;
    public class SceneRegistry {
        private readonly Dictionary<SceneID, Scene> scenes = [];

        public Scene GetScene(SceneID sceneID) => scenes[sceneID];

        public SceneID MakeScene() {
            var sceneId = CreateSceneId();

            Scene scene = new();
            scenes.Add(sceneId, scene);

            return sceneId;
        }

        public void DeleteScene(SceneID sceneId) {
            if (scenes.ContainsKey(sceneId)) {
                Scene scene = scenes[sceneId];
            }

            scenes.Remove(sceneId);
        }


        private static int idCounter = 0;
        private SceneID CreateSceneId() {
            var id = idCounter;
            idCounter += 1;

            return id;
        }
    }
}
