﻿using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using FromGoldenCombs.BlockEntities;
using Vintagestory.GameContent;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using static OpenTK.Graphics.OpenGL.GL;

namespace FromGoldenCombs.Blocks
{

    class CeramicBroodPot : BlockContainer
    {

        /// <summary>
        /// When a player does a right click while targeting this placed block. Should return true if the event is handled, so that other events can occur, e.g. eating a held item if the block is not interactable with.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="byPlayer"></param>
        /// <param name="blockSel"></param>
        /// <returns>False if the interaction should be stopped. True if the interaction should continue. If you return false, the interaction will not be synced to the server.</returns>
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {                      
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockSel.Position);         
            if (beCeramicBroodPot is BECeramicBroodPot) return beCeramicBroodPot.OnInteract(byPlayer);
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        /// <summary>Called when a block is initially placed.</summary>
        /// <param name="world">The world.</param>
        /// <param name="blockPos">The block position.</param>
        /// <param name="stack">The stack.</param>
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack stack)
        {
            if (stack != null && stack.Attributes != null)
            {
                bool isHiveActive = stack.Attributes.GetBool("isactivehive", false);
                base.OnBlockPlaced(world, blockPos);
                BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(blockPos);
                if (beCeramicBroodPot == null) return;
                beCeramicBroodPot.isActiveHive = isHiveActive;
                beCeramicBroodPot.SetHiveSize(stack.Attributes.GetAsInt("hiveHealth"));
                beCeramicBroodPot.TestHarvestable(0);
            }
        }

        /// <summary>Called when a block is broken, and rolls chance of producing a beemob if populated.</summary>
        /// <param name="world">The world.</param>
        /// <param name="pos">The position.</param>
        /// <param name="byPlayer">The by player.</param>
        /// <param name="dropQuantityMultiplier">The drop quantity multiplier.</param>
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            BECeramicBroodPot beCeramicBroodPot = (BECeramicBroodPot)world.BlockAccessor.GetBlockEntity(pos);

            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                
                ItemStack stack = this.OnPickBlock(world, pos);
                beCeramicBroodPot.SetAttributes(stack);

                world.SpawnItemEntity(stack, new Vec3d((double)pos.X + 0.5, (double)pos.Y + 0.5, (double)pos.Z + 0.5));

                world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer);
            }

            if (EntityClass != null)
            {
                world.BlockAccessor.GetBlockEntity(pos)?.OnBlockBroken();
            }

            world.BlockAccessor.SetBlock(0, pos);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            WorldInteraction[] wi = null;
            WorldInteraction[] wi2 = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
            WorldInteraction[] wi3 = base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);

            
            if (world.BlockAccessor.GetBlockEntity(selection.Position) is BECeramicBroodPot pot)
            {
                if (pot != null)

                {

                    List<ItemStack> topList = new();
                    topList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-empty"))));
                    topList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", "hivetop-harvestable"))));

                    //Information about world interaction
                    if (!pot.isActiveHive)
                    {
                        wi = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions", () =>
                        {
                            List<ItemStack> skepList = new();
                            skepList.Add(new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("game", "skep-populated-east")), 1));

                            return new WorldInteraction[]
                            {
                            new WorldInteraction(){
                                ActionLangCode = "fromgoldencombs:blockhelp-ceramichive-empty-notop",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = skepList.ToArray()
                            }
                            };
                        });
                    }

                    wi2 = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions2", () =>
                    {
                        return new WorldInteraction[]
                        {
                          new WorldInteraction(){
                                ActionLangCode = Lang.Get("placeremovepot"),
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = topList.ToArray()
                       }
                     };
                    });

                    if (Variant["top"] == "notop")
                    {
                        wi3 = ObjectCacheUtil.GetOrCreate(api, "broodPotInteractions3", () =>
                            {

                                return new WorldInteraction[]
                                {
                            new WorldInteraction(){
                                ActionLangCode = Lang.Get("emptybagslot"),
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = null
                            }
                                };
                            });

                    }
                }

                if (wi != null)
                {
                    return wi.Append(wi2).Append(wi3);
                }
                return wi2.Append(wi3);
            }
            return wi;
        }

    }
}
