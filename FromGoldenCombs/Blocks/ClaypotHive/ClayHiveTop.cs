using FromGoldenCombs.Util.config;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
//using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace FromGoldenCombs.Blocks
{
    class ClayHiveTop : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            Block emptyTop = world.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"));

            //provided TryGiveItemStack is true just drops it into a convenient empty slot.
            if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack == null && byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(this)))
            {
                //If the active hot bar slot is empty, and can the player can accept the item, pick it up, play sound.
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.PlaySoundAt(new AssetLocation("sounds/block/planks"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, false);
                return true;
            }
            else if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Item?.Tool != null && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Tool.Value == EnumTool.Knife
              && this.Variant["type"] == "harvestable")
            {
                //If the top is harvestable, and the player uses a knife on it, drop between 1-5 honeycomb and return an empty pot.
                Random rand = new();
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(world.GetItem(new AssetLocation("game", "honeycomb")), rand.Next(FromGoldenCombsConfig.Current.CeramicPotMinYield, FromGoldenCombsConfig.Current.CeramicPotMaxYield)));
                byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Item.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, 1);
                world.BlockAccessor.SetBlock(emptyTop.BlockId, blockSel.Position);
            }
            return true;

        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi;
            if (Variant["type"] == "harvestable") {
                wi = ObjectCacheUtil.GetOrCreate(api, "honeyPotInteractions", () =>
                {
                    List<ItemStack> knifeStacklist = new List<ItemStack>();

                    foreach (Item item in api.World.Items)
                    {
                        if (item.Code == null) continue;

                        if (item.Tool == EnumTool.Knife)
                        {
                            knifeStacklist.Add(new ItemStack(item));
                        }
                    }

                    return new WorldInteraction[] {
                        new WorldInteraction() {
                                ActionLangCode = "fromgoldencombs:blockhelp-honeypot-harvestable",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = knifeStacklist.ToArray()
                            }
                        };
                });
                return wi.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }

            return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = Variant["type"] == "raw" ? "fromgoldencombs:blockhelp-honeypot-raw" : "fromgoldencombs:blockhelp-honeypot-empty",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = null
                    }
            };
        }
    }
}
