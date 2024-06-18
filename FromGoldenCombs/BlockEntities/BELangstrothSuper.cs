
using FromGoldenCombs.Blocks;
using static OpenTK.Graphics.OpenGL.GL;
using System.Collections.Generic;
using System.Text;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{
    //TODO: Consider adding a lid object, or adding an animation showing the lid being slid off (This sounds neat). 
    //TODO: Find out how to get animation functioning
    //TODO: Fix selection box issue

    class BELangstrothSuper : BlockEntityDisplay
    {

        readonly InventoryGeneric inv;
        public override string InventoryClassName => "langstrothsuper";

        private Block block;
        public override InventoryBase Inventory => inv;
        private MeshData mesh;

        private string type = "closed";

        private string material;

        private string material2;

        private float[] mat;

        private int[] UsableSlots;

        private Cuboidf[] UsableSelectionBoxes;

        public override string AttributeTransformCode => "onlangstrothsuperTransform";

        public float MeshAngleRad { get; set; }

        public string Type => type;

        public string Material => material;
        public string Material2 => material2;

        public BELangstrothSuper()
        {
            inv = new InventoryGeneric(10, "superslot-0", null, null);
        }

        public string getMaterial()
        {
            return material;
        }

        public string getMaterial2()
        {
            return material2;
        }

        public override void Initialize(ICoreAPI api)
        {
            block = api.World.BlockAccessor.GetBlock(Pos, 0);
            base.Initialize(api);
            if (mesh == null && type != null)
            {
                initLangstrothSuper();
            }
        }

        private void initLangstrothSuper()
        {
            if (Api != null && type != null && base.Block is LangstrothSuper)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    mesh = (base.Block as LangstrothSuper).GetOrCreateMesh(type, material, material2);
                    mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(MeshAngleRad)
                        .Translate(-0.5f, -0.5f, -0.5f)
                        .Values;
                }

                if (block is LangstrothSuper)
                {
                    type = type;
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
            int[] slots = (base.Block as LangstrothSuper).slots;
            List<int> list = new();
            list.AddRange(slots);
            UsableSlots = list.ToArray();
            Cuboidf[] selectionboxes = type == "open" ? (base.Block as LangstrothSuper).openselectionboxes : (base.Block as LangstrothSuper).closedselectionboxes;
            UsableSelectionBoxes = new Cuboidf[selectionboxes.Length];
            for (int i = 0; i < selectionboxes.Length; i++)
            {
                UsableSelectionBoxes[i] = selectionboxes[i].RotatedCopy(0f, MeshAngleRad * (180f / MathF.PI), 0f, new Vec3d(0.5, 0.5, 0.5));
            }
        }
        public override void OnBlockBroken(IPlayer player)
        {
            // Don't drop inventory contents
        }
        public Cuboidf[] getOrCreateSelectionBoxes()
        {
            getOrCreateUsableSlots();
            return UsableSelectionBoxes;
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            type = byItemStack?.Attributes.GetString("type");
            material = byItemStack?.Attributes.GetString("material");
            material2 = byItemStack?.Attributes.GetString("material2");
            initLangstrothSuper();
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemStack itemstack = activeHotbarSlot.Itemstack;
            bool flag = (itemstack != null ? itemstack.Collectible.FirstCodePart() == "beeframe" : false);
            BlockContainer blockContainer = this.Api.World.BlockAccessor.GetBlock(blockSel.Position, 0) as BlockContainer;
            blockContainer.SetContents(new ItemStack(blockContainer, 1), base.GetContentStacks(true));
            if (!activeHotbarSlot.Empty && activeHotbarSlot.Itemstack.Collectible.FirstCodePart(0) == "langstrothbroodtop" && activeHotbarSlot.Itemstack.Collectible.Variant["primary"] == material && activeHotbarSlot.Itemstack.Collectible.Variant["accent"] == material2)
            {
                if (this.inv.Empty)
                {
                    this.Api.World.BlockAccessor.SetBlock(this.Api.World.BlockAccessor.GetBlock(new AssetLocation("fromgoldencombs", string.Concat(new string[]
                    {
                        "langstrothbrood-empty-",
                        base.Block.Variant["primary"],
                        "-",
                        base.Block.Variant["accent"],
                        "-",
                        this.block.Variant["side"]
                    }))).BlockId, this.Pos);
                    activeHotbarSlot.TakeOut(1);
                    base.MarkDirty(true, null);
                    return true;
                }
                ICoreClientAPI coreClientAPI = byPlayer.Entity.World.Api as ICoreClientAPI;
                if (coreClientAPI != null)
                {
                    coreClientAPI.TriggerIngameError(this, "nonemptysuper", Lang.Get("fromgoldencombs:nonemptysuper", Array.Empty<object>()));
                }
            }
            else if ((activeHotbarSlot.Empty || !flag) && blockSel.SelectionBoxIndex < 10 && type == "open")
            {
                if (this.TryTake(byPlayer, blockSel))
                {
                    base.MarkDirty(true, null);
                    return true;
                }
            }
            else if (flag && blockSel.SelectionBoxIndex < 10 && type == "open")
            {
                base.MarkDirty(true, null);
                if (this.TryPut(activeHotbarSlot, blockSel))
                {
                    return true;
                }
            }
            else
            {
                if (activeHotbarSlot.Itemstack == null && activeHotbarSlot.StorageType == EnumItemStorageFlags.Backpack && type == "closed" && byPlayer.InventoryManager.TryGiveItemstack(blockContainer.OnPickBlock(this.Api.World, blockSel.Position), false))
                {
                    this.Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                    base.MarkDirty(true, null);
                    return true;
                }
                if (type == "open" && !byPlayer.Entity.Controls.Sneak)
                {
                    type = "closed";
                    base.MarkDirty(true, null);
                    updateMeshes();
                    return true;
                }
                if (type == "closed")
                {
                    type = "open";
                    updateMeshes();
                    base.MarkDirty(true, null);
                    return true;
                }
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

                //updateMeshes();
                return true;
            }

            return false;
        }

        protected override float[][] genTransformationMatrices()
        {
            tfMatrices = new float[Inventory.Count][];
            Cuboidf[] selectionBoxes = type == "open" ? (base.Block as LangstrothSuper).openselectionboxes : (base.Block as LangstrothSuper).closedselectionboxes;
            for (int i = 0; i < selectionBoxes.Length - 1; i++)
            {
                Cuboidf obj = selectionBoxes[i];
                float midX = obj.MidX;
                float midY = 0.069f;
                float midZ = obj.MidZ;
                Vec3f vec3f = new Vec3f(midX, midY, midZ);
                vec3f = new Matrixf().RotateY(MeshAngleRad).TransformVector(vec3f.ToVec4f(0f)).XYZ;
                tfMatrices[i] = new Matrixf().Translate(vec3f.X, vec3f.Y, vec3f.Z).Translate(0.5f, 0f, 0.5f).RotateY(MeshAngleRad - MathF.PI).Values;
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
            tree.SetString("material2", material2);
            tree.SetFloat("meshAngleRad", MeshAngleRad);
            tree.SetBool("usableSlotsDirty", UsableSlots == null);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            type = tree.GetString("type");
            material = tree.GetString("material");
            material2 = tree.GetString("material2");
            MeshAngleRad = tree.GetFloat("meshAngleRad");
            if (tree.GetBool("usableSlotsDirty"))
            {
                UsableSlots = null;
            }

            initLangstrothSuper();
            RedrawAfterReceivingTreeAttributes(worldForResolving);
        }



        public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
        {
            MeshAngleRad = tree.GetFloat("meshAngleRad");
            MeshAngleRad -= (float)degreeRotation * (MathF.PI / 180f);
            tree.SetFloat("meshAngleRad", MeshAngleRad);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
            if (forPlayer.CurrentBlockSelection == null)
            {
                base.GetBlockInfo(forPlayer, sb);
            }
            else if (type == "closed")
            {
                return;
            }
            else if (index == 10)
            {

                sb.AppendLine("");
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
            else if (index < 10)
            {
                ItemSlot slot = inv[index];
                sb.AppendLine("");
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