using BepInEx;
using R2API;
using R2API.Networking;
using HG;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using R2API.Utils;
using System.Linq;
using BossItemsPlus.Items;
using System.Collections.Generic;
using RoR2.ContentManagement;
using BossItemsPlus.Utils;
using BossItemsPlus.Bases;
using System.Drawing;

namespace BossItemsPlus
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    //[BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    //[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    //  [BepInDependency(R2API.ColorsAPI.PluginGUID)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    //[BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]

    //[R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(ColorsAPI))]//nameof(BuffAPI), nameof(ResourcesAPI), nameof(EffectAPI), nameof(ProjectileAPI), nameof(ArtifactAPI), nameof(LoadoutAPI),   
    // nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI),
    // nameof(NetworkingAPI), nameof(DirectorAPI), nameof(RecalculateStatsAPI), nameof(UnlockableAPI), nameof(EliteAPI),
    // nameof(CommandHelper), nameof(DamageAPI))]


    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Main : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string ModGuid = "com.Synodii.BossItemsPlus"; //Our Package Name
        public const string ModName = "BossItemsPlus";
        public const string ModVer = "0.0.1";


        internal static BepInEx.Logging.ManualLogSource ModLogger;

        internal static BepInEx.Configuration.ConfigFile MainConfig;

        //We need our item definition to persist through our functions, and therefore make it a class field.
        //private static ItemDef myItemDef;

        //The Awake() method is run at the very start when the game is initialized.
        public static AssetBundle MainAssets;

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"fake ror/hopoo games/deferred/hgstandard", "shaders/deferred/hgstandard"},
            {"fake ror/hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"fake ror/hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
            {"fake ror/hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
            {"fake ror/hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
            {"fake ror/hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
        };

        //public List<CoreModule> CoreModules = new List<CoreModule>();
        //public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public List<BuffBase> Buffs = new List<BuffBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        // public List<InteractableBase> Interactables = new List<InteractableBase>();
        // public List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static HashSet<ItemDef> BlacklistedFromPrinter = new HashSet<ItemDef>();

        //public static ExpansionDef AetheriumExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();


        // For modders that seek to know whether or not one of the items or equipment are enabled for use in...I dunno, adding grip to Blaster Sword?
        //public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new Dictionary<ArtifactBase, bool>();
        //public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        //public static Dictionary<InteractableBase, bool> InteractableStatusDictionary = new Dictionary<InteractableBase, bool>();
        //public static Dictionary<SurvivorBase, bool> SurvivorStatusDictionary = new Dictionary<SurvivorBase, bool>();

        //public static ColorCatalog.ColorIndex TempCoreLight = ColorsAPI.RegisterColor(Color.cyan);//new Color32(21, 99, 58, 255));//ColorCatalogUtils.RegisterColor(new Color32(21, 99, 58, 255));
        //public static ColorCatalog.ColorIndex TempCoreDark = ColorsAPI.RegisterColor(Color.cyan); //new Color32(1, 126, 62, 255)); //ColorCatalogUtils.RegisterColor(new Color32(1, 126, 62, 255));
        public void Awake()
        {
            ModLogger = this.Logger;
            MainConfig = Config;
        }

        private void Start()
        {

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BossItemsPlus.morebossitems_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }


            var disableBuffs = Config.Bind<bool>("Buffs", "Disable All Standalone Buffs?", false, "Do you wish to disable every standalone buff in BossItemsPlus?").Value;
            if (!disableBuffs)
            {
                //Standalone Buff Initialization
                var BuffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

                ModLogger.LogInfo("--------------BUFFS---------------------");

                foreach (var buffType in BuffTypes)
                {
                    BuffBase buff = (BuffBase)System.Activator.CreateInstance(buffType);
                    if (ValidateBuff(buff, Buffs))
                    {
                        buff.Init(Config);

                        ModLogger.LogInfo("Buff: " + buff.BuffName + " Initialized!");
                    }
                }
            }

            var disableItems = Config.Bind<bool>("Items", "Disable All Items?", false, "Do you wish to disable every item in BossItemsPlus?");
            if (!disableItems.Value)
            {
                //Item Initialization
                var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

                ModLogger.LogInfo("----------------------ITEMS--------------------");

                foreach (var itemType in ItemTypes)
                {
                    ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                    if (ValidateItem(item, Items))
                    {
                        item.Init(Config);

                        ModLogger.LogInfo("Item: " + item.ItemName + " Initialized!");
                    }
                }

                //IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ItemBase.BlacklistFromPrinter;
                On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;
            }
            var disableEquipment = Config.Bind<bool>("Equipment", "Disable All Equipment?", false, "Do you wish to disable every equipment in BossItemsPlus?");
            if (!disableEquipment.Value)
            {
                //Equipment Initialization
                var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

                ModLogger.LogInfo("-----------------EQUIPMENT---------------------");

                foreach (var equipmentType in EquipmentTypes)
                {
                    EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                    if (ValidateEquipment(equipment, Equipments))
                    {
                        equipment.Init(Config);

                        ModLogger.LogInfo("Equipment: " + equipment.EquipmentName + " Initialized!");
                    }
                }
            }
        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ConfigItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ConfigItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            var printerBlacklist = Config.Bind<bool>("Item: " + item.ConfigItemName, "Blacklist Item from Printers?", false, "Should the printers be able to print this item?").Value;
            var requireUnlock = Config.Bind<bool>("Item: " + item.ConfigItemName, "Require Unlock", true, "Should we require this item to be unlocked before it appears in runs? (Will only affect items with associated unlockables.)").Value;

            ItemStatusDictionary.Add(item, enabled);

            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
                if (printerBlacklist)
                {
                    item.PrinterBlacklisted = true;
                }

                //item.RequireUnlock = requireUnlock;
            }
            return enabled;
        }

        public bool ValidateBuff(BuffBase buff, List<BuffBase> buffList)
        {
            var enabled = Config.Bind<bool>("Buff: " + buff.BuffName, "Enable Buff?", true, "Should this buff be registered for use in the game?").Value;

            BuffStatusDictionary.Add(buff, enabled);

            if (enabled)
            {
                buffList.Add(buff);
            }
            return enabled;
        }

        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value;

            EquipmentStatusDictionary.Add(equipment, enabled);

            if (enabled)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }
    }
}