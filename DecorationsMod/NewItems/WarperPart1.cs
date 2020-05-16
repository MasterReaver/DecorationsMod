﻿using System.Collections.Generic;
using UnityEngine;

namespace DecorationsMod.NewItems
{
    public class WarperPart1 : DecorationItem
    {
        public WarperPart1() // Feeds abstract class
        {
            this.ClassID = "WarperPart1";
            this.PrefabFileName = DecorationItem.DefaultResourcePath + this.ClassID;

            this.GameObject = AssetsHelper.Assets.LoadAsset<GameObject>("Precursor_LostRiverBase_Warper");

            this.TechType = SMLHelper.V2.Handlers.TechTypeHandler.AddTechType(this.ClassID,
                                                        LanguageHelper.GetFriendlyWord("BigWarperPartName"),
                                                        LanguageHelper.GetFriendlyWord("BigWarperPartDescription"),
                                                        true);

            this.IsHabitatBuilder = true;

#if BELOWZERO
            this.Recipe = new SMLHelper.V2.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[5]
                    {
                        new Ingredient(TechType.Titanium, 2),
                        new Ingredient(TechType.Silicone, 1),
                        new Ingredient(TechType.FiberMesh, 1),
                        new Ingredient(TechType.Glass, 1),
                        new Ingredient(TechType.PrecursorIonCrystal, 1)
                    }),
            };
#else
            this.Recipe = new SMLHelper.V2.Crafting.TechData()
            {
                craftAmount = 1,
                Ingredients = new List<SMLHelper.V2.Crafting.Ingredient>(new SMLHelper.V2.Crafting.Ingredient[5]
                    {
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Titanium, 2),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Silicone, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Glass, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.Lubricant, 1),
                        new SMLHelper.V2.Crafting.Ingredient(TechType.FiberMesh, 1)
                    }),
            };
#endif
        }

        public override void RegisterItem()
        {
            if (this.IsRegistered == false)
            {
                // Get objects
                GameObject model = this.GameObject.FindChild("Model");

                // Rotate model
                //model.transform.localEulerAngles = new Vector3(model.transform.localEulerAngles.x - 90f, model.transform.localEulerAngles.y, model.transform.localEulerAngles.z);

                // Move model
                //model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y + 0.032f, model.transform.localPosition.z);

                // Apply shaders and materials
                var renderers = this.GameObject.GetAllComponentsInChildren<Renderer>();
                Shader marmosetUber = Shader.Find("MarmosetUBER");
                Texture normal1 = AssetsHelper.Assets.LoadAsset<Texture>("precursor_lab_warper_normal");
                Texture spec1 = AssetsHelper.Assets.LoadAsset<Texture>("precursor_lab_warper_spec");
                Texture illum1 = AssetsHelper.Assets.LoadAsset<Texture>("precursor_lab_warper_illum");
                //Texture normal2 = AssetsHelper.Assets.LoadAsset<Texture>("precursor_lab_warper_liquid_normal");
                if (renderers.Length > 0)
                {
                    foreach (Renderer rend in renderers)
                    {
                        if (rend.materials.Length > 0)
                        {
                            foreach (Material tmpMat in rend.materials)
                            {
                                if (tmpMat.name.CompareTo("precursor_lab_warper_liquid (Instance)") != 0 && !tmpMat.name.StartsWith("precursor_lab_warper_tube_"))
                                {
                                    tmpMat.shader = marmosetUber;
                                    if (tmpMat.name.CompareTo("precursor_lab_warper (Instance)") == 0)
                                    {
                                        tmpMat.SetTexture("_SpecTex", spec1);
                                        tmpMat.SetTexture("_BumpMap", normal1);
                                        tmpMat.SetTexture("_Illum", illum1);
                                        tmpMat.SetFloat("_EmissionLM", 1f);

                                        tmpMat.EnableKeyword("MARMO_SPECMAP");
                                        tmpMat.EnableKeyword("MARMO_NORMALMAP");
                                        tmpMat.EnableKeyword("MARMO_EMISSION");
                                        tmpMat.EnableKeyword("_ZWRITE_ON");
                                    }
                                    else if (tmpMat.name.CompareTo("precursor_lab_warper_box (Instance)") == 0)
                                    {
                                        tmpMat.SetTexture("_SpecTex", spec1);
                                        tmpMat.SetTexture("_BumpMap", normal1);
                                        tmpMat.SetTexture("_Illum", illum1);
                                        tmpMat.SetFloat("_EmissionLM", 1f);

                                        tmpMat.EnableKeyword("MARMO_SPECMAP");
                                        tmpMat.EnableKeyword("MARMO_NORMALMAP");
                                        tmpMat.EnableKeyword("MARMO_EMISSION");
                                        tmpMat.EnableKeyword("_ZWRITE_ON");
                                    }
                                }
                            }
                        }
                    }
                }

                // Add prefab identifier
                var prefabId = this.GameObject.AddComponent<PrefabIdentifier>();
                prefabId.ClassId = this.ClassID;

                // Add large world entity
                PrefabsHelper.SetDefaultLargeWorldEntity(this.GameObject);

                // Add tech tag
                var techTag = this.GameObject.AddComponent<TechTag>();
                techTag.type = this.TechType;

                // Add collider
                var collider = this.GameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.0075f, 0.0075f, 0.038f);
                collider.center = new Vector3(collider.center.x, collider.center.y, collider.center.z + 0.019f);

                // Add sky applier
                SkyApplier applier = this.GameObject.AddComponent<SkyApplier>();
                applier.renderers = renderers;
                applier.anchorSky = Skies.BaseInterior;
                applier.updaterIndex = 0;

                // Add contructable
                Constructable constructable = this.GameObject.AddComponent<Constructable>();
                constructable.allowedInBase = true;
                constructable.allowedInSub = true;
                constructable.allowedOnCeiling = true;
                constructable.allowedOnWall = true;
                constructable.allowedOutside = false;
                constructable.allowedOnGround = false;
                constructable.allowedOnConstructables = false;
#if BELOWZERO
                constructable.allowedUnderwater = true;
#endif
                constructable.controlModelState = true;
                constructable.deconstructionAllowed = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = this.TechType;
                constructable.enabled = true;

                // Add constructable bounds
                ConstructableBounds bounds = this.GameObject.AddComponent<ConstructableBounds>();
                //bounds.bounds.position = new Vector3(bounds.bounds.position.x, bounds.bounds.position.y + 0.032f, bounds.bounds.position.z);

                // Add new TechType to the buildables
                SMLHelper.V2.Handlers.CraftDataHandler.AddBuildable(this.TechType);
                SMLHelper.V2.Handlers.CraftDataHandler.AddToGroup(TechGroup.Miscellaneous, TechCategory.Misc, this.TechType);

                // Set the buildable prefab
                SMLHelper.V2.Handlers.PrefabHandler.RegisterPrefab(this);

                // Set the custom sprite
                SMLHelper.V2.Handlers.SpriteHandler.RegisterSprite(this.TechType, AssetsHelper.Assets.LoadAsset<Sprite>("warper_icon_1"));

                // Associate recipe to the new TechType
                SMLHelper.V2.Handlers.CraftDataHandler.SetTechData(this.TechType, this.Recipe);

                this.IsRegistered = true;
            }
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = GameObject.Instantiate(this.GameObject);

            prefab.name = this.ClassID;

            return prefab;
        }
    }
}