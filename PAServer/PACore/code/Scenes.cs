using System;
using System.Collections.Generic;

namespace PolyArchitect.Core {
    public class Scenes {
        private Dictionary<Guid, Scene> scenes = new();
        private Scene? activeScene = null;

        // If there is no active scene the newly made scene will become the active scene.
        public Guid MakeScene() {
            Guid sceneId = CreateSceneId();

            Scene scene = new Scene();
            scenes.Add(sceneId, scene);

            if (activeScene == null) {
                activeScene = scene;
            }

            return sceneId;
        }

        public void SetActiveScene(Guid sceneId) {
            if (scenes.ContainsKey(sceneId)) {
                activeScene = scenes[sceneId];
            }
        }

        public void DeleteScene(Guid sceneId) {
            if (scenes.ContainsKey(sceneId)) {
                Scene scene = scenes[sceneId];

                if (activeScene == scene) {
                    activeScene = null;
                }
            }

            scenes.Remove(sceneId);
        }

        private Guid CreateSceneId() {
            Guid guid = Guid.NewGuid();

            // NOTE: On the impossibly rare chance of a collision, try again.
            while (scenes.ContainsKey(guid)) {
                guid = Guid.NewGuid();
            }

            return guid;
        }
    }
}
