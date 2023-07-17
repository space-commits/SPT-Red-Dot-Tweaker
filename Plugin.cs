using BepInEx;
using BepInEx.Configuration;
using EFT;
using UnityEngine;

namespace RedDotTweaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> AdjustmentSpeed { get; set; }
        public static ConfigEntry<KeyboardShortcut> RaiseBrightnessKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> LowerBrightnessKey { get; set; }

        public static ConfigEntry<bool> EnableColorChange { get; set; }
        public static ConfigEntry<float> r { get; set; }
        public static ConfigEntry<float> g { get; set; }
        public static ConfigEntry<float> b { get; set; }

        public static ConfigEntry<float> Scale { get; set; }

        public static ConfigEntry<float> BrightnessLimit { get; set; }

        public static Player player;

        public static float AdjustmentValue = 1f;

        public static Vector3 LastBaseColor = Vector3.zero;

        public static Color CurrentColor = Color.white;

        private void Awake()
        {
            string bright = "1. Brightness";
            string color = "1. Color";
            string size = "1. Size";

            AdjustmentSpeed = Config.Bind<float>(bright, "Brightness Adjustment Steps", 0.95f, new ConfigDescription("Lower = Faster.", new AcceptableValueRange<float>(0.01f, 0.999f), new ConfigurationManagerAttributes { Order = 40 }));
            RaiseBrightnessKey = Config.Bind(bright, "Increase Brightness", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 30 }));
            LowerBrightnessKey = Config.Bind(bright, "Lower Brightness", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 20 }));
            BrightnessLimit = Config.Bind<float>(bright, "Brightness Limit", 20f, new ConfigDescription("Lower = Faster.", new AcceptableValueRange<float>(0.1f, 100f), new ConfigurationManagerAttributes { Order = 1 }));

            EnableColorChange = Config.Bind<bool>(color, "Enable Color Change", true, new ConfigDescription("If Enabled All Reddots Will Use This Color", null, new ConfigurationManagerAttributes { Order = 5 }));
            r = Config.Bind<float>(color, "R", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 4 }));
            g = Config.Bind<float>(color, "G", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 3 }));
            b = Config.Bind<float>(color, "B", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f), new ConfigurationManagerAttributes { Order = 2 }));

            Scale = Config.Bind<float>(size, "Dot Scale", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 2f), new ConfigurationManagerAttributes { Order = 2 }));

            CollimatorSight.OnCollimatorUpdated += colmUpdate;
        }

        public void HandleBrightnessAdjustment(CollimatorSight sight, Material mat, float adjustment)
        {
            Plugin.AdjustmentValue = Mathf.Clamp(Plugin.AdjustmentValue * adjustment, 0.1f, Plugin.BrightnessLimit.Value);
            UpdateColor(sight, mat, adjustment);
        }

        public void UpdateColor(CollimatorSight sight, Material mat, float adjustment)
        {
            Color changedBaseColor = new Color(r.Value * Plugin.AdjustmentValue, g.Value * Plugin.AdjustmentValue, b.Value * Plugin.AdjustmentValue, 1 * Plugin.AdjustmentValue);
            Color adjustedBaseColor = UpdateBaseColor(sight, mat, adjustment);

            mat.color = EnableColorChange.Value ? changedBaseColor : adjustedBaseColor;
            CurrentColor = mat.color;
        }

        public Color UpdateBaseColor(CollimatorSight sight, Material mat, float adjustment) 
        {
            if (mat.color.r > mat.color.g && mat.color.r > mat.color.b) 
            {
                return new Color(mat.color.r * adjustment, mat.color.g, mat.color.b, 1f * Plugin.AdjustmentValue);
            }
            if (mat.color.g > mat.color.r && mat.color.g > mat.color.b)
            {
                return new Color(mat.color.r, mat.color.g * adjustment, mat.color.b, 1f * Plugin.AdjustmentValue);
            }
            if (mat.color.b > mat.color.r && mat.color.b > mat.color.g)
            {
                return new Color(mat.color.r, mat.color.g, mat.color.b * adjustment, 1f * Plugin.AdjustmentValue);
            }
            return mat.color;
        }

        public void HandleSizeAdjustment(CollimatorSight sight, Material mat)
        {
            Vector3 scale = new Vector3(Scale.Value, Scale.Value, Scale.Value);
            sight.transform.localScale = scale;
        }

        private void colmUpdate(CollimatorSight sight)
        {
            Material mat = sight.CollimatorMeshRenderer.material;

            if (Input.GetKey(RaiseBrightnessKey.Value.MainKey))
            {
                float brightnessAdjustment = 1f + (1f - Plugin.AdjustmentSpeed.Value);
                HandleBrightnessAdjustment(sight, mat, brightnessAdjustment);
            }
            if (Input.GetKey(LowerBrightnessKey.Value.MainKey))
            {
                HandleBrightnessAdjustment(sight, mat, Plugin.AdjustmentSpeed.Value);
            }

            bool sightChanged = mat.color != CurrentColor;   
            Vector3 currentBaseColor = new Vector3(r.Value, g.Value, b.Value);
            if (EnableColorChange.Value && (LastBaseColor != currentBaseColor || sightChanged)) 
            {
                UpdateColor(sight, mat, Plugin.AdjustmentSpeed.Value);
            }
            LastBaseColor = currentBaseColor;
            HandleSizeAdjustment(sight, mat);

            Logger.LogWarning(mat.color);

            sight.CollimatorMeshRenderer.enabled = false;
            sight.CollimatorMeshRenderer.enabled = true;
        }
    }
}
