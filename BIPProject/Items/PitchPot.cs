using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BossItemsPlus.Bases;

namespace BossItemsPlus.Items
{
    public class PitchPot : ItemBase<PitchPot>
        {
            public override string ItemName => "Pitch Pot";

            public override string ItemLangTokenName => "PITCH_POT";

            public override string ItemPickupDesc => "Every 6 seconds, fire out a seeking tarball.";

            public override string ItemFullDescription => "Every 6 seconds (-10% per stack), <style=cIsDamage>fire out 3 seeking tarballs for 200% (+100% per stack) damage. </style>";

            public override string ItemLore => "eats shit and wiggles its tarry balls";

            public override ItemTier Tier => ItemTier.Boss;
            
            public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override GameObject ItemModel => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

        public override Sprite ItemIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();

        public override void Init(ConfigFile config)
            {
                CreateConfig(config);
                CreateLang();
                CreateItem();
                Hooks();
            }

            public override void CreateConfig(ConfigFile config)
            {
            
            }

            public override ItemDisplayRuleDict CreateItemDisplayRules()
            {
                return new ItemDisplayRuleDict();
            }

            public override void Hooks()
            {

            }

        }
    }