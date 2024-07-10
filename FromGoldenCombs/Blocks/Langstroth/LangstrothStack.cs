using FromGoldenCombs.BlockEntities;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothStack : LangstrothCore
    {
        #region ABR Added Variables
        public Cuboidf[] selectionboxes;
        public int[] slots = new int[3];
        #endregion

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            this.selectionboxes = this.SelectionBoxes;
            // Todo: Add interaction help
        }

        #region ABR Added Methods
        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool num = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (num && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BELangstrothStack beLangstrothStack)
            {
                BlockPos blockPos = (blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position);
                double y = byPlayer.Entity.Pos.X - ((double)blockPos.X + blockSel.HitPosition.X);
                double x = (double)(float)byPlayer.Entity.Pos.Z - ((double)blockPos.Z + blockSel.HitPosition.Z);
                float num2 = (float)Math.Atan2(y, x);
                float num3 = MathF.PI / 2f;
                float meshAngleRad = (float)(int)Math.Round(num2 / num3) * num3;
                beLangstrothStack.MeshAngleRad = meshAngleRad;
                beLangstrothStack.OnBlockPlaced(byItemStack);
                this.selectionboxes = byItemStack.Block.SelectionBoxes;
            }

            return num;
        }



        //public virtual MeshData GetOrCreateMesh(string type, string material, string material2, ITexPositionSource overrideTexturesource = null)
        //{
        //    Dictionary<string, MeshData> orCreate = ObjectCacheUtil.GetOrCreate(api, "langstrothStackMeshes", () => new Dictionary<string, MeshData>());
        //    ICoreClientAPI coreClientAPI = api as ICoreClientAPI;
        //    string key = type + "-" + material + "-" + material2;
        //    if (overrideTexturesource != null || !orCreate.TryGetValue(key, out var modeldata))
        //    {
        //        modeldata = new MeshData(4, 3);
        //        CompositeShape compositeShape = cshape.Clone();
        //        compositeShape.Base.Path = compositeShape.Base.Path.Replace("{type}", type).Replace("{material}", material).Replace("{material2}", material2);
        //        compositeShape.Base.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/");
        //        Shape shape = coreClientAPI.Assets.TryGet(compositeShape.Base)?.ToObject<Shape>();
        //        ITexPositionSource texPositionSource = overrideTexturesource;
        //        if (texPositionSource == null)
        //        {
        //            ShapeTextureSource shapeTextureSource = new ShapeTextureSource(coreClientAPI, shape, compositeShape.Base.ToString());
        //            texPositionSource = shapeTextureSource;
        //            foreach (KeyValuePair<string, CompositeTexture> texture in textures)
        //            {
        //                CompositeTexture compositeTexture = texture.Value.Clone();
        //                compositeTexture.Base.Path = compositeTexture.Base.Path.Replace("{type}", type).Replace("{material}", material).Replace("{material2}", material2);
        //                compositeTexture.Bake(coreClientAPI.Assets);
        //                shapeTextureSource.textures[texture.Key] = compositeTexture;
        //            }
        //        }

        //        if (shape == null)
        //        {
        //            return modeldata;
        //        }

        //        coreClientAPI.Tesselator.TesselateShape("LangstrothStack block", shape, out modeldata, texPositionSource, null, 0, 0, 0);
        //        if (overrideTexturesource == null)
        //        {
        //            orCreate[key] = modeldata;
        //        }
        //    }

        //    return modeldata;
        //}

        ////This method seems directly tied to rendering the contents of the blocks inventory.
        //public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        //{
        //    base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        //    Dictionary<string, MultiTextureMeshRef> orCreate = ObjectCacheUtil.GetOrCreate(capi, "LangstrothStackMeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
        //    string @string = itemstack.Attributes.GetString("type", "");
        //    string string2 = itemstack.Attributes.GetString("material", "");
        //    string string3 = itemstack.Attributes.GetString("material2", "");
        //    string key = @string + "-" + string2 + "-" + string3;
        //    if (!orCreate.TryGetValue(key, out var value))
        //    {
        //        MeshData orCreateMesh = GetOrCreateMesh(@string, string2, string3);
        //        value = (orCreate[key] = capi.Render.UploadMultiTextureMesh(orCreateMesh));
        //    }

        //    renderinfo.ModelRef = value;
        //}

        #endregion

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (world.BlockAccessor.GetBlock(pos.DownCopy(), 0).BlockMaterial == EnumBlockMaterial.Air)
            {
                this.OnBlockBroken(world, pos, null);
                if (world.BlockAccessor.GetBlock(pos.UpCopy(), 0) is LangstrothCore)
                {
                    world.BlockAccessor.GetBlock(pos.UpCopy(), 0).OnNeighbourBlockChange(world, pos.UpCopy(), neibpos);
                }

            }
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack[] array = new ItemStack[] { };
                for (int i = 0; i < array.Length; i++)
                {
                    world.SpawnItemEntity(array[i], new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5), null);
                }
                world.PlaySoundAt(this.Sounds.GetBreakSound(byPlayer), (double)pos.X, (double)pos.Y, (double)pos.Z, byPlayer, true, 32f, 1f);
            }
            if (this.EntityClass != null)
            {
                BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
                if (blockEntity != null)
                {
                    blockEntity.OnBlockBroken();
                }
            }
            world.BlockAccessor.SetBlock(0, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BELangstrothStack belangstrothstack = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELangstrothStack;
            if (belangstrothstack is BELangstrothStack) return belangstrothstack.OnInteract(byPlayer);
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi;

            wi = ObjectCacheUtil.GetOrCreate(api, "stackInteractions1", () =>
            {

                return new WorldInteraction[] {
                            new WorldInteraction() {
                                    ActionLangCode = "fromgoldencombs:blockhelp-langstrothstack",
                                    MouseButton = EnumMouseButton.Right,
                            }
                    };

            });

            return wi;
        }
    }
}