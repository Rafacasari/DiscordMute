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

            Page.AddSimpleButton("Accept", new Action(() =>
            {
                AcceptAction?.Invoke(selectedKey);
                fetchingKeys = false;
                selectedKey = 0;
                Page.Hide();
            }));

            Page.AddSimpleButton("Cancel", new Action(() =>
            {
                CancelAction?.Invoke();
                fetchingKeys = false;
                selectedKey = 0;
                Page.Hide();
            }));  
        }


        public static void Show(string title, Action<Keys> acceptAction, Action cancelAction)
        {    
            AcceptAction = acceptAction;
            CancelAction = cancelAction;
            Page.Show();

            if (titleObject != null && titleObject.GetComponentInChildren<Text>() != null) titleObject.GetComponentInChildren<Text>().text = title;

            fetchingKeys = true;
            MelonLoader.MelonCoroutines.Start(WaitForKey());
        }

        private static Action<Keys> AcceptAction;
        private static Action CancelAction;

        private static bool fetchingKeys = false;
        public static IEnumerator WaitForKey()
        {
            while (fetchingKeys && textObject != null)
            {
                foreach (Keys inputKey in Enum.GetValues(typeof(Keys)))
                    if (!BlacklistedKeys.Contains(inputKey) && Keyboard.IsKeyDown(inputKey) && inputKey != Keys.None)
                        selectedKey = inputKey;

                if (textObject != null && selectedKey != Keys.None)
                    textObject.GetComponentInChildren<Text>().text = selectedKey.ToString();
                else if (textObject != null && selectedKey == Keys.None)
                    textObject.GetComponentInChildren<Text>().text = "Waiting for key...";

                yield return new WaitForEndOfFrame();
            }
            yield break;
        }

        private static Keys selectedKey = 0;

        #region BlacklistedKeys
        private static readonly List<Keys> BlacklistedKeys = new List<Keys>() {
            Keys.LButton, Keys.RButton
        }; // Mouse Buttons
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
