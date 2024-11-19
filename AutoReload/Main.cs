using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

namespace AutoReload
{
    public class Main : MelonMod
    {
        private PlayerInventoryScript piss;
        private bool isloaded;
        private FieldInfo atks_f;
        private Type atks_t = typeof(AttackScript);
        private AttackScript atks;
        private FieldInfo atks_ReloadTimer_f;
        private MelonPreferences_Category AutoreloadMod;
        private MelonPreferences_Entry<bool> melonentry_autoreload;
        private GameObject prefab;
        TextMeshProUGUI autoreload_text;
        private AssetBundle assets;
        private string scene;

        public override void OnInitializeMelon()
        {
            AutoreloadMod = MelonPreferences.CreateCategory("AutoreloadMod");
            melonentry_autoreload = AutoreloadMod.CreateEntry<bool>("autoreload", true);
            MelonPreferences.Save();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            scene = sceneName;
            if (sceneName != "MainMenu")
            {
                ldl();
            }

            switch (sceneName)
            {
                case ("MainMenu"):
                    MainMenuInit();
                    break;
            }
        }
        private void ldl()
        {
            try
            {
                piss = GameObject.Find("Player").GetComponentInChildren<PlayerInventoryScript>();
                atks = GameObject.Find("Player").GetComponentInChildren<AttackScript>();
                atks_f = atks_t.GetField("RoundInChamber", BindingFlags.NonPublic | BindingFlags.Instance);
                atks_ReloadTimer_f = atks_t.GetField("ReloadTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch { return; }
        }
        public void MainMenuInit()
        {
            string assetpath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "AutoReloadMod/modmenubundle");
            if (assets == null)
            {
                assets = AssetBundle.LoadFromFile(assetpath);
            }
            if (assets == null && prefab == null)
            {
                LoggerInstance.Msg("Couldn't load asste bundle from" + assetpath);
                return;
            }
            prefab = assets.LoadAsset<GameObject>("modmenucontainer.prefab");
            Transform mainmenu = GameObject.Find("Menus/MainMenu/").transform;
            Tree.Instantiate(prefab, mainmenu);
            Button autoreload_button = GameObject.Find("Menus/MainMenu/ModMenuContainer(Clone)/Button_Autoreload").GetComponent<Button>();
            autoreload_button.onClick.AddListener(ToggleReload);
            Button Button_close = GameObject.Find("Menus/MainMenu/ModMenuContainer(Clone)/Button_close").GetComponent<Button>();
            Button_close.onClick.AddListener(CloseMenu);
            autoreload_text = GameObject.Find("Menus/MainMenu/ModMenuContainer(Clone)/Button_Autoreload/Text (TMP)").GetComponent<TextMeshProUGUI>();
            ReloadStatusUpdate();
            return;
        }
        public override void OnUpdate()
        {
            if (piss != null && melonentry_autoreload.Value && scene != "MainMenu")
            {
                TryReload();
            }
        }

        public void CloseMenu()
        {
            Tree.Destroy(GameObject.Find("Menus/MainMenu/ModMenuContainer(Clone)"));
        }

        public void TryReload()
        {
            isloaded = (bool)atks_f.GetValue(atks);
            float ReloadTimer = (float)atks_ReloadTimer_f.GetValue(atks);
            if (!isloaded && atks != null && ReloadTimer == 0 && piss.MyAmmo > 0)
            {
                atks.Reload();
                return;
            }

        }

        public void ToggleReload()
        {
            melonentry_autoreload.Value = !melonentry_autoreload.Value;
            MelonPreferences.Save();
            ReloadStatusUpdate();
            return;
        }
        public void ReloadStatusUpdate()
        {
            if (melonentry_autoreload.Value)
            {
                autoreload_text.text = "Enabled";
                autoreload_text.color = new Color(0, 1, 0, 1);
            }
            else
            {
                autoreload_text.text = "Disabled";
                autoreload_text.color = new Color(1, 0, 0, 1);
            }
            return;
        }
    }
}
