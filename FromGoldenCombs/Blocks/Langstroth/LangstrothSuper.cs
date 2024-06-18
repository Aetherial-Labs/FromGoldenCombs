using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace FromGoldenCombs.Blocks
{
    class LangstrothSuper : LangstrothCore
    {
        private string[] types;

        private string[] materials;

        private string[] materials2;

        private Dictionary<string, CompositeTexture> textures;

        private CompositeShape closedshape;
        private CompositeShape openshape;

        private Cuboidf[] CollisionBox = new[] { new Cuboidf(0.062, 0.0126, 0.156, 0.9254, 0.3624, 0.845) };

        public Cuboidf[] openselectionboxes;
        public Cuboidf[] closedselectionboxes;

        public int[] slots = new int[10];

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            LoadTypes();
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Block is LangstrothCore))
            {
                BELangstrothSuper belangstrothsuper = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothSuper;
                if (belangstrothsuper is BELangstrothSuper) return belangstrothsuper.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        public void LoadTypes()
        {
            types = Attributes["types"].AsArray<string>();
            closedshape = Attributes["shape"].AsObject<CompositeShape>();
            openshape = Attributes["shape"].AsObject<CompositeShape>();
            textures = Attributes["textures"].AsObject<Dictionary<string, CompositeTexture>>();
            openselectionboxes = Attributes["openselectionboxes"].AsObject<Cuboidf[]>();
            closedselectionboxes = Attributes["closedselectionboxes"].AsObject<Cuboidf[]>();
            RegistryObjectVariantGroup registryObjectVariantGroup = Attributes["materials"].AsObject<RegistryObjectVariantGroup>();
            RegistryObjectVariantGroup registryObjectVariantGroup2 = Attributes["materials2"].AsObject<RegistryObjectVariantGroup>();
            materials = registryObjectVariantGroup.States;
            materials2 = registryObjectVariantGroup2.States;
            if (registryObjectVariantGroup.LoadFromProperties != null)
            {
                StandardWorldProperty standardWorldProperty = api.Assets.TryGet(registryObjectVariantGroup.LoadFromProperties.WithPathPrefixOnce("worldproperties/").WithPathAppendixOnce(".json"))?.ToObject<StandardWorldProperty>();
                materials = standardWorldProperty.Variants.Select((WorldPropertyVariant p) => p.Code.Path).ToArray().Append(materials);
                materials2 = standardWorldProperty.Variants.Select((WorldPropertyVariant p) => p.Code.Path).ToArray().Append(materials2);
            }

            List<JsonItemStack> list = new();
            string[] array = types;
            foreach (string text in array)
            {
                string[] array2 = materials;
                foreach (string text2 in array2)
                {
                    string[] array3 = materials2;
                    foreach (string text3 in array3)
                    {
                        JsonItemStack jsonItemStack = new JsonItemStack();
                        jsonItemStack.Code = Code;
                        jsonItemStack.Type = EnumItemClass.Block;
                        jsonItemStack.Attributes = new JsonObject(JToken.Parse("{ \"type\": \"" + text + "\", \"material\": \"" + text2 + "\", \"material2\": \"" + text3 + "\" }"));
                        JsonItemStack jsonItemStack2 = jsonItemStack;
                        jsonItemStack2.Resolve(api.World, Code?.ToString() + " type");
                        list.Add(jsonItemStack2);
                    }
                }
            }

            CreativeInventoryStacks = new CreativeTabAndStackList[1]
            {
            new()
            {
                Stacks = list.ToArray(),
                Tabs = new string[2] { "general", "decorative" }
            }
            };
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return GetBlockEntity<BELangstrothSuper>(pos)?.getOrCreateSelectionBoxes() ?? base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return CollisionBox;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool num = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (num && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BELangstrothSuper bELangstrothSuper)
            {
                BlockPos blockPos = (blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position);
                double y = byPlayer.Entity.Pos.X - ((double)blockPos.X + blockSel.HitPosition.X);
                double x = (double)(float)byPlayer.Entity.Pos.Z - ((double)blockPos.Z + blockSel.HitPosition.Z);
                float num2 = (float)Math.Atan2(y, x);
                float num3 = MathF.PI / 2f;
                float meshAngleRad = (float)(int)Math.Round(num2 / num3) * num3;
                bELangstrothSuper.MeshAngleRad = meshAngleRad;
                bELangstrothSuper.OnBlockPlaced(byItemStack);
            }

            return num;
        }

        public virtual MeshData GetOrCreateMesh(string type, string material, string material2, ITexPositionSource overrideTexturesource = null)
        {
            Dictionary<string, MeshData> orCreate = ObjectCacheUtil.GetOrCreate(api, "langstrothsuperMeshes", () => new Dictionary<string, MeshData>());
            ICoreClientAPI coreClientAPI = api as ICoreClientAPI;
            string key = type + "-" + material + "-" + material2;
            if (overrideTexturesource != null || !orCreate.TryGetValue(key, out var modeldata))
            {
                modeldata = new MeshData(4, 3);
                CompositeShape compositeShape = type == "closed"?closedshape.Clone():openshape.Clone();
                compositeShape.Base.Path = compositeShape.Base.Path.Replace("{type}", type).Replace("{material}", material).Replace("{material2}", material2);
                compositeShape.Base.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/");
                Shape shape = coreClientAPI.Assets.TryGet(compositeShape.Base)?.ToObject<Shape>();
                ITexPositionSource texPositionSource = overrideTexturesource;
                if (texPositionSource == null)
                {
                    ShapeTextureSource shapeTextureSource = new ShapeTextureSource(coreClientAPI, shape, compositeShape.Base.ToString());
                    texPositionSource = shapeTextureSource;
                    foreach (KeyValuePair<string, CompositeTexture> texture in textures)
                    {
                        CompositeTexture compositeTexture = texture.Value.Clone();
                        compositeTexture.Base.Path = compositeTexture.Base.Path.Replace("{type}", type).Replace("{material}", material).Replace("{material2}", material2);
                        compositeTexture.Bake(coreClientAPI.Assets);
                        shapeTextureSource.textures[texture.Key] = compositeTexture;
                    }
                }

                if (shape == null)
                {
                    return modeldata;
                }

                coreClientAPI.Tesselator.TesselateShape("LangstrothSuper block", shape, out modeldata, texPositionSource, null, 0, 0, 0);
                if (overrideTexturesource == null)
                {
                    orCreate[key] = modeldata;
                }
            }

            return modeldata;
        }

        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            BELangstrothSuper blockEntity = GetBlockEntity<BELangstrothSuper>(pos);
            if (blockEntity != null)
            {
                float[] values = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(blockEntity.MeshAngleRad)
                    .Translate(-0.5f, -0.5f, -0.5f)
                    .Values;
                blockModelData = GetOrCreateMesh(blockEntity.Type, blockEntity.Material, blockEntity.Material2).Clone().MatrixTransform(values);
                decalModelData = GetOrCreateMesh(blockEntity.Type, blockEntity.Material, blockEntity.Material2, decalTexSource).Clone().MatrixTransform(values);
            }
            else
            {
                base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
            }
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
            Dictionary<string, MultiTextureMeshRef> orCreate = ObjectCacheUtil.GetOrCreate(capi, "LangstrothSuperMeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
            string @string = itemstack.Attributes.GetString("type", "");
            string string2 = itemstack.Attributes.GetString("material", "");
            string string3 = itemstack.Attributes.GetString("material2", "");
            string key = @string + "-" + string2 + "-" + string3;
            if (!orCreate.TryGetValue(key, out var value))
            {
                MeshData orCreateMesh = GetOrCreateMesh(@string, string2, string3);
                value = (orCreate[key] = capi.Render.UploadMultiTextureMesh(orCreateMesh));
            }

            renderinfo.ModelRef = value;
        }
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack itemStack = base.OnPickBlock(world, pos);
            if (world.BlockAccessor.GetBlockEntity(pos) is BELangstrothSuper beLangstrothSuper)
            {
                itemStack.Attributes.SetString("type", beLangstrothSuper.Type);
                itemStack.Attributes.SetString("material", beLangstrothSuper.Material);
                itemStack.Attributes.SetString("material2", beLangstrothSuper.Material2);
            }

            return itemStack;
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            StringBuilder sb = new();
            Block block = world.BlockAccessor.GetBlock(pos);
            BELangstrothSuper be = world.BlockAccessor.GetBlockEntity<BELangstrothSuper>(pos);
            //return Lang.Get(be.getMaterial().UcFirst()) + " & " + Lang.Get(be.getMaterial2().UcFirst() + sb.AppendLine() + OnPickBlock(world, pos)?.GetName());
            return null;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
           
            WorldInteraction[] wi;
            WorldInteraction[] wi2 = null;
            WorldInteraction[] wi3 = null;
            WorldInteraction[] wx = null;

            wi = ObjectCacheUtil.GetOrCreate(api, "superInteractions1", () =>
            {

                return new WorldInteraction[] {
                    new WorldInteraction() {
                            ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper",
                            MouseButton = EnumMouseButton.Right

                        }
                    };
            });

            if (Variant["open"] == "open")
            {
                wi2 = ObjectCacheUtil.GetOrCreate(api, "superInteractions2", () =>
                {

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-open",
                                    MouseButton = EnumMouseButton.Right
                            }
                    };

                });
            }

            if (Variant["open"] == "closed")
            {
                wi2 = ObjectCacheUtil.GetOrCreate(api, "superInteractions3", () =>
                {

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-closed",
                                    MouseButton = EnumMouseButton.Right,
                                    HotKeyCode = "sneak"
                            }
                    };

                });

                wi3 = ObjectCacheUtil.GetOrCreate(api, "superInteractions4", () =>
                {
                    List<ItemStack> toplist = new();

                    foreach (Item item in api.World.Items)
                    {
                        if (item.Code == null) continue;

                        if (item.FirstCodePart() == "langstrothbroodtop")
                        {
                            toplist.Add(new ItemStack(item));
                        }
                    }

                    return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothsuper-broodtop",
                                    MouseButton = EnumMouseButton.Right,
                                    Itemstacks = toplist.ToArray()
                            }
                    };
                });
            }
            if (wi2 == null || wi3 == null)
            {
                return wi;
            } else if (wi3 == null)
            {
                return wi.Append(wi2);
            }
            return wi.Append(wi2).Append(wi3).Append(wx);
        }
    }
}
