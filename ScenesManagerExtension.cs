using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandoTestbed
{
    public static class ScenesManagerExtension
    {
        public static void AdditivelyLoadSceneWithoutDependencies(this ScenesManager scenesManager, RuntimeSceneMetaData sceneMetaData, bool async, bool keepPreloaded = false)
        {
            //var field = AccessTools.Field(typeof(MapStone), nameof(MapStone.CurrentState));

            SceneManagerScene fromCurrentScenes = scenesManager.GetFromCurrentScenes(sceneMetaData);
            if (fromCurrentScenes != null)
            {
                if (fromCurrentScenes.CurrentState == SceneManagerScene.State.LoadingCancelled)
                {
                    fromCurrentScenes.ChangeState(SceneManagerScene.State.Loading);
                    //this.LoadDependantScenes(fromCurrentScenes.MetaData, true);
                    if (keepPreloaded)
                    {
                        fromCurrentScenes.PreventUnloading = true;
                        return;
                    }
                }
            }
            else if (Application.CanStreamedLevelBeLoaded(sceneMetaData.Scene)) //else if (this.CanLevelBeLoaded(sceneMetaData.Scene))
            {
                if (scenesManager.CanLoadScenes)
                {
                    if (async)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        Application.LoadLevelAdditiveAsync(sceneMetaData.Scene).priority = 2;
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        Application.LoadLevelAdditive(sceneMetaData.Scene);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
                SceneManagerScene sceneManagerScene = new SceneManagerScene(sceneMetaData);
                scenesManager.ActiveScenes.Add(sceneManagerScene);
                sceneManagerScene.PreventUnloading = keepPreloaded;
                //this.LoadDependantScenes(sceneMetaData, async);
            }
        }

    }

}
