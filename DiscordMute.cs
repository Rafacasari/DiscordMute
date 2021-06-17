#region
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using Keys = System.Windows.Forms.Keys;
#endregion

namespace DiscordMute
{
    public static class BuildInfo
    {
        public const string Name = "DiscordMute"; 
        public const string Description = "Mod for mute/unmute Discord directly in-game";
        public const string Author = "Rafa";
        public const string Company = "RBX";
        public const string Version = "1.1.2";
        public const string DownloadLink = null;
    }

    public class DiscordMute : MelonMod
    {
        #region DllImport
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private static readonly int KEYEVENTF_KEYUP = 0x0002;
        #endregion

        private string MuteKey;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("DiscordMute");
            MelonPreferences.CreateEntry("DiscordMute", nameof(MuteKey), "", "Mute Key", true);
            OnPreferencesSaved();
            MelonCoroutines.Start(UiManagerInitializer());
        }

        public override void OnPreferencesSaved()
        {
            MuteKey = MelonPreferences.GetEntryValue<string>("DiscordMute", nameof(MuteKey));
        }
        
        private IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }

        private GameObject DiscordButton;

        public void OnUiManagerInit()
        {
            BindManager.Initialize();

            void ShowBindManager()
            {
                BindManager.Show("Press your mute key in keyboard", new Action<List<Keys>>(selectedKeys =>
                {
                    string stringKeys = "";
                    if (selectedKeys.Count > 0) stringKeys = string.Join(",", selectedKeys);

                    MelonPreferences.SetEntryValue("DiscordMute", nameof(MuteKey), stringKeys);
                    MelonPreferences.Save();
                }), null);
            }

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Discord Bind", () => ShowBindManager());

            var originalMic = GameObject.Find("/UserInterface/QuickMenu/MicControls");
            DiscordButton = GameObject.Instantiate(originalMic, originalMic.transform);
            DiscordButton.name = "Discord";

            DiscordButton.SetActive(true);
            DiscordButton.transform.localPosition -= new Vector3(420, 7);

            DiscordButton.GetComponentInChildren<Text>().text = "Discord";

            var button = DiscordButton.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(new Action(() => {

                bool isEnabled = !GameObject.Find("/UserInterface/QuickMenu/MicControls/Discord/MicButton/MicEnabled").activeSelf;

                GameObject.Find("/UserInterface/QuickMenu/MicControls/Discord/MicButton/MicEnabled").gameObject.SetActive(isEnabled);
                GameObject.Find("/UserInterface/QuickMenu/MicControls/Discord/MicButton/MicDisabled").gameObject.SetActive(!isEnabled);

                if (!string.IsNullOrEmpty(MuteKey))
                {
                    List<Keys> selectedKeys = new List<Keys>();
                    if (!string.IsNullOrEmpty(MuteKey))
                    {
                        string[] stringKeys = MuteKey.Split(',');
                        foreach(var stringKey in stringKeys) selectedKeys.Add((Keys)Enum.Parse(typeof(Keys), stringKey));
                    }

                    // Hold and Release the selected keys
                    foreach (var key in selectedKeys) HoldKey(key);
                    foreach (var key in selectedKeys) ReleaseKey(key);

                } else ShowBindManager(); 
            }));
        }

        private void HoldKey(Keys key) => keybd_event((byte)key, (byte)key, 0, 0); 
        private void ReleaseKey(Keys key) => keybd_event((byte)key, (byte)key, KEYEVENTF_KEYUP, 0); 
    }
}
