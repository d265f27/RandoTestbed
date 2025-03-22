using BepInEx;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using OriModding;
using OriModding.BF.Core;

namespace RandoTestbed
{
    [BepInDependency(OriModding.BF.Core.PluginInfo.PLUGIN_GUID)]
    [BepInDependency(OriModding.BF.ConfigMenu.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance = null;
        private Harmony harmony;
        public static PreloadBootstrap preloadBoostrap = null;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            instance = this;
            harmony = new Harmony("com.ori.randomiser");
            harmony.PatchAll();
            Controllers.Add<Preloader>(null, "RandoTestbed");
        }
        public static void Log(string message)
        {
            instance.Logger.LogInfo(message);
        }


    }

    public static class GeneralExtensions
    {
        public static string GetNicePath(this Transform element)
        {
            string output = element.name;
            while (element.parent != null)
            {
                element = element.parent;
                output = element.name + "/" + output;
            }

            return output;
        }

        public static RectTransform rectTransform(this Transform element)
        {
            return (RectTransform)element;
        }

        public static List<Transform> FindChildrenByName(this Transform parent, string name)
        {
            List<Transform> results = new List<Transform>();

            Transform[] children = parent.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.gameObject.name == name)
                {
                    results.Add(child);
                }
            }
            return results;
        }
    }

}
