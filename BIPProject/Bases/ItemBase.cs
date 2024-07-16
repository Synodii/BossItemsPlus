using BepInEx.Configuration;
using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using On.RoR2.Items;
using System.Linq;
using HarmonyLib;

namespace BossItemsPlus.Bases
{

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }

        public string ConfigItemName
        {
            get
            {
                return ItemName.Replace("\'", "");
            }
        }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public virtual ItemTierDef ModdedTierDef { get; } = null;

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; } = new ItemTag[] { };

        public virtual GameObject ItemModel { get; } = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        public virtual Sprite ItemIcon { get; } = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public virtual bool CanRemove { get; } = true;
        public virtual bool Hidden { get; } = false;
        public bool AIBlacklisted { get; internal set; }
        public bool PrinterBlacklisted { get; internal set; }

        public virtual string[] CorruptsItem { get; set; } = null;

        public ItemDef ItemDef;

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = Hidden;
            ItemDef.canRemove = CanRemove;
            //ItemDef.tier = Tier;
            if (ModdedTierDef == null)
                ItemDef.deprecatedTier = Tier;
            else
                ItemDef._itemTierDef = ModdedTierDef;
            ItemDef.tags = ItemTags;
            //ItemTag.WorldUnique
            var itemDisplayRuleDict = CreateItemDisplayRules();
            ItemAPI.Add(new CustomItem(ItemDef, itemDisplayRuleDict));
        }



        public abstract void Hooks();

        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        internal static void RegisterVoidPairings(ContagiousItemManager.orig_Init orig)
        {
            var voidTiers = new ItemTier[]{
                ItemTier.VoidBoss,
                ItemTier.VoidTier1,
                ItemTier.VoidTier2,
                ItemTier.VoidTier3};

            foreach (KeyValuePair<ItemBase, bool> itemPair in Main.ItemStatusDictionary)
            {
                if (itemPair.Value == true)
                {
                    var item = itemPair.Key;

                    if (item.ItemDef && voidTiers.Any(x => item.ItemDef.tier == x))
                    {
                        for (int i = 0; i < item.CorruptsItem.Count(); i++)
                        {
                            var itemToCorrupt = ItemCatalog.itemDefs.Where(x => x.nameToken == item.CorruptsItem[i]).FirstOrDefault();
                            if (!itemToCorrupt)
                            {
                                Main.ModLogger.LogError($"Tried to add {item.ItemName} in a Void item tier but no relationship was set for what it corrupts or could not be found. Aborting!");
                                continue;
                            }

                            var pair = new ItemDef.Pair[]
                            {
                            new ItemDef.Pair
                            {
                                itemDef1 = itemToCorrupt,
                                itemDef2 = item.ItemDef,
                            }
                            };

                            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pair);
                        }
                    }
                }
            }
            orig();
        }
    }


}