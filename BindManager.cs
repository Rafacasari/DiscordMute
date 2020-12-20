#region
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using UnityEngine.UI;
using UIExpansionKit.API;
#endregion

namespace DiscordMute
{
    public static class BindManager
    {
        public static ICustomShowableLayoutedMenu Page;
        private static GameObject titleObject;
        private static GameObject textObject;

        public static void Initialize()
        {
            Page = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);    
            
            Page.AddLabel("Title", new Action<GameObject>((obj) => { titleObject = obj; }));    
            Page.AddLabel("Waiting for key...", new Action<GameObject>((obj) => { textObject = obj; }));

            Page.AddSimpleButton("Clear", new Action(() =>
            {
                selectedKeys.Clear();
            }));

            Page.AddSimpleButton("Accept", new Action(() =>
            {
                AcceptAction?.Invoke(selectedKeys);
                fetchingKeys = false;
                Page.Hide();
            }));

            Page.AddSimpleButton("Cancel", new Action(() =>
            {
                CancelAction?.Invoke();
                fetchingKeys = false;
                Page.Hide();
            }));   
        }


        public static void Show(string title, Action<List<Keys>> acceptAction, Action cancelAction)
        {
            selectedKeys.Clear();
            AcceptAction = acceptAction;
            CancelAction = cancelAction;
            Page.Show();

            if (titleObject != null && titleObject.GetComponentInChildren<Text>() != null) titleObject.GetComponentInChildren<Text>().text = title;

            fetchingKeys = true;
            MelonLoader.MelonCoroutines.Start(WaitForKey());
        }

        private static Action<List<Keys>> AcceptAction;
        private static Action CancelAction;

        private static bool fetchingKeys = false;
        public static IEnumerator WaitForKey()
        {
            while (fetchingKeys && textObject != null)
            {
                foreach (Keys inputKey in Enum.GetValues(typeof(Keys)))
                {
                    if (BlacklistedKeys.Contains(inputKey) || inputKey == Keys.None) continue;
         
                    if (Keyboard.IsKeyDown(inputKey) && !selectedKeys.Contains(inputKey) && selectedKeys.Count < 4) // Discord Max Limit
                        selectedKeys.Add(inputKey);
                }

                if (textObject != null && selectedKeys.Count == 0)
                    textObject.GetComponentInChildren<Text>().text = "Waiting for key...";
                else if (textObject != null)
                {
                    List<string> names = new List<string>();
                    foreach (var name in selectedKeys) names.Add(GetName(name));

                    textObject.GetComponentInChildren<Text>().text = string.Join(" + ", names);
                }
                
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }
        private static string GetName(Keys key)
        {
            switch (key)
            {
                case Keys.LMenu: return "ALT";
                case Keys.RMenu: return "RIGHT ALT";
                case Keys.LControlKey: return  "CTRL";
                case Keys.RControlKey: return "RIGHT CTRL";
                case Keys.LShiftKey: return "SHIFT";
                case Keys.RShiftKey: return "RIGHT SHIFT";
                case Keys.XButton1: return "MOUSE3";
                case Keys.XButton2: return "MOUSE4";

                default:
                    return key.ToString();
            }
        }
        private static readonly List<Keys> selectedKeys = new List<Keys>();

        #region BlacklistedKeys
        private static readonly List<Keys> BlacklistedKeys = new List<Keys>() {
            Keys.LButton, Keys.RButton, Keys.ControlKey, Keys.ShiftKey, Keys.Menu, Keys.LShiftKey
        }; // BlackList Mouse Buttons
        #endregion
    }

    public abstract class Keyboard
    {
        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(Keys key) => KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        public static bool IsKeyToggled(Keys key) => KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);  
    }
}
