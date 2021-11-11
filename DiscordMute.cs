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
using VRC.UI.Elements.Controls;
using VRC.UI.Elements.Tooltips;
#endregion

namespace DiscordMute
{
    public static class BuildInfo
    {
        public const string Name = "DiscordMute";
        public const string Description = "Mod for mute/unmute Discord directly in-game";
        public const string Author = "Rafa";
        public const string Company = "RBX";
        public const string Version = "1.2.0";
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
            MelonPreferences.CreateEntry("DiscordMute", nameof(MuteKey), "", is_hidden: true);
            OnPreferencesSaved();

            MelonCoroutines.Start(FindMePls());
        }

        public override void OnPreferencesSaved()
        {
            MuteKey = MelonPreferences.GetEntryValue<string>("DiscordMute", nameof(MuteKey));
        }


        private static Transform UserInterface;
        private static Transform QuickMenu;
        private IEnumerator FindMePls()
        {
            MelonLogger.Msg("Waiting for VRChat UI...");
            while ((UserInterface = GameObject.Find("UserInterface")?.transform) is null)
                yield return null;

            while ((QuickMenu = UserInterface.Find("Canvas_QuickMenu(Clone)")) is null)
                yield return null;

            MelonCoroutines.Start(InitializeUI());
        }



        private GameObject DiscordButton;

        public IEnumerator InitializeUI()
        {
            MelonLogger.Msg("Initializing DiscordMute UI...");
            var parentObject = QuickMenu.Find("Container").Find("Window").transform;
            var originalMic = parentObject.Find("MicButton").gameObject;

            DiscordButton = UnityEngine.Object.Instantiate(originalMic, originalMic.transform);
            DiscordButton.name = "DiscordButton";
            DiscordButton.transform.SetParent(parentObject);

            DiscordButton.SetActive(true);
            DiscordButton.transform.localPosition += new Vector3(30, 97, 0);


            MelonLogger.Msg("Initializing Bind Manager");
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

            UnityEngine.Object.Destroy(DiscordButton.GetComponent<MicToggle>());
            UnityEngine.Object.Destroy(DiscordButton.GetComponent<Toggle>());

            yield return null; // Wait object destroy

            var tooltip = DiscordButton.GetComponent<UiToggleTooltip>();

            tooltip.field_Public_String_0 = "Discord";
            tooltip.field_Public_String_1 = "Discord";

            var toggle = DiscordButton.AddComponent<Toggle>();

            yield return null; // Wait component

            toggle.onValueChanged.RemoveAllListeners();

            toggle.isOn = true;
            toggle.onValueChanged.AddListener(new Action<bool>(value =>
            {
                if (!string.IsNullOrEmpty(MuteKey))
                {
                    List<Keys> selectedKeys = new List<Keys>();
                    if (!string.IsNullOrEmpty(MuteKey))
                    {
                        string[] stringKeys = MuteKey.Split(',');
                        foreach (var stringKey in stringKeys) selectedKeys.Add((Keys)Enum.Parse(typeof(Keys), stringKey));
                    }

                    // Hold and Release the selected keys
                    foreach (var key in selectedKeys) HoldKey(key);
                    foreach (var key in selectedKeys) ReleaseKey(key);

                }
                else ShowBindManager();
            }));

            yield break;
        }

        private void HoldKey(Keys key) => keybd_event((byte)key, (byte)key, 0, 0);
        private void ReleaseKey(Keys key) => keybd_event((byte)key, (byte)key, KEYEVENTF_KEYUP, 0);
    }
}
