using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEngine;
using Il2CppSystem.Collections.Generic;
using System;
using System.Globalization;

namespace VampireModMenu
{
    public class ConfigData
    {
        public string Name = ModInfo.Name;
        public string Color = "#74b884";
        public string FontFileName = "LiberationSans.ttf";
        public float ScrollSensitivity = 10;
    }

    public static class ModInfo
    {
        public const string Name = "Vampire Mod Menu";
        public const string Description = "Adds a configuration screen for mods.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.1";
        public const string DownloadLink = "https://github.com/LeCloutPanda/VampireModMenu";
    }

    public class VampireModMenuMod : MelonMod
    {
        static ConfigData config = new ConfigData();

        static readonly string ConfigDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");
        static readonly string ConfigFile = Path.Combine(ConfigDirectory, "VampireModMenu.json");
        static string FontDirectory;

        static TMP_FontAsset newFont;
        static bool createdConfigPanel = false;
        static Transform configPanel = null;

        static DateTime lastModified;

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
            createdConfigPanel = false;
        }

        static void CreateMenu()
        {
            if (createdConfigPanel) return;

            MelonLogger.Msg("Creating Mod Configuration Panel");
            try
            {
                try
                {
                    Font f = new Font();
                    Font.Internal_CreateFontFromPath(f, FontDirectory);
                    f.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    newFont = TMP_FontAsset.CreateFontAsset(f);
                    newFont.material.EnableKeyword("UNDERLAY_ON");
                    newFont.material.SetFloat("_UnderlayOffsetX", 0.5f);
                    newFont.material.SetFloat("_UnderlayOffsetY", -0.5f);
                }
                catch
                {
                    MelonLogger.Error("Invalid Font in config file");
                }

                var list = CreatePanel();
                configPanel = list[0];

                FileInfo[] Files = new DirectoryInfo(ConfigDirectory).GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in Files)
                {
                    CreateModEntry(list[1], file);
                }

                configPanel.gameObject.SetActive(false);
                configPanel.gameObject.SetActive(true);

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

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            DateTime lastWriteTime = File.GetLastWriteTime(ConfigFile);

            if (lastModified != lastWriteTime)
            {
                lastModified = lastWriteTime;
                LoadConfig();
            }
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                if (createdConfigPanel == false) CreateMenu();
                else configPanel.gameObject.SetActive(!configPanel.gameObject.active);
            }
        }

        static RectTransform CreateRect(string name, Transform parent, object localPosition, object anchoredPosition, object pivot, object sizeDelta, object anchorMin, object anchorMax, object offsetMin, object offsetMax)
        {
            RectTransform newRect = new GameObject(name).gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            if (parent != null) newRect.parent = parent.transform;
            if (localPosition != null) newRect.localPosition = (Vector2)localPosition;
            if (anchoredPosition != null) newRect.anchoredPosition = (Vector2)anchoredPosition;
            if (pivot != null) newRect.pivot = (Vector2)pivot;
            if (sizeDelta != null) newRect.sizeDelta = (Vector2)sizeDelta;
            if (anchorMin != null) newRect.anchorMin = (Vector2)anchorMin;
            if (anchorMax != null) newRect.anchorMax = (Vector2)anchorMax;
            if (offsetMin != null) newRect.offsetMin = (Vector2)offsetMin;
            if (offsetMax != null) newRect.offsetMax = (Vector2)offsetMax;

            return newRect;
        }

        static ScrollRect scrollRectComp;
        static Image panelImage;

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

            RectTransform modPanelRect = CreateRect("Mod Menu", modCanvas.transform, new Vector2(256, 0), null, null, new Vector2(384, 768), new Vector2(0, 0.5f), new Vector2(0, 0.5f), null, null);
            modPanelRect.gameObject.AddComponent<CanvasRenderer>();

            RectTransform labelRect = CreateRect("Panel Label", modPanelRect.transform, Vector2.zero, null, new Vector2(0.5f, 1), new Vector2(0, 40), new Vector2(0, 1), new Vector2(1, 1), null, null);
            labelRect.gameObject.AddComponent<CanvasRenderer>();
            panelImage = labelRect.gameObject.AddComponent<Image>();
            panelImage.color = HexToColor(config.Color);
            panelImage.raycastTarget = true;

            RectTransform labelTextRect = CreateRect("Panel Label Text", labelRect.transform, new Vector2(0, -20), null, new Vector2(0.5f, 0.5f), new Vector2(-20, 0), new Vector2(0, 0), new Vector2(1, 1), null, null);
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelText.font = newFont;
            labelText.text = $"Mod Configuration {ModInfo.Version}";
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 30;

            RectTransform scrollRect = CreateRect("Scroll Rect", modPanelRect.transform, new Vector2(0, -20), new Vector2(0, -20), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(0, 0), new Vector2(1, 1), null, null);
            scrollRect.gameObject.AddComponent<CanvasRenderer>();
            scrollRect.gameObject.AddComponent<Image>().color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            scrollRect.gameObject.AddComponent<Mask>();

            RectTransform entryPanel = CreateRect("Config Entries", scrollRect.transform, null, null, new Vector2(0.5f, 1.0f), null, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, -40));
            entryPanel.gameObject.AddComponent<CanvasRenderer>();
            entryPanel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            VerticalLayoutGroup entryPanelLayout = entryPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            entryPanelLayout.childControlWidth = true;
            entryPanelLayout.childControlHeight = true;
            entryPanelLayout.childForceExpandWidth = true;
            entryPanelLayout.childForceExpandHeight = false;
            tempImage = entryPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;
            scrollRectComp = scrollRect.gameObject.AddComponent<ScrollRect>();
            scrollRectComp.movementType = ScrollRect.MovementType.Clamped;
            scrollRectComp.content = entryPanel;
            scrollRectComp.inertia = false;
            scrollRectComp.scrollSensitivity = config.ScrollSensitivity;
            scrollRectComp.horizontal = false;

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

            RectTransform modPanel = CreateRect(final, ModEntryList, null, null, null, null, null, null, null, null);
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

            RectTransform labelRect = CreateRect("Entry Label", modPanel.transform, null, null, null, new Vector2(0, 40), null, null, null, null);
            labelRect.gameObject.AddComponent<CanvasRenderer>();
            labelRect.gameObject.AddComponent<Image>().color = new Color(0.09803922f, 0.09803922f, 0.09803922f);

            RectTransform labelTextRect = CreateRect("Panel Label Text", labelRect.transform, null, null, new Vector2(0.5f, 0.5f), new Vector2(-20, 0), new Vector2(0, 0), new Vector2(1, 1), null, null);
            labelTextRect.parent = labelRect.transform;
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();

            labelText.font = newFont;
            labelText.text = final;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            foreach (JProperty property in json.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Boolean:
                        CreateCheckbox(modPanel, property);
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

        static void CreateCheckbox(Transform Parent, JProperty Property)
        {
            Image tempImage;

            RectTransform propertyPanel = CreateRect($"{Property.Name}", Parent, null, null, null, new Vector2(0, 40), null, null, null, null);
            propertyPanel.gameObject.AddComponent<CanvasRenderer>();
            tempImage = propertyPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = CreateRect("Button Label", propertyPanel.transform, null, new Vector2(-15, 0), new Vector2(0.5f, 0.5f), new Vector2(-50, 0), new Vector2(0, 0), new Vector2(1, 1), null, null);
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();

            labelText.font = newFont;
            labelText.text = $"{Property.Name}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform checkBoxRect = CreateRect("Check Box", propertyPanel.transform, null, new Vector2(-20, 0), null, new Vector2(30, 30), new Vector2(1, 0.5f), new Vector2(1, 0.5f), null, null);
            tempImage = checkBoxRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            RectTransform checkBoxVisual = CreateRect("Check Box Graphic", checkBoxRect.transform, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(20, 20), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), null, null);
            tempImage = checkBoxVisual.gameObject.AddComponent<Image>();
            tempImage.raycastTarget = true;

            Toggle checkBoxToggle = checkBoxRect.gameObject.AddComponent<Toggle>();
            checkBoxToggle.navigation.mode = Navigation.Mode.None;
            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            void ValueChanged(bool value) => jsonModifier.ModifyConfigValue(Property.Name, value);
            System.Action<bool> valueChanged = ValueChanged;
            checkBoxToggle.onValueChanged.AddListener(valueChanged);

            checkBoxToggle.isOn = (bool)Property.Value;
            checkBoxToggle.transition = Selectable.Transition.None;
            checkBoxToggle.toggleTransition = Toggle.ToggleTransition.None;
            checkBoxToggle.targetGraphic = tempImage;
            checkBoxToggle.graphic = tempImage;
        }

        static void CreateStringEntry(Transform Parent, JProperty Property)
        {
            Image tempImage;

            RectTransform propertyPanel = CreateRect($"{Property.Name}", Parent, null, null, null, new Vector2(0, 40), null, null, null, null);
            propertyPanel.gameObject.AddComponent<CanvasRenderer>();
            tempImage = propertyPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = CreateRect("Label", propertyPanel.transform, null, new Vector2(-80.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(-181, 0), new Vector2(0, 0), new Vector2(1, 1), null, null);
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelText.font = newFont;
            labelText.text = $"{Property.Name}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform inputRect = CreateRect("Input", propertyPanel.transform, null, new Vector2(298, -20), new Vector2(0.5f, 0.5f), new Vector2(160, 30), new Vector2(0, 1), new Vector2(0, 1), null, null);
            tempImage = inputRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
            input.navigation.mode = Navigation.Mode.None;

            input.text = (string)Property.Value;
            input.selectionColor = HexToColor("#A8CEFF");
            input.caretBlinkRate = 1;
            input.caretWidth = 1;

            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, value);
            System.Action<string> valueChanged = ValueChanged;
            input.onValueChanged.AddListener(valueChanged);

            RectTransform inputTextArea = CreateRect("Text Area", inputRect.transform, null, new Vector2(0, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10, -9), new Vector2(0, 0), new Vector2(1, 1), null, null);

            RectTransform valueInput = CreateRect("Input Text", inputTextArea.transform, null, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0), new Vector2(1, 1), null, null);
            valueInput.gameObject.AddComponent<CanvasRenderer>();
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

            RectTransform propertyPanel = CreateRect($"{Property.Name}", Parent, null, null, null, new Vector2(0, 40), null, null, null, null);
            propertyPanel.gameObject.AddComponent<CanvasRenderer>();
            tempImage = propertyPanel.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0.1960784f);
            tempImage.raycastTarget = true;

            RectTransform labelTextRect = CreateRect("Label", propertyPanel.transform, null, new Vector2(-80.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(-181, 0), new Vector2(0, 0), new Vector2(1, 1), null, null);
            labelTextRect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI labelText = labelTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            labelText.font = newFont;
            labelText.text = $"{Property.Name}";
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.fontSize = 20;

            RectTransform inputRect = CreateRect("Input", propertyPanel.transform, null, new Vector2(348, -20), new Vector2(0.5f, 0.5f), new Vector2(60, 30), new Vector2(0, 1), new Vector2(0, 1), null, null);
            tempImage = inputRect.gameObject.AddComponent<Image>();
            tempImage.color = new Color(0.09803922f, 0.09803922f, 0.09803922f);
            tempImage.raycastTarget = true;

            TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
            input.navigation.mode = Navigation.Mode.None;

            input.text = (string)Property.Value;
            input.contentType = ContentType;
            input.selectionColor = HexToColor("#A8CEFF");
            input.caretBlinkRate = 1;
            input.caretWidth = 1;

            JsonModifier jsonModifier = Parent.GetComponent<JsonModifier>();

            if (ContentType == TMP_InputField.ContentType.IntegerNumber)
            {
                void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, Int32.Parse(value));
                System.Action<string> valueChanged = ValueChanged;
                input.onValueChanged.AddListener(valueChanged);
            }
            else if (ContentType == TMP_InputField.ContentType.DecimalNumber)
            {
                void ValueChanged(string value) => jsonModifier.ModifyConfigValue(Property.Name, float.Parse(value));
                System.Action<string> valueChanged = ValueChanged;
                input.onValueChanged.AddListener(valueChanged);
            }


            RectTransform inputTextArea = CreateRect("Text Area", inputRect.transform, null, new Vector2(0, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10, -9), new Vector2(0, 0), new Vector2(1, 1), null, null);

            RectTransform valueInput = CreateRect("Input Text", inputTextArea.transform, null, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0), new Vector2(1, 1), null, null);
            valueInput.gameObject.AddComponent<CanvasRenderer>();
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
            config.Color = (string)json.GetValue("Color");
            config.FontFileName = (string)json.GetValue("FontFileName");
            config.ScrollSensitivity = (float)json.GetValue("ScrollSensitivity");

            // Change the values live
            if (scrollRectComp != null) scrollRectComp.scrollSensitivity = config.ScrollSensitivity;
            if (panelImage != null) panelImage.color = HexToColor(config.Color);

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

            return new Color((float)red / 255, (float)green / 255, (float)blue / 255, 1f);
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
