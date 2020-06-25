﻿using DecorationsMod.Controllers;
using DecorationsMod.Fixers;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DecorationsMod.NewItems
{
    public class ReactorLamp : DecorationItem
    {
        public ReactorLamp() // Feeds abstract class
        {
            this.ClassID = "ReactorLamp";
            this.PrefabFileName = DecorationItem.DefaultResourcePath + this.ClassID;

            this.GameObject = AssetsHelper.Assets.LoadAsset<GameObject>("nuclearreactorrod_white");

            this.TechType = SMLHelper.V2.Handlers.TechTypeHandler.AddTechType(this.ClassID,
                                                        LanguageHelper.GetFriendlyWord("ReactorLampName"),
                                                        LanguageHelper.GetFriendlyWord("ReactorLampDescription"),
                                                        true);

            CrafterLogicFixer.ReactorLamp = this.TechType;
            KnownTechFixer.AddedNotifications.Add((int)this.TechType, false);

            this.IsHabitatBuilder = true;

#if BELOWZERO
            this.Recipe = new SMLHelper.V2.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[4]
                    {
                        new Ingredient(TechType.ComputerChip, 1),
                        new Ingredient(TechType.Glass, 1),
                        new Ingredient(TechType.Titanium, 1),
                        new Ingredient(TechType.Diamond, 1)
                    }),
            };
#else
            this.Recipe = new SMLHelper.V2.Crafting.TechData()
            {
                craftAmount = 1,
                Ingredients = new List<SMLHelper.V2.Crafting.Ingredient>(new SMLHelper.V2.Crafting.Ingredient[4]
                    {
                        new SMLHelper.V2.Crafting.Ingredient(TechType.ComputerChip, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Glass, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Titanium, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Diamond, 1)
                        
                    }),
            };
#endif
        }

        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {
                GameObject aquarium = Resources.Load<GameObject>("Submarine/Build/Aquarium");

                // Retrieve model node
                GameObject model = this.GameObject.FindChild("model");
                
                // Move model
                model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y - 0.04f, model.transform.localPosition.z + 0.03f);

                // Disable light at start
                var reactorRodLight = this.GameObject.GetComponentInChildren<Light>();
                reactorRodLight.intensity = 1.0f;
                reactorRodLight.range = 10.0f;
                reactorRodLight.color = Color.white;
                reactorRodLight.enabled = false;

                // Add prefab identifier
                var prefabId = this.GameObject.AddComponent<PrefabIdentifier>();
                prefabId.ClassId = this.ClassID;

                // Add large world entity
                var lwe = this.GameObject.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

                // Add tech tag
                var techTag = this.GameObject.AddComponent<TechTag>();
                techTag.type = this.TechType;

                // Add box collider
                var collider = this.GameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.18f, 0.36f, 0.18f);
                collider.center = new Vector3(collider.center.x, collider.center.y + 0.22f, collider.center.z);

                // Get glass material
                Material glass = null;
                Renderer[] aRenderers = aquarium.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer aRenderer in aRenderers)
                {
                    foreach (Material aMaterial in aRenderer.materials)
                    {
                        if (aMaterial.name.StartsWith("Aquarium_glass", StringComparison.OrdinalIgnoreCase))
                        {
                            glass = aMaterial;
                            break;
                        }
                    }
                    if (glass != null)
                        break;
                }

                // Set proper shaders (for crafting animation)
                Shader shader = Shader.Find("MarmosetUBER");
                Texture normalTexture = AssetsHelper.Assets.LoadAsset<Texture>("nuclear_reactor_rod_normal");
                Texture specTexture = AssetsHelper.Assets.LoadAsset<Texture>("nuclear_reactor_rod_spec");
                Texture illumTexture = AssetsHelper.Assets.LoadAsset<Texture>("nuclear_reactor_rod_illum_white");

                List<Renderer> renderers = new List<Renderer>();
                this.GameObject.GetComponentsInChildren<Renderer>(renderers);
                foreach (Renderer renderer in renderers)
                {
                    if (string.Compare(renderer.name, "nuclear_reactor_rod_mesh", true, CultureInfo.InvariantCulture) == 0)
                    {
                        // Associate MarmosetUBER shader
                        renderer.sharedMaterial.shader = shader;
                        renderer.material.shader = shader;

                        // Update normal map
                        renderer.sharedMaterial.SetTexture("_BumpMap", normalTexture);
                        renderer.material.SetTexture("_BumpMap", normalTexture);

                        // Update spec map
                        renderer.sharedMaterial.SetTexture("_SpecTex", specTexture);
                        renderer.material.SetTexture("_SpecTex", specTexture);

                        // Update emission map
                        renderer.sharedMaterial.SetTexture("_Illum", illumTexture);
                        renderer.material.SetTexture("_Illum", illumTexture);

                        // Increase emission map strength
                        renderer.sharedMaterial.SetColor("_GlowColor", Color.white);
                        renderer.sharedMaterial.SetFloat("_GlowStrength", 1.0f);
                        renderer.material.SetColor("_GlowColor", Color.white);
                        renderer.material.SetFloat("_GlowStrength", 1.0f);

                        // Enable emission
                        renderer.sharedMaterial.EnableKeyword("MARMO_NORMALMAP");
                        renderer.sharedMaterial.EnableKeyword("MARMO_EMISSION");
                        renderer.sharedMaterial.EnableKeyword("MARMO_SPECMAP");
                        renderer.sharedMaterial.EnableKeyword("_ZWRITE_ON"); // Enable Z write
                        renderer.material.EnableKeyword("MARMO_NORMALMAP");
                        renderer.material.EnableKeyword("MARMO_EMISSION");
                        renderer.material.EnableKeyword("MARMO_SPECMAP");
                        renderer.material.EnableKeyword("_ZWRITE_ON"); // Enable Z write
                    }
                    else if (string.Compare(renderer.name, "nuclear_reactor_rod_glass", true, CultureInfo.InvariantCulture) == 0 && glass != null)
                        renderer.material = glass;
                }

                // Add sky applier
