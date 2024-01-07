using System;
using System.Collections.Generic;

namespace PolyArchitect.Core {
    public class SceneRegistry {
        private readonly Dictionary<Guid, Scene> scenes = [];

        public Scene GetScene(Guid sceneID) => scenes[sceneID];

        public Guid MakeScene() {
            Guid sceneId = CreateSceneId();

            Scene scene = new();
            scenes.Add(sceneId, scene);

            return sceneId;
        }

        public void DeleteScene(Guid sceneId) {
            if (scenes.ContainsKey(sceneId)) {
                Scene scene = scenes[sceneId];
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
