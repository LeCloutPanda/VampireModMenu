using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEngine;
using Il2CppVampireSurvivors.UI;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine.Windows;

namespace VampireModMenu
{
    public class ConfigData
    {
        public string Name = ModInfo.Name;
        public string Color = "#74b884";
        public string FontFileName = "LiberationSans.ttf";
    }

    public static class ModInfo
    {
        public const string Name = "Vampire Mod Menu";
        public const string Description = "Adds a configuration screen for mods.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.0.39";
        public const string DownloadLink = "";
    }

    public class VampireModMenuMod : MelonMod
    {
        // Need to fix/implement
        // Checks to see if font is valid and if it's not download/load one from system
        // Add menu scrolling 
        // Fix menu appearing at end of run
        // Provide a guide on how to use

        static ConfigData config = new ConfigData();

        static readonly string ConfigDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");
        static readonly string ConfigFile = Path.Combine(ConfigDirectory, "VampireModMenu.json");
        static string FontDirectory;
       
        static TMP_FontAsset newFont;
        static bool createdConfigPanel = true;
        static Transform configPanel = null;
        static Transform optionsPage = null;

        DateTime lastModified;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            ValidateConfig();

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.vampiremodmenu");
            harmony.PatchAll();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (sceneName.ToLower() == "mainmenu") createdConfigPanel = false;
        }

