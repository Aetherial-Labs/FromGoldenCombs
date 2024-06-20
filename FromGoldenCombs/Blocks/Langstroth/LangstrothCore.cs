using FromGoldenCombs.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.Blocks.Langstroth
{
    class LangstrothCore : BlockContainer
    {

        //Enable selectionbox interaction
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            Block block = api.World.BlockAccessor.GetBlock(blockSel.Position, 0);
            if (!slot.Empty && slot.Itemstack.Collectible is Block && IsValidLangstroth(slot.Itemstack.Block))
            {
                ItemStack langstrothblock = api.World.BlockAccessor.GetBlock(blockSel.Position).OnPickBlock(world, blockSel.Position);
                ItemStack langstrothstackblock = new(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "langstrothstack-two-east")));
                ((LangstrothStack)langstrothstackblock.Collectible).DoPlaceBlock(world, byPlayer, blockSel, langstrothstackblock);
                BELangstrothStack lStack = (BELangstrothStack)api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                lStack.InitializePut(langstrothblock, slot, byPlayer, blockSel);
            }
            return true;
        }

        public static bool IsValidLangstroth(Block block)
        {
            if (block is LangstrothCore && !(block is LangstrothBrood))
            {
                return true;
            }
            return false;
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            StringBuilder sb = new();
            if (this is LangstrothStack)
            return base.GetPlacedBlockName(world, pos);
            if (this is FrameRack rack) {
                BEFrameRack be = world.BlockAccessor.GetBlockEntity<BEFrameRack>(pos);
                return null;
            }
            return Lang.Get(this.Variant["primary"].ToString().UcFirst()) + " " + Lang.Get(this.Variant["accent"].ToString().UcFirst()) + sb.AppendLine() + base.GetPlacedBlockName(world, pos);
        }
    }
 }