#if BELOWZERO
                BaseModuleLighting bml = this.GameObject.AddComponent<BaseModuleLighting>();
                var skyapplier = this.GameObject.GetComponent<SkyApplier>();
                if (skyapplier == null)
                    skyapplier = this.GameObject.GetComponentInChildren<SkyApplier>();
                if (skyapplier == null)
                    skyapplier = this.GameObject.AddComponent<SkyApplier>();
                skyapplier.renderers = this.GameObject.GetComponentsInChildren<Renderer>();
                skyapplier.anchorSky = Skies.Auto;
#else
                var skyapplier = this.GameObject.GetComponent<SkyApplier>();
                if (skyapplier != null)
                    GameObject.Destroy(skyapplier);
                var skyappliers = this.GameObject.GetComponentsInChildren<SkyApplier>();
                if (skyappliers != null)
                    foreach (SkyApplier sa in skyappliers)
                        GameObject.Destroy(sa);
                PrefabsHelper.SetDefaultSkyApplier(this.GameObject, null, Skies.Auto, false, true);
#endif

                // Add contructable
                var constructible = this.GameObject.AddComponent<Lamp_C>();
                constructible.allowedInBase = true;
                constructible.allowedInSub = true;
                constructible.allowedOutside = true;
                constructible.allowedOnCeiling = true;
                constructible.allowedOnGround = true;
                constructible.allowedOnConstructables = true;
                constructible.allowedOnWall = true;
#if BELOWZERO
                constructible.allowedUnderwater = true;
#endif
                constructible.controlModelState = true;
                constructible.deconstructionAllowed = true;
                constructible.rotationEnabled = false;
                constructible.model = model;
                constructible.techType = this.TechType;
                constructible.enabled = true;

                // Add constructable bounds
                var bounds = this.GameObject.AddComponent<ConstructableBounds>();
                //bounds.bounds.position = new Vector3(bounds.bounds.position.x, bounds.bounds.position.y + 0.003f, bounds.bounds.position.z);

                // Add lamp brightness controler
                var lampBrightness = this.GameObject.AddComponent<ReactorLampBrightness>();
                
                // Associate recipe to the new TechType
                SMLHelper.V2.Handlers.CraftDataHandler.SetTechData(this.TechType, this.Recipe);

                // Add new TechType to the buildables
                SMLHelper.V2.Handlers.CraftDataHandler.AddBuildable(this.TechType);
                SMLHelper.V2.Handlers.CraftDataHandler.AddToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorLight, this.TechType, TechType.Spotlight);

                // Set the buildable prefab
                SMLHelper.V2.Handlers.PrefabHandler.RegisterPrefab(this);
                
                // Set the custom sprite
                SMLHelper.V2.Handlers.SpriteHandler.RegisterSprite(this.TechType, AssetsHelper.Assets.LoadAsset<Sprite>("reactorrod_white"));

                this.IsRegistered = true;
            }
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);
            
            prefab.name = this.ClassID;
            // Scale prefab
            prefab.transform.localScale *= 1.5f;

            return prefab;
        }
    }
}