        static void CreateMenu()
        {
            if (createdConfigPanel) return;

            MelonLogger.Msg("Creating Mod Configuration Panel");
            try {
                Font f = new Font();
                Font.Internal_CreateFontFromPath(f, FontDirectory);
                f.hideFlags = HideFlags.DontUnloadUnusedAsset;
                newFont = TMP_FontAsset.CreateFontAsset(f);
                newFont.material.EnableKeyword("UNDERLAY_ON");
                newFont.material.SetFloat("_UnderlayOffsetX", 0.5f);
                newFont.material.SetFloat("_UnderlayOffsetY", -0.5f);

                var list = CreatePanel();
                configPanel = list[0];

                FileInfo[] Files = new DirectoryInfo(ConfigDirectory).GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in Files)
                {
                    CreateModEntry(list[1], file);
                }

                createdConfigPanel = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error creating VampireModMenu panel");
                MelonLogger.Error($"source: {ex.Source}");
                MelonLogger.Error($"message: {ex.Message}");
                MelonLogger.Error($"inner exception: {ex.InnerException}");
                MelonLogger.Error($"data: {ex.Data}");
                MelonLogger.Error($"stack trace: {ex.StackTrace}");
                MelonLogger.Error($"target site: {ex.TargetSite}");
                MelonLogger.Error($"ex: {ex}");
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (optionsPage != null && configPanel != null && createdConfigPanel == true) configPanel.gameObject.active = optionsPage.gameObject.active;
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            DateTime lastWriteTime = File.GetLastWriteTime(ConfigFile);

            if (lastModified != lastWriteTime)
            {
                lastModified = lastWriteTime;
                LoadConfig();
            }
        }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.AddTabs))]
        class PatchOptionsPage
        {
            [HarmonyPrefix]
            static void Prefix(OptionsController __instance)
            {
                CreateMenu();
                optionsPage = __instance.transform;
            }
        }

        static List<Transform> CreatePanel()
        {
            Image tempImage;

            Canvas modCanvas = new GameObject("Mod Menu Canvas").gameObject.AddComponent<Canvas>().GetComponent<Canvas>();
            modCanvas.gameObject.AddComponent<CanvasRenderer>();
            modCanvas.gameObject.AddComponent<GraphicRaycaster>();
            modCanvas.gameObject.AddComponent<CanvasScaler>();
            modCanvas.gameObject.AddComponent<EventSystem>();
            modCanvas.gameObject.AddComponent<StandaloneInputModule>();
            modCanvas.gameObject.AddComponent<BaseInput>();

            modCanvas.name = "Mod Menu Canvas";
            modCanvas.sortingOrder = -32768;
            modCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            RectTransform modPanelRect = new GameObject("Mod Menu").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            modPanelRect.parent = modCanvas.transform;
            modPanelRect.gameObject.AddComponent<CanvasRenderer>();
            modPanelRect.localPosition = new Vector3(256, 0, 0);
            modPanelRect.sizeDelta = new Vector2(384, 768);
            modPanelRect.anchorMin = new Vector2(0, 0.5f);
            modPanelRect.anchorMax = new Vector2(0, 0.5f);

            RectTransform labelRect = new GameObject("Panel Label").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelRect.parent = modPanelRect.transform;
            labelRect.gameObject.AddComponent<CanvasRenderer>();

            tempImage = labelRect.gameObject.AddComponent<Image>();
            tempImage.color = HexToColor(config.Color);
            tempImage.raycastTarget = true;

            labelRect.localPosition = Vector3.zero;
            labelRect.pivot = new Vector2(0.5f, 1);
            labelRect.sizeDelta = new Vector2(0, 40);
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);

            RectTransform labelTextRect = new GameObject("Panel Label Text").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelTextRect.parent = labelRect.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTextRect.pivot = new Vector2(0.5f, 0.5f);
            labelTextRect.offsetMin = new Vector2(0, 0);
            labelTextRect.offsetMax = new Vector2(0, 0);
            labelTextRect.anchorMin = new Vector2(0, 0);
            labelTextRect.anchorMax = new Vector2(1, 1);

            labelText.font = newFont;
            labelText.text = "Mod Configuration";
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 30;

            RectTransform entryPanel = new GameObject("Config Entries").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            entryPanel.parent = modPanelRect.transform;
            entryPanel.gameObject.AddComponent<CanvasRenderer>();
            entryPanel.gameObject.AddComponent<Mask>();
            VerticalLayoutGroup entryPanelLayout = entryPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            entryPanelLayout.childControlWidth = true;
            entryPanelLayout.childControlHeight = true;
            entryPanelLayout.childForceExpandWidth = true;
            entryPanelLayout.childForceExpandHeight = false;

            tempImage = entryPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            entryPanel.pivot = new Vector2(0.5f, 0.5f);
            entryPanel.offsetMin = new Vector2(0, 0);
            entryPanel.offsetMax = new Vector2(0, -40);
            entryPanel.anchorMin = new Vector2(0, 0);
            entryPanel.anchorMax = new Vector2(1, 1);

            List<Transform> items = new List<Transform>();
            items.Add(modCanvas.transform);
            items.Add(entryPanel.transform);

            return items;
        }

        static void CreateModEntry(Transform ModEntryList, FileInfo ConfigFile)
        {
            Image tempImage;

            JObject json = JObject.Parse(File.ReadAllText(ConfigFile.FullName));

            string Name = (string)(json["Name"] ?? ConfigFile.Name);
            string Version = (json["Version"] != null ? $"v{json["Version"]}" : "");
            string final = string.Format("{0} {1}", Name, Version);

            RectTransform modPanel = new GameObject(final).gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            modPanel.parent = ModEntryList;
            modPanel.gameObject.AddComponent<CanvasRenderer>();

            JsonModifier jsonModifier = modPanel.gameObject.AddComponent<JsonModifier>();
            jsonModifier.filePath = ConfigFile.FullName;

            VerticalLayoutGroup modPanelLayout = modPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            modPanelLayout.childControlWidth = true;
            modPanelLayout.childControlHeight = false;
            modPanelLayout.childForceExpandWidth = true;
            modPanelLayout.childForceExpandHeight = false;

            tempImage = modPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1568628f, 0.1568628f, 0.1568628f);
            tempImage.raycastTarget = true;

            RectTransform labelRect = new GameObject("Entry Label").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelRect.parent = modPanel.transform;
            labelRect.gameObject.AddComponent<CanvasRenderer>();
            labelRect.gameObject.AddComponent<Image>().color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            labelRect.sizeDelta = new Vector2(0, 40);

            RectTransform labelTextRect = new GameObject("Panel Label Text").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelTextRect.parent = labelRect.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTextRect.pivot = new Vector2(0.5f, 0.5f);
            labelTextRect.sizeDelta = new Vector2(-20, 0);
            labelTextRect.anchorMin = new Vector2(0, 0);
            labelTextRect.anchorMax = new Vector2(1, 1);

            labelText.font = newFont;
            labelText.text = final;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            foreach (JProperty property in json.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Boolean:
                        CreateCheckbox(modPanel, property.Name, (bool)property.Value);
                        break;

                    case JTokenType.String:
                        if (property.Name.ToLower() != "name") CreateStringEntry(modPanel, property);
                        break;

                    case JTokenType.Integer:
                        CreateNumberEntry(modPanel, property, TMP_InputField.ContentType.IntegerNumber);
                        break;

                    case JTokenType.Float:
                        CreateNumberEntry(modPanel, property, TMP_InputField.ContentType.DecimalNumber);
                        break;
                }
            }
        }

        static void CreateCheckbox(Transform Parent, string ToggleName, bool DefaultValue)
        {
            Image tempImage;

            RectTransform togglePanel = new GameObject($"{ToggleName}").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            togglePanel.parent = Parent;
            togglePanel.gameObject.AddComponent<CanvasRenderer>();
            togglePanel.offsetMin = new Vector2(0, 10);

            tempImage = togglePanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = new GameObject("Button Label").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelTextRect.parent = togglePanel.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTextRect.pivot = new Vector2(0.5f, 0.5f);
            labelTextRect.sizeDelta = new Vector2(-50, 0);
            labelTextRect.anchoredPosition = new Vector2(-15, 0);
            labelTextRect.anchorMin = new Vector2(0, 0);
            labelTextRect.anchorMax = new Vector2(1, 1);

            labelText.font = newFont;
            labelText.text = $"{ToggleName}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform checkBoxRect = new GameObject("Check Box").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            checkBoxRect.parent = togglePanel.transform;
            checkBoxRect.sizeDelta = new Vector2(30, 30);
            checkBoxRect.anchoredPosition = new Vector2(-20, 0);
            checkBoxRect.anchorMin = new Vector2(1, 0.5f);
            checkBoxRect.anchorMax = new Vector2(1, 0.5f);

            tempImage = checkBoxRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            Toggle checkBoxToggle = checkBoxRect.gameObject.AddComponent<Toggle>();
            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            void ValueChanged(bool value) => jsonModifier.ModifyConfigValue(ToggleName, value);
            System.Action<bool> valueChanged = ValueChanged;
            checkBoxToggle.onValueChanged.AddListener(valueChanged);

            checkBoxToggle.isOn = DefaultValue;

            RectTransform checkBoxVisual = new GameObject("Check Box Graphic").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            tempImage = checkBoxVisual.gameObject.AddComponent<Image>();
            tempImage.raycastTarget = true;

            checkBoxToggle.targetGraphic = tempImage;
            checkBoxToggle.graphic = tempImage;

            checkBoxToggle.transition = Selectable.Transition.None;
            checkBoxToggle.toggleTransition = Toggle.ToggleTransition.None;
            checkBoxVisual.parent = checkBoxRect;
            checkBoxVisual.offsetMin = new Vector2(-20, -20);
            checkBoxVisual.offsetMax = new Vector2(0, 0);
            checkBoxVisual.anchorMin = new Vector2(0.5f, 0.5f);
            checkBoxVisual.anchorMax = new Vector2(0.5f, 0.5f);
            checkBoxVisual.localPosition = new Vector3(0, 0, 0);
        }

        static void CreateStringEntry(Transform Parent, JProperty Property)
        {
            Image tempImage;

            RectTransform togglePanel = new GameObject($"{Property.Name}").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            togglePanel.parent = Parent;
            togglePanel.gameObject.AddComponent<CanvasRenderer>();
            togglePanel.offsetMin = new Vector2(0, 10);

            tempImage = togglePanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = new GameObject("Label").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelTextRect.parent = togglePanel.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTextRect.pivot = new Vector2(0.5f, 0.5f);
            labelTextRect.sizeDelta = new Vector2(-181, 0);
            labelTextRect.anchoredPosition = new Vector2(-80.5f, 0);
            labelTextRect.anchorMin = new Vector2(0, 0);
            labelTextRect.anchorMax = new Vector2(1, 1);

            labelText.font = newFont;
            labelText.text = $"{Property.Name}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform inputRect = new GameObject("Input").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            inputRect.parent = togglePanel.transform;
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.sizeDelta = new Vector2(160, 30);
            inputRect.anchoredPosition = new Vector2(298, -20);
            inputRect.anchorMin = new Vector2(0, 1);
            inputRect.anchorMax = new Vector2(0, 1);

            tempImage = inputRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, value);
            System.Action<string> valueChanged = ValueChanged;
            input.onValueChanged.AddListener(valueChanged);

            input.text = (string)Property.Value;

            RectTransform inputTextArea = new GameObject("Text Area").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            inputTextArea.parent = inputRect.transform;
            inputTextArea.pivot = new Vector2(0.5f, 0.5f);
            inputTextArea.sizeDelta = new Vector2(-10, -9);
            inputTextArea.anchoredPosition = new Vector2(0, -0.5f);
            inputTextArea.anchorMin = new Vector2(0, 0);
            inputTextArea.anchorMax = new Vector2(1, 1);

            RectTransform valueInput = new GameObject("Input Text").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            valueInput.gameObject.AddComponent<CanvasRenderer>();
            valueInput.parent = inputTextArea.transform;
            valueInput.anchorMin = new Vector2(0, 0);
            valueInput.anchorMax = new Vector2(1, 1);
            valueInput.anchoredPosition = new Vector2(0, 0);
            valueInput.sizeDelta = new Vector2(0, 0);
            valueInput.pivot = new Vector2(0.5f, 0.5f);

            TextMeshProUGUI valueInputTextMesh = valueInput.gameObject.AddComponent<TextMeshProUGUI>();
            valueInputTextMesh.text = (string)Property.Value;
            valueInputTextMesh.overflowMode = TextOverflowModes.Ellipsis;
            valueInputTextMesh.font = newFont;
            valueInputTextMesh.color = Color.white;
            valueInputTextMesh.alignment = TextAlignmentOptions.Left;
            valueInputTextMesh.fontSize = 14;

            input.textViewport = inputTextArea;
            input.textComponent = valueInputTextMesh;
            input.fontAsset = newFont;
        }

        static void CreateNumberEntry(Transform Parent, JProperty Property, TMP_InputField.ContentType ContentType)
        {
            Image tempImage;

            RectTransform togglePanel = new GameObject($"{Property.Name}").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            togglePanel.parent = Parent;
            togglePanel.gameObject.AddComponent<CanvasRenderer>();
            togglePanel.offsetMin = new Vector2(0, 10);

            tempImage = togglePanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = new GameObject("Label").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            labelTextRect.parent = togglePanel.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelTextRect.anchorMin = new Vector2(0, 0);
            labelTextRect.anchorMax = new Vector2(1, 1);
            labelTextRect.anchoredPosition = new Vector2(-31, 0);
            labelTextRect.sizeDelta = new Vector2(-82, 0);
            labelTextRect.pivot = new Vector2(0.5f, 0.5f);

            labelText.font = newFont;
            labelText.text = $"{Property.Name}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform inputRect = new GameObject("Input").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            inputRect.parent = togglePanel.transform;
            inputRect.anchorMin = new Vector2(0, 1);
            inputRect.anchorMax = new Vector2(0, 1);
            inputRect.anchoredPosition = new Vector2(348, -20);
            inputRect.sizeDelta = new Vector2(60, 30);
            inputRect.pivot = new Vector2(0.5f, 0.5f);

            tempImage = inputRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            if (ContentType == TMP_InputField.ContentType.IntegerNumber)
            {
                void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, Int32.Parse(value));
                System.Action<string> valueChanged = ValueChanged;
                input.onValueChanged.AddListener(valueChanged);
            }
            else if(ContentType == TMP_InputField.ContentType.DecimalNumber)
            {
                void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, float.Parse(value));
                System.Action<string> valueChanged = ValueChanged;
                input.onValueChanged.AddListener(valueChanged);
            }

            input.text = (string)Property.Value;
            input.contentType = ContentType;

            RectTransform inputTextArea = new GameObject("Text Area").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            inputTextArea.parent = inputRect.transform;
            inputTextArea.pivot = new Vector2(0.5f, 0.5f);
            inputTextArea.sizeDelta = new Vector2(-10, -9);
            inputTextArea.anchoredPosition = new Vector2(0, -0.5f);
            inputTextArea.anchorMin = new Vector2(0, 0);
            inputTextArea.anchorMax = new Vector2(1, 1);

            RectTransform valueInput = new GameObject("Input Text").gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            valueInput.gameObject.AddComponent<CanvasRenderer>();
            valueInput.parent = inputTextArea.transform;
            valueInput.anchorMin = new Vector2(0, 0);
            valueInput.anchorMax = new Vector2(1, 1);
            valueInput.anchoredPosition = new Vector2(0, 0);
            valueInput.sizeDelta = new Vector2(0, 0);
            valueInput.pivot = new Vector2(0.5f, 0.5f);

            TextMeshProUGUI valueInputTextMesh = valueInput.gameObject.AddComponent<TextMeshProUGUI>();
            valueInputTextMesh.text = (string)Property.Value;
            valueInputTextMesh.overflowMode = TextOverflowModes.Ellipsis;
            valueInputTextMesh.font = newFont;
            valueInputTextMesh.color = Color.white;
            valueInputTextMesh.alignment = TextAlignmentOptions.Left;
            valueInputTextMesh.fontSize = 14;

            input.textViewport = inputTextArea;
            input.textComponent = valueInputTextMesh;
            input.fontAsset = newFont;
        }

        private static void ValidateConfig()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory)) Directory.CreateDirectory(ConfigDirectory);
                if (!File.Exists(ConfigFile)) File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(new ConfigData { }, Formatting.Indented));

                LoadConfig();
            }
            catch (System.Exception ex) { MelonLogger.Error($"Error validating Config: {ex}"); }
        }

        private static void LoadConfig()
        {
            JObject json = JObject.Parse(File.ReadAllText(ConfigFile) ?? "{}");

            config.Name = (string)json.GetValue("Name");
            config.FontFileName = (string)json.GetValue("FontFileName");
            config.Color = (string)json.GetValue("Color");

            FontDirectory = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "UserData", config.FontFileName));
        }

        public static Color HexToColor(string hexColor)
        {
            if (hexColor.IndexOf('#') != -1) hexColor = hexColor.Replace("#", "");

            int red = 0;
            int green = 0;
            int blue = 0;

            if (hexColor.Length == 6)
            {
                //#RRGGBB
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                //#RGB
                red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }

            return new Color((float) red / 255, (float) green / 255, (float) blue / 255, 1f);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class JsonModifier : MonoBehaviour
    {
        public string filePath = string.Empty;

        public void ModifyConfigValue<T>(string key, T value)
        {
            try
            {
                string file = File.ReadAllText(filePath);
                JObject json = JObject.Parse(file);

                if (!json.ContainsKey(key)) json.Add(key, JToken.FromObject(value));
                else
                {
                    System.Type type = typeof(T);
                    JToken newValue = JToken.FromObject(value);

                    if (type == typeof(string)) json[key] = newValue.ToString();
                    else if (type == typeof(int)) json[key] = newValue.ToObject<int>();
                    else if (type == typeof(bool)) json[key] = newValue.ToObject<bool>();
                    else if (type == typeof(float)) json[key] = newValue.ToObject<float>();
                    else { MelonLogger.Error($"Unsupported type '{type.FullName}'"); return; }
                }

                string finalJson = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(filePath, finalJson);
            }
            catch (System.Exception ex) { MelonLogger.Error($"Error while modifying Config: {ex}"); }

        }
    }
}
