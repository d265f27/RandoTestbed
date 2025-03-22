using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using HarmonyLib;
using UnityEngine;

namespace RandoTestbed
{
    internal class ExtraDebugText : MonoBehaviour
    {
        private void Awake()
        {
            textField = this.gameObject.GetComponent<GUIText>();
        }
    
        private void Update()
        {
            string position = "";
            if (DebugGUIText.Enabled && Characters.Sein != null && Characters.Sein.Active)
            {
                Camera camera = UI.Cameras.Current.Camera;
                Vector2 cursorPosition = Core.Input.CursorPosition;
                Vector2 vector = camera.ViewportToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, -camera.transform.position.z));
                position = string.Format("Ori (World) X: {0} / Y: {1}\nCursor (World) X {2} / Y: {3}", new object[]
                {
                    Characters.Sein.Position.x,
                    Characters.Sein.Position.y,
                    vector.x,
                    vector.y
                });
            }
            textField.text = position;
        }
    
        GUIText textField;
    }
    
    [HarmonyPatch(typeof(DebugGUIText), nameof(DebugGUIText.Awake))]
    class DebugGUITextHook
    {
        static void Postfix(DebugGUIText __instance)
        {
            Transform deathText = __instance.transform.Find("deathText");
            Transform deaths = __instance.transform.Find("deaths");
            Transform timeText = __instance.transform.Find("timeText");
            Transform time = __instance.transform.Find("time");
            Transform soundText = __instance.transform.Find("soundText");
            Transform sound = __instance.transform.Find("sound");
                
            // Move things up a bit for space.
            deathText.transform.position += new Vector3(0.0f, 0.015f, 0.0f);
            deaths.transform.position += new Vector3(0.0f, 0.015f, 0.0f);
            timeText.transform.position += new Vector3(0.0f, 0.03f, 0.0f);
            time.transform.position += new Vector3(0.0f, 0.03f, 0.0f);
            soundText.transform.position += new Vector3(0.0f, 0.051f, 0.0f);
            sound.transform.position += new Vector3(0.0f, 0.051f, 0.0f);
    
            // Clone and add our debug text class as a component.
            Transform positionInfo = UnityEngine.Object.Instantiate<Transform>(soundText);
            positionInfo.parent = soundText.parent;
            positionInfo.gameObject.AddComponent<ExtraDebugText>();
            positionInfo.name = "positionInfo";
            positionInfo.transform.position -= new Vector3(0.0f, 0.025f, 0.0f);
            GUIText textField2 = positionInfo.gameObject.GetComponent<GUIText>();
            textField2.text = "";
            textField2.fontSize = 18;
        }
    }
}
