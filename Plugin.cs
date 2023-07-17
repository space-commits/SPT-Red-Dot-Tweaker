using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using HarmonyLib;
using EFT.Animations;
using EFT.InventoryLogic;
using EFT.CameraControl;
using BepInEx.Bootstrap;
using EFT.UI;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Sirenix.Serialization.Utilities;

namespace RedDotTweaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> AdjustmentSpeed { get; set; }
        public static ConfigEntry<KeyboardShortcut> RaiseBrightnessKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> LowerBrightnessKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> UpdateKey { get; set; }


        public static ConfigEntry<bool> EnableColorChange { get; set; }
        public static ConfigEntry<float> r { get; set; }
        public static ConfigEntry<float> g { get; set; }
        public static ConfigEntry<float> b { get; set; }

        public static ConfigEntry<float> Scale { get; set; }

        public static ConfigEntry<float> Limit { get; set; }

        public static Player player;

        public static bool hasRun = false;

        private void Awake()
        {
            string bright = "1. Brightness";
            string color = "1. Color";
            string size = "1. Size";

            AdjustmentSpeed = Config.Bind<float>(bright, "Brightness Adjustment Steps", 0.95f, new ConfigDescription("Lower = Faster.", new AcceptableValueRange<float>(0.01f, 0.99f), new ConfigurationManagerAttributes { Order = 40 }));
            RaiseBrightnessKey = Config.Bind(bright, "Increase Brightness", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 30 }));
            LowerBrightnessKey = Config.Bind(bright, "Lower Brightness", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 20 }));
            Limit = Config.Bind<float>(bright, "Brightness Limit", 15f, new ConfigDescription("Lower = Faster.", new AcceptableValueRange<float>(0.1f, 20f), new ConfigurationManagerAttributes { Order = 1 }));

            UpdateKey = Config.Bind(bright, "Update Size and Color", new KeyboardShortcut(KeyCode.KeypadEnter), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 30 }));
            EnableColorChange = Config.Bind<bool>(bright, "Enable Color Change", true, new ConfigDescription("If Enabled All Reddots Will Use This Color", null, new ConfigurationManagerAttributes { Order = 5 }));
            r = Config.Bind<float>(color, "R", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 4 }));
            g = Config.Bind<float>(color, "G", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 3 }));
            b = Config.Bind<float>(color, "B", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 2 }));

            Scale = Config.Bind<float>(size, "Dot Scale", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 2 }));


            CollimatorSight.OnCollimatorUpdated += colmUpdate;
        }

        public void HandleBrightnessAdjustment(CollimatorSight sight, Material mat, float adjustment)
        {
            Color color = new Color(mat.color.r * adjustment, mat.color.g * adjustment, mat.color.b * adjustment, mat.color.a * adjustment);
            mat.color = color;
            sight.CollimatorMeshRenderer.enabled = false;
            sight.CollimatorMeshRenderer.enabled = true;

            Logger.LogWarning(mat.color);
        }

        public void HandleSizeAdjustment(CollimatorSight sight, Material mat)
        {
            Vector3 scale = new Vector3(Scale.Value, Scale.Value, Scale.Value);
            sight.transform.localScale = scale;
        }

        public void HandleColorChange(Material mat)
        {
            Color color = new Color(r.Value, g.Value, b.Value, 1);
            mat.color = color;
        }

        private void colmUpdate(CollimatorSight sight)
        {
            Material mat = sight.CollimatorMeshRenderer.material;
            if (Input.GetKey(RaiseBrightnessKey.Value.MainKey))
            {
                if (mat.color.a < Limit.Value)
                {
                    float brightnessAdjustment = 1f + (1f - Plugin.AdjustmentSpeed.Value);
                    HandleBrightnessAdjustment(sight, mat, brightnessAdjustment);
                }
            }
            if (Input.GetKey(LowerBrightnessKey.Value.MainKey))
            {
                if (mat.color.a > 0.01)
                {
                    HandleBrightnessAdjustment(sight, mat, Plugin.AdjustmentSpeed.Value);
                }
            }
            if (Input.GetKeyDown(UpdateKey.Value.MainKey))
            {
                HandleSizeAdjustment(sight, mat);
                if (EnableColorChange.Value)
                {
                    HandleColorChange(mat);
                }
                sight.CollimatorMeshRenderer.enabled = false;
                sight.CollimatorMeshRenderer.enabled = true;
            }
        }
    }
}
