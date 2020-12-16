using MelonLoader;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using Keys = System.Windows.Forms.Keys;

namespace DiscordMute
{

    public static class BuildInfo
    {
        public const string Name = "DiscordMute"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Mod for mute/unmute Discord in-game"; // Description for the Mod.  (Set as null if none)
        public const string Author = "Rafa"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
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
            MelonPrefs.RegisterString("DiscordMute", nameof(MuteKey), "", null, true);
            OnModSettingsApplied();

            //MelonCoroutines.Start(WaitForUi());
        }

        public override void OnModSettingsApplied()
        {
            MuteKey = MelonPrefs.GetString("DiscordMute", nameof(MuteKey));
        }

        private GameObject DiscordButton;

        //public IEnumerator WaitForUi()
        //{
        //    while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;

            

            
        //}

        public override void VRChat_OnUiManagerInit()
        {
            BindManager.Initialize();

            ICustomLayoutedMenu quickMenu = ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu);
            quickMenu.AddSimpleButton("Select Discord Bind", () => BindManager.Show("Press your mute key in keyboard", new Action<Keys>(selectedKey =>
            {
                MelonPrefs.SetString("DiscordMute", nameof(MuteKey), selectedKey.ToString());
                MelonPrefs.SaveConfig();
                
            }), null));


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
                    var bind = (Keys)Enum.Parse(typeof(Keys), MuteKey);
                    keybd_event((byte)bind, (byte)bind, 0, 0);
                    keybd_event((byte)bind, (byte)bind, KEYEVENTF_KEYUP, 0);
                }
            }));
        }
    }
}
