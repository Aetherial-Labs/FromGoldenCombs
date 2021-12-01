﻿using FromGoldenCombs.Blocks;
using FromGoldenCombs.config;
using FromGoldenCombs.Items;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    //TODO: Consider adding a lid object, or adding an animation showing the lid being slid off (This sounds neat). 
    //TODO: Find out how to get animation functioning
    //TODO: Fix selection box issue
    
    class BEFrameRack : BlockEntityDisplay
    {

        readonly InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "framerack";

        Block block;

        public BEFrameRack()
        {
            inv = new InventoryGeneric(10, "frameslot-0", null, null);
            meshes = new MeshData[10];
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos);
            base.Initialize(api);
        }

        public override void OnBlockBroken()
        {
            // Don't drop inventory contents
        }
        
        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isBeeframe = colObj is LangstrothFrame;
            BlockContainer block = Api.World.BlockAccessor.GetBlock(blockSel.Position) as BlockContainer;
            int index = blockSel.SelectionBoxIndex;
            block.SetContents(new(block), this.GetContentStacks());
            if (slot.Empty && index < 10)
            {
                if (TryTake(byPlayer, blockSel))
                {
                    MarkDirty(true);
                    return true;
                }
            }
            else if (slot.Itemstack?.Item?.FirstCodePart() == "knife" && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "harvestable")
            {
                ItemStack stack = slot.Itemstack;
                ItemStack rackSlot = inv[index].Itemstack;
                if (TryHarvest(Api.World, byPlayer, inv[index]))
                {

                    slot.Itemstack.Item.DamageItem(Api.World, byPlayer.Entity, slot, 1);
                    MarkDirty(true);
                    return true;
                }
                System.Diagnostics.Debug.WriteLine("TryHarvest Checkpoint Beta");
                MarkDirty(true);
            }
            else if (slot.Itemstack?.Item?.FirstCodePart() == "waxedflaxtwine" && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "lined")
            {
                ItemStack rackSlot = inv[index].Itemstack;
                if (TryRepair(slot, rackSlot, index))
                {

                    MarkDirty(true);
                    return true;
                }
                MarkDirty(true);
            }
            else if (slot.Itemstack?.Item?.FirstCodePart() == "frameliner" && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "empty")
            {
                inv[index].Itemstack = new ItemStack(Api.World.GetItem(inv[index].Itemstack.Item.CodeWithVariant("harvestable", "lined")));
                inv[index].Itemstack.Attributes.SetInt("durability", 32);
                slot.TakeOut(1);
                MarkDirty(true);
                return true;
            }
            else if (isBeeframe && index < 10)
            {
                MarkDirty(true);
                if (TryPut(slot, blockSel))
                {
                    return true;
                }

            }
            else if (slot.Empty
                     && (int)slot.StorageType == 2
                     && byPlayer.InventoryManager.TryGiveItemstack(block.OnPickBlock(Api.World, blockSel.Position)))
            {
                Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                MarkDirty(true);
                return true;
            }
            return false;
        }

        private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;

            for (int i = 0; i < inv.Count; i++)
            {
                int slotnum = (index + i) % inv.Count;
                    if (inv[slotnum].Empty)
                    {
                        int moved = slot.TryPutInto(Api.World, inv[slotnum]);
                        updateMeshes();
                        return moved > 0;
                    }
            }
            return false;
        }

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
            
            if (!inv[index].Empty)
            {
                ItemStack stack = inv[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMeshes();
                return true;
            }

            return false;
        }

        private bool TryHarvest(IWorldAccessor world, IPlayer player, ItemSlot rackStack)
        {
            ThreadSafeRandom rnd = new();
            int minYield = FromGoldenCombsConfig.Current.minFrameYield;
            int maxYield = FromGoldenCombsConfig.Current.maxFrameYield;
            ItemStack stackHandler;
            int durability = FromGoldenCombsConfig.Current.baseframedurability;

            stackHandler = rackStack.Itemstack;
            durability = rackStack.Itemstack.Attributes.GetInt("durability");

            //Check to see if harvestable rack will break when harvested
            if (rackStack.Itemstack.Attributes.GetInt("durability") == 1)
            {
                //Next use will destroy frame, swap it for an empty frame instead
                rackStack.Itemstack = new ItemStack(Api.World.GetItem(stackHandler.Item.CodeWithVariant("harvestable", "empty")));
            } else {
                rackStack.Itemstack.Collectible.DamageItem(Api.World, player.Entity, rackStack, 1);
                durability = rackStack.Itemstack.Attributes.GetInt("durability");
                rackStack.Itemstack = new ItemStack(Api.World.GetItem(stackHandler.Item.CodeWithVariant("harvestable", "lined")));
                rackStack.Itemstack.Attributes.SetInt("durability", durability);

            }
            Api.World.SpawnItemEntity(new ItemStack(Api.World.GetItem(new AssetLocation("game", "honeycomb")), rnd.Next(minYield, maxYield)), Pos.ToVec3d());
            return true;
        }

        private bool TryRepair(ItemSlot slot, ItemStack rackStack, int index)
        {
            int durability = rackStack.Attributes.GetInt("durability");
            int maxDurability = FromGoldenCombsConfig.Current.baseframedurability;

            if (durability == maxDurability)
            return false;

            rackStack.Attributes.SetInt("durability", (maxDurability - durability) < 16 ? maxDurability : durability + 16);
            slot.TakeOut(1);
            inv[index].Itemstack = rackStack;
            return true;
        }

        readonly Matrixf mat = new();

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        protected override void updateMeshes()
        {
            mat.Identity();
            mat.RotateYDeg(block.Shape.rotateY);

            base.updateMeshes();
        }

        protected override MeshData genMesh(ItemStack stack, int index)
        {

            ICoreClientAPI capi = Api as ICoreClientAPI;
            nowTesselatingItem = stack.Item;
            nowTesselatingShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
            capi.Tesselator.TesselateItem(stack.Item, out MeshData mesh, this);

            mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

            float x = 0f;
            float y = 0.069f;
            float z = 0f;

            if (block.Variant["side"] == "north")
            {
                x = .7253f + .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            } else if (block.Variant["side"] == "south")
            {
                x = 1.2878f - .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            } else if (block.Variant["side"] == "east")
            {
                z = 1.2878f - .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
            else if (block.Variant["side"] == "west")
            {
                z = .7253f + .0625f * index - 1;
                Vec4f offset = mat.TransformVector(new Vec4f(x, y, z, 0));
                mesh.Translate(offset.XYZ);
            }
            ModelTransform transform = stack.Collectible.Attributes.AsObject<ModelTransform>();
            transform.EnsureDefaultValues();
            transform.Rotation.X = 0;
            transform.Rotation.Y = block.Shape.rotateY;
            transform.Rotation.Z = 0;
            mesh.ModelTransform(transform);

            return mesh;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            }
            else { 
                sb.AppendLine();
                for (int i = 0; i < 10; i++)
                {
                    ItemSlot slot = inv[i];
                    if (slot.Empty)
                    {
                        sb.AppendLine(Lang.Get("Empty"));
                    }
                    else
                    {
                        sb.AppendLine(slot.Itemstack.GetName());
                    }
                }
            }
        }
    }
}

