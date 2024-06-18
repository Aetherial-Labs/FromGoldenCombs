using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.Blocks.Langstroth;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.ServerMods;
using Vintagestory.GameContent;
using System;
using System.Text;
using Vintagestory.API.Config;



namespace FromGoldenCombs.Blocks
{
    class FrameRack : LangstrothCore
    {
        
        private string[] types;

        private string[] materials;

        private string[] materials2;

        private Dictionary<string, CompositeTexture> textures;

        private Cuboidf[] CollisionBox = new[]{ new Cuboidf(0.062, 0.0126, 0.156, 0.9254, 0.3624, 0.845)};

        private CompositeShape cshape;

        public Cuboidf[] selectionboxes;

        public int[] slots = new int[10];

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            LoadTypes();
        }

        public void LoadTypes()
        {
            types = Attributes["types"].AsArray<string>();
            cshape = Attributes["shape"].AsObject<CompositeShape>();
            textures = Attributes["textures"].AsObject<Dictionary<string, CompositeTexture>>();
            selectionboxes = Attributes["selectionboxes"].AsObject<Cuboidf[]>();
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

            List<JsonItemStack> list = new ();
            string[] array = types;
            foreach (string text in array)
            {
                string[] array2 = materials;
                foreach (string text2 in array2)
                {
                    string[] array3 = materials2;
                    foreach (string text3 in array3) { 
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
            return GetBlockEntity<BEFrameRack>(pos)?.getOrCreateSelectionBoxes() ?? base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return CollisionBox;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool num = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (num && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEFrameRack beFrameRack2)
            {
                BlockPos blockPos = (blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position);
                double y = byPlayer.Entity.Pos.X - ((double)blockPos.X + blockSel.HitPosition.X);
                double x = (double)(float)byPlayer.Entity.Pos.Z - ((double)blockPos.Z + blockSel.HitPosition.Z);
                float num2 = (float)Math.Atan2(y, x);
                float num3 = MathF.PI / 2f;
                float meshAngleRad = (float)(int)Math.Round(num2 / num3) * num3;
                beFrameRack2.MeshAngleRad = meshAngleRad;
                beFrameRack2.OnBlockPlaced(byItemStack);
            }

            return num;
        }

        public virtual MeshData GetOrCreateMesh(string type, string material, string material2, ITexPositionSource overrideTexturesource = null)
        {
            Dictionary<string, MeshData> orCreate = ObjectCacheUtil.GetOrCreate(api, "framerackMeshes", () => new Dictionary<string, MeshData>());
            ICoreClientAPI coreClientAPI = api as ICoreClientAPI;
            string key = type + "-" + material + "-" + material2;
            if (overrideTexturesource != null || !orCreate.TryGetValue(key, out var modeldata))
            {
                modeldata = new MeshData(4, 3);
                CompositeShape compositeShape = cshape.Clone();
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

                coreClientAPI.Tesselator.TesselateShape("Framerack2 block", shape, out modeldata, texPositionSource, null, 0, 0, 0);
                if (overrideTexturesource == null)
                {
                    orCreate[key] = modeldata;
                }
            }

            return modeldata;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            return new ItemStack[1] { OnPickBlock(world, pos) };
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            BlockDropItemStack[] dropsForHandbook = base.GetDropsForHandbook(handbookStack, forPlayer);
            dropsForHandbook[0] = dropsForHandbook[0].Clone();
            dropsForHandbook[0].ResolvedItemstack.SetFrom(handbookStack);
            return dropsForHandbook;
        }

        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            BEFrameRack blockEntity = GetBlockEntity<BEFrameRack>(pos);
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

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
            Dictionary<string, MultiTextureMeshRef> orCreate = ObjectCacheUtil.GetOrCreate(capi, "FramerackMeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
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
            if (world.BlockAccessor.GetBlockEntity(pos) is BEFrameRack beFrameRack2)
            {
                itemStack.Attributes.SetString("type", beFrameRack2.Type);
                itemStack.Attributes.SetString("material", beFrameRack2.Material);
                itemStack.Attributes.SetString("material2", beFrameRack2.Material2);
            }

            return itemStack;
        }

        /// <summary>Called when player right clicks the block</summary>
        /// <param name="world">The world.</param>
        /// <param name="byPlayer">The by player.</param>
        /// <param name="blockSel">The Selected Block.</param>
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEFrameRack beFrameRack = (BEFrameRack)world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEFrameRack;

            if (beFrameRack is BEFrameRack)
            {
                return beFrameRack.OnInteract(byPlayer, blockSel);
            }
            return false;
        }



        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            StringBuilder sb = new();
            Block block = world.BlockAccessor.GetBlock(pos);
            BEFrameRack be = world.BlockAccessor.GetBlockEntity<BEFrameRack>(pos);
            return Lang.Get(be.getMaterial().UcFirst()) + " & " + Lang.Get(be.getMaterial2().UcFirst() + sb.AppendLine() +  OnPickBlock(world, pos)?.GetName());
        }
    }
}
