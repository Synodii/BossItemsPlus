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
using UnityEngine.Networking;

namespace BossItemsPlus.Items
{
    public class DefensiveDigestion : ItemBase<DefensiveDigestion>
    {
        public override string ItemName => "Defensive Digestion";

        public override string ItemLangTokenName => "BG_GOOPER";

        public override string ItemPickupDesc => "Gain a chance upon taking damage to fire acid balls that leave damaging pools of beetlejuice.";

        public override string ItemFullDescription => "15% <style=cStack>(+7% per stack)</style> chance to fire 3 acid balls that leave pools that applies beetlejuice and deals a constant (TBD)% damage.";

        public override string ItemLore => "eats shit and wiggles its beetley balls";

        public override ItemTier Tier => ItemTier.Boss;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static float projectileDamage;
        //public static float poolDamage;
        public static int baseprojectileCount;
        public static float stackChance;
        public static float baseChance;
        public static float yawSpread = 20f;
        private Ray aimRay;
        private float duration;
        public static float arcAngle = 5f;


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            CreateProjectile();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            projectileDamage = config.Bind<float>("Item: " + ItemName, "Projectile Damage", 3f, "How much damage does the acid ball direct hit do?").Value;
            //poolDamage = config.Bind<float>("Item: " + ItemName, "Pool Damage", 2f, "How much damage does the acid pool do per second?").Value;
            //stackprojectileCount = config.Bind<int>("Item: " + ItemName, "Additional Projectile Count per Stack", 1, "How many additional acid balls will be shot out per additional stacks?").Value;
            baseprojectileCount = config.Bind<int>("Item: " + ItemName, "Base Projectile Count", 3, "How many acid balls will be shot out with one stack?").Value;
            stackChance = config.Bind<float>("Item: " + ItemName, "Chance per stack", .07f, "Chance per additional stack to fire acid balls when hurt.").Value;
            baseChance = config.Bind<float>("Item: " + ItemName, "Base chance", .15f, "Chance to fire acid balls when hurt.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += FireAcidBall;
        }

        public static GameObject AcidProjectile;
        private void CreateProjectile()
        {
            AcidProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Missile/MissileProjectile.prefab").WaitForCompletion(), "AcidProjectile", true);
            var impactExplosion = AcidProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenSpit.prefab").WaitForCompletion();


            var projectileController = AcidProjectile.GetComponent<ProjectileController>();
            projectileController.startSound = "Play_item_void_critGlasses";
            projectileController.allowPrediction = false;

            PrefabAPI.RegisterNetworkPrefab(AcidProjectile);
            //ProjectileAPI.Add(Projectile);
            ContentAddition.AddProjectile(AcidProjectile);
        }
        private void FireAcidBall(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var master = self.gameObject.GetComponent<CharacterMaster>();
            var body = self.gameObject.GetComponent<CharacterBody>();

            if (master)
            {
                var inventoryCount = GetCount(master);
                if (inventoryCount > 0)
                {
                    if (Util.CheckRoll((baseChance + stackChance * (inventoryCount - 1)), master))
                    {
                        Ray ray = this.aimRay;
                        ray.origin = this.aimRay.GetPoint(6f);
                        RaycastHit raycastHit;
                        var footPosition = master.transform.position;
                        var fpi = new FireProjectileInfo
                        {
                            owner = self.gameObject,
                            force = 1f,
                            damage = projectileDamage,
                            position = footPosition,
                            rotation = Util.QuaternionSafeLookRotation(body.corePosition),
                            crit = false,
                            projectilePrefab = AcidProjectile,
                            damageColorIndex = DamageColorIndex.Default,
                            damageTypeOverride = DamageType.Generic
                       };
                        for (int i = 0; i < baseprojectileCount; i++)
                        {
                            ProjectileManager.instance.FireProjectile(fpi);
                        }
                    }
                }
            }
        }
    }
}