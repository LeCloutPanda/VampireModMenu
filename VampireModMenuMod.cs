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
using Il2CppSystem.Collections;

namespace VampireModMenu
{

    public static class ModInfo
    {
        public const string Name = "Vampire Mod Menu";
        public const string Description = "Adds a configuration screen for mods.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.0.19";
        public const string DownloadLink = "";
    }

    public class VampireModMenuMod : MelonMod
    {
        static readonly string ConfigDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");

        static TMP_FontAsset newFont;
        static bool createdConfigPanel = true;
        static Transform configPanel = null;
        static Transform optionsPage = null;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

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

            Font f = new Font();
            Font.Internal_CreateFontFromPath(f, "C:\\Users\\Lucas\\Documents\\Dev\\Unity\\Playground\\Assets\\AssetBundles\\LiberationSans.ttf");
            f.hideFlags = HideFlags.DontUnloadUnusedAsset;
            newFont = TMP_FontAsset.CreateFontAsset(f);
            newFont.material.EnableKeyword("UNDERLAY_ON");
            newFont.material.SetFloat("_UnderlayOffsetX", 0.5f);
            newFont.material.SetFloat("_UnderlayOffsetY", -0.5f);

            var list = CreatePanel();
            configPanel = list[0];

            FileInfo[] Files = new DirectoryInfo(ConfigDirectory).GetFiles("*"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                CreateModEntry(list[1], file.Name);
            }

            createdConfigPanel = true;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (optionsPage != null && configPanel != null && createdConfigPanel == true) configPanel.gameObject.active = optionsPage.gameObject.active;
        }

        [HarmonyPatch(typeof(OptionsPage), nameof(OptionsPage.OnEnable))]
        class PatchOptionsPage
        {
            [HarmonyPrefix]
            static void Prefix(OptionsPage __instance)
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
            tempImage.color = new Color(0.9528302f, 0.3370862f, 0.6591695f);
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

        static void CreateModEntry(Transform ModEntryList, string ConfigName)
        {
            Image tempImage;

            string filePath = Path.Combine(ConfigDirectory, $"{ConfigName}");
            JObject json = JObject.Parse(File.ReadAllText(filePath));

            string Name = (string)(json["Name"] ?? ConfigName);
            string Version = (json["Version"] != null ? $"v{json["Version"]}" : "");
            string final = string.Format("{0} {1}", Name, Version);

            RectTransform modPanel = new GameObject(final).gameObject.AddComponent<RectTransform>().GetComponent<RectTransform>();
            modPanel.parent = ModEntryList;
            modPanel.gameObject.AddComponent<CanvasRenderer>();

            JsonModifier jsonModifier = modPanel.gameObject.AddComponent<JsonModifier>();
            jsonModifier.filePath = filePath;

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
                if (property.Value.Type == JTokenType.Boolean) CreateCheckbox(modPanel, property.Name, (bool)property.Value);
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
            //checkBoxToggle.onValueChanged.AddListener(Test);

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
                    else { MelonLogger.Error($"Unsupported type '{type.FullName}'"); return; }
                }

                string finalJson = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(filePath, finalJson);
            }
            catch (System.Exception ex) { MelonLogger.Error($"Error while modifying Config: {ex}"); }

        }
    }
}
