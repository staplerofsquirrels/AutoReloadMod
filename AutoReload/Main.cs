using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private Stream dest;
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
        public string[] getresourcenames(string resourcedomain)
        {
            string assemblyname = Assembly.GetExecutingAssembly().GetName().Name;
            string[] allresource = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string[] resource_names = new string[] { };

            foreach (string resource in allresource)
            {
                bool isindomain = resource.StartsWith(assemblyname + "." + resourcedomain + ".");
                if (isindomain)
                {
                    resource_names = resource_names.Append<string>(resource).ToArray();
                }
            }
            return resource_names;
        }

        public void extractresource(string resource, string file)
        {
            FileStream destfile = File.Open(file, FileMode.OpenOrCreate);
            Stream resourcefilestream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            resourcefilestream.CopyTo(destfile);
            destfile.Close();
            resourcefilestream.Close();
        }

        public void assetbundlecheck(string resourcedomain, string assetpath_base)
        {
            string assemblyname = Assembly.GetExecutingAssembly().GetName().Name;
            //string assetpath_base = Path.Combine(UnityEngine.Application.streamingAssetsPath, "AutoReloadMod");
            //string resourcedomain = "asset_bundles";
            string[] resourcenames = getresourcenames(resourcedomain);

            if (!Directory.Exists(assetpath_base))
            {
                LoggerInstance.Msg(assetpath_base + " does not exists");
                Directory.CreateDirectory(assetpath_base);
            }
            else { LoggerInstance.Msg(assetpath_base + " exists");}

            foreach (string resourcename in resourcenames)
            {
                string filepath = Path.Combine(assetpath_base, resourcename.Substring(assemblyname.Length+resourcedomain.Length +2));
                MD5 md5 = MD5.Create();
                bool filecheck = false;
                if (File.Exists(filepath))
                {
                    Stream embeded = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcename);
                    Stream local = File.OpenRead(filepath);
                    byte[] embeded_hash = md5.ComputeHash(embeded);
                    byte[] local_hash = md5.ComputeHash(local);
                    embeded.Close();
                    local.Close();
                    filecheck = embeded_hash.SequenceEqual(local_hash);
                    if (!filecheck)
                    {
                        LoggerInstance.Msg("Hash check failed for:" + filepath + ":; MD5 hash:" + BitConverter.ToString(local_hash) + "\n doesn't match with: " + BitConverter.ToString(embeded_hash));
                    }
                    else
                    {
                        LoggerInstance.Msg("Filecheck was sucessful");
                    }
                    
                }

                if (!filecheck)
                {
                    LoggerInstance.Msg("extracting " + resourcename + " to:" + filepath);
                    extractresource(resourcename, filepath);
                }
            }
        }

        public void MainMenuInit()
        {
            string assetpath_base = Path.Combine(UnityEngine.Application.streamingAssetsPath, "AutoReloadMod");
            string[] resourcenames = getresourcenames("asset_bundles");

            assetbundlecheck("asset_bundles", assetpath_base);

            if (assets == null)
            {
                assets = AssetBundle.LoadFromFile(Path.Combine(assetpath_base, "modmenubundle"));
            }
            if (assets == null && prefab == null)
            {
                LoggerInstance.Msg("Couldn't load asste bundle(s)");
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
