using FromGoldenCombs.config;
using System.Collections.Generic;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using FromGoldenCombs.Blocks;
using System.Diagnostics;
using Vintagestory;

namespace FromGoldenCombs.BlockEntities
{

    class BEFrameRack2 : BlockEntityDisplay, IRotatable
    {

        readonly InventoryGeneric inv;
        public override string InventoryClassName => "framerack";

        private Block block;
        public override InventoryBase Inventory => inv;
        private MeshData mesh;

        private string type;

        private string material;

        private float[] mat;

        private int[] UsableSlots;

        private Cuboidf[] UsableSelectionBoxes;

        public override string AttributeTransformCode => "onframerackTransform";

        public float MeshAngleRad { get; set; }

        public string Type => type;

        public string Material => material;

        public BEFrameRack2()
        {
            inv = new InventoryGeneric(10, "frameslot-0", null, null);
        }



        public override void Initialize(ICoreAPI api)
        {

            block = api.World.BlockAccessor.GetBlock(Pos, 0);
            base.Initialize(api);
            if (mesh == null && type != null)
            {
                initFrameRack();
            }
        }

        private void initFrameRack()
        {
            if (Api != null && type != null && base.Block is FrameRack2)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    mesh = (base.Block as FrameRack2).GetOrCreateMesh(type, material);
                    mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(MeshAngleRad)
                        .Translate(-0.5f, -0.5f, -0.5f)
                        .Values;
                }

                if (block is FrameRack2)
                {
                    type = "normal";
                }
            }
        }

        public int[] getOrCreateUsableSlots()
        {
            if (UsableSlots == null)
            {
                genUsableSlots();
            }

            return UsableSlots;
        }

        private void genUsableSlots()
        {
            bool num = isRack(BEBehaviorDoor.getAdjacentOffset(-1, 0, 0, MeshAngleRad, invertHandles: false));
            int[] slots = (base.Block as FrameRack2).slots;
            List<int> list = new();
            list.AddRange(slots);
            UsableSlots = list.ToArray();
            Cuboidf[] selectionboxes = (base.Block as FrameRack2).selectionboxes;
            UsableSelectionBoxes = new Cuboidf[selectionboxes.Length];
            for (int i = 0; i < selectionboxes.Length; i++)
            {
                UsableSelectionBoxes[i] = selectionboxes[i].RotatedCopy(0f, MeshAngleRad * (180f / MathF.PI), 0f, new Vec3d(0.5, 0.5, 0.5));
            }
        }

        private bool isRack(Vec3i offset)
        {
            BEFrameRack2 blockEntity = Api.World.BlockAccessor.GetBlockEntity<BEFrameRack2>(Pos.AddCopy(offset));
            if (blockEntity != null)
            {
                return blockEntity.MeshAngleRad == MeshAngleRad;
            }

            return false;
        }
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            type = byItemStack?.Attributes.GetString("type");
            material = byItemStack?.Attributes.GetString("material");
            initFrameRack();
        }

        public override void OnBlockBroken(IPlayer byPlayer)
        {
            // Don't drop inventory contents
        }

        public Cuboidf[] getOrCreateSelectionBoxes()
        {
            getOrCreateUsableSlots();
            return UsableSelectionBoxes;
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            getOrCreateUsableSlots();
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            CollectibleObject colObj = slot.Itemstack?.Collectible;
            bool isBeeframe = colObj?.FirstCodePart() == "beeframe";
            BlockContainer block = Api.World.BlockAccessor.GetBlock(blockSel.Position, 0) as BlockContainer;
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
            else if (slot.Itemstack?.Item?.Tool != null && slot.Itemstack?.Item?.Tool == EnumTool.Knife && index < 10 && !inv[index].Empty && inv[index].Itemstack.Collectible.Variant["harvestable"] == "harvestable")
            {
                if (TryHarvest(Api.World, byPlayer, inv[index]))
                {

                    slot.Itemstack.Item.DamageItem(Api.World, byPlayer.Entity, slot, 1);
                    MarkDirty(true);
                    return true;
                }
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

            ItemStack stackHandler;
            int durability;

            stackHandler = rackStack.Itemstack;

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
            Api.World.SpawnItemEntity(new ItemStack(Api.World.GetItem(new AssetLocation("game", "honeycomb")), rnd.Next(FromGoldenCombsConfig.Current.FrameMinYield, FromGoldenCombsConfig.Current.FrameMaxYield)), Pos.ToVec3d());
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

        protected override float[][] genTransformationMatrices()
        {
            tfMatrices = new float[Inventory.Count][];
            Cuboidf[] selectionBoxes = (base.Block as FrameRack2).selectionboxes;
            for (int i = 0; i < Inventory.Count; i++)
            {
                Cuboidf obj = selectionBoxes[i];
                float midX = obj.MidX;
                float midY = 0.069f;
                float midZ = obj.MidZ;
                Vec3f vec3f = new Vec3f(midX, midY, midZ);
                vec3f = new Matrixf().RotateY(MeshAngleRad).TransformVector(vec3f.ToVec4f(0f)).XYZ;
                tfMatrices[i] = new Matrixf().Translate(vec3f.X, vec3f.Y, vec3f.Z).Translate(0.5f, 0f, 0.5f).RotateY(MeshAngleRad - MathF.PI)

                    .Values;
            }
            return tfMatrices;
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            mesher.AddMeshData(mesh, mat);
            base.OnTesselation(mesher, tessThreadTesselator);
            return true;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("type", type);
            tree.SetString("material", material);
            tree.SetFloat("meshAngleRad", MeshAngleRad);
            tree.SetBool("usableSlotsDirty", UsableSlots == null);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            type = tree.GetString("type");
            material = tree.GetString("material");
            MeshAngleRad = tree.GetFloat("meshAngleRad");
            if (tree.GetBool("usableSlotsDirty"))
            {
                UsableSlots = null;
            }

            initFrameRack();
            RedrawAfterReceivingTreeAttributes(worldForResolving);
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

        public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
        {
            MeshAngleRad = tree.GetFloat("meshAngleRad");
            MeshAngleRad -= (float)degreeRotation * (MathF.PI / 180f);
            tree.SetFloat("meshAngleRad", MeshAngleRad);
        }
    }
}

