using FromGoldenCombs.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;
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
                api.World.BlockAccessor.SetBlock(api.World.GetBlock(
                new AssetLocation("fromgoldencombs", "langstrothstack-two-" + block.Variant["side"])).BlockId, blockSel.Position);
                BELangstrothStack lStack = (BELangstrothStack)api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                lStack.InitializePut(langstrothblock, slot);
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
            if (this is LangstrothStack)
            return base.GetPlacedBlockName(world, pos);

            if (this is FrameRack2) return "FrameRack2";
            StringBuilder sb = new();
            return base.GetPlacedBlockName(world, pos) + sb.AppendLine() + Lang.Get("fromgoldencombs:getmaterials", this.Variant["primary"].ToString().UcFirst(), this.Variant["accent"].ToString().UcFirst());
        }
    }
 }
