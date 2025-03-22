using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using UnityEngine;

namespace RandoTestbed
{
    public class PreloadBootstrap : MonoBehaviour
    {
        public static PreloadBootstrap Instance = null;
        public static Dictionary<string, Action<SceneRoot>> BootstrapActions = new Dictionary<string, Action<SceneRoot>>();
        private static readonly HashSet<string> loadedScenes = new HashSet<string>();

        public PreloadBootstrap()
        {
            Events.Scheduler.OnSceneRootPreEnabled.Add(OnSceneRootPreEnabled);
            On.SceneRoot.Unload += (orig, self) =>
            {
                orig(self);
                OnSceneRootUnloaded(self.name);
            };
            SetupBootstrap();
            Instance = this;
        }

        public static void SetupBootstrap()
        {
            BootstrapActions = new Dictionary<string, Action<SceneRoot>>
            {
                ["sunkenGladesRunaway"] = BootstrapSpawn,
            };
        }

        private static void OnSceneRootPreEnabled(SceneRoot sceneRoot)
        {
            if (!loadedScenes.Contains(sceneRoot.name) && BootstrapActions.ContainsKey(sceneRoot.name))
            {
                BootstrapActions[sceneRoot.name].Invoke(sceneRoot);
                loadedScenes.Add(sceneRoot.name);
            }
        }

        private static void OnSceneRootUnloaded(string name)
        {
            loadedScenes.Remove(name);
        }

        private static void BootstrapSpawn(SceneRoot sceneRoot)
        {
            Plugin.Log("Bootstrapping spawn.");
            if (Preloader.plant != null)
            {
                Preloader.PlacePlant(sceneRoot, new Vector3(150.0f, -211.0f), new MoonGuid(123, 234, 345, 456));
            }
            if (Preloader.jumperEnemy != null)
            {
                Preloader.PlaceJumperEnemy(sceneRoot, new Vector3(147.0f, -211.0f), new MoonGuid(1234, 2345, 3456, 4567));
            }
            if (Preloader.isPreloaded)
            {
                Preloader.PlaceGeneric(sceneRoot, "Spitter", new Vector3(144.0f, -211.0f), new MoonGuid(12134, 23415, 34536, 455167));
                //Preloader.PlaceGeneric(sceneRoot, "StarSlug", new Vector3(171.0f, -211.0f), new MoonGuid(12134, 23415, 34536, 455167));

                // Long spider test.
                //Preloader.PlaceShootingSpider(sceneRoot, new Vector3(242.0f, -186.0f), new MoonGuid(12134, 23415, 34536, 455167));


                // Working base spider    
                //Preloader.PlaceShootingSpiderWorking(sceneRoot, new Vector3(257.0f, -198.0f), new MoonGuid(12134, 23415, 34536, 455167));


                //Preloader.PlaceGeneric(sceneRoot, "AcidSlug", new Vector3(175.0f, -221.5f), new MoonGuid(12134, 23415, 34536, 455167));
                //Preloader.PlaceGeneric(sceneRoot, "RammingEnemy", new Vector3(164.0f, -211.0f), new MoonGuid(12134, 23415, 34536, 455167));
                //Preloader.PlaceGeneric(sceneRoot, "KamikazeSoot", new Vector3(171.0f, -211.0f), new MoonGuid(12134, 23415, 34536, 455167));
            }
        }



    }
}
