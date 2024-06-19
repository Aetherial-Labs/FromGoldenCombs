using FromGoldenCombs.Blocks;
using FromGoldenCombs.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FromGoldenCombs.BlockEntities
{

    internal class BELangstrothBase : BlockEntity
    {
        readonly InventoryGeneric inv;
        //public override string InventoryClassName => "langstrothbase";

        private Block block;
        //public override InventoryBase Inventory => inv;
        private MeshData mesh;

        private string type;

        private string material;

        private string material2;

        private float[] mat;

        private int[] UsableSlots;

        private Cuboidf[] UsableSelectionBoxes;

        //public override string AttributeTransformCode => "onframerackTransform";

        public float MeshAngleRad { get; set; }

        public string Type => type;

        public string Material => material;
        public string Material2 => material2;

        public BELangstrothBase()
        {
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
                initLangstrothBase();
            }
        }

        private void initLangstrothBase()
        {
            if (Api != null && type != null && base.Block is LangstrothBase)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    mesh = (base.Block as LangstrothBase).GetOrCreateMesh(type, material, material2);
                    mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(MeshAngleRad)
                        .Translate(-0.5f, -0.5f, -0.5f)
                        .Values;
                }

                if (block is LangstrothBase)
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
            int[] slots = (base.Block as LangstrothBase).slots;
            List<int> list = new();
            list.AddRange(slots);
            UsableSlots = list.ToArray();
            Cuboidf[] selectionboxes = (base.Block as LangstrothBase).selectionboxes;
            UsableSelectionBoxes = new Cuboidf[selectionboxes.Length];
            for (int i = 0; i < selectionboxes.Length; i++)
            {
                UsableSelectionBoxes[i] = selectionboxes[i].RotatedCopy(0f, MeshAngleRad * (180f / MathF.PI), 0f, new Vec3d(0.5, 0.5, 0.5));
            }
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            type = byItemStack?.Attributes.GetString("type");
            material = byItemStack?.Attributes.GetString("material");
            material2 = byItemStack?.Attributes.GetString("material2");
            initLangstrothBase();
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
            if (slot.Empty
                     && (int)slot.StorageType == 2
                     && byPlayer.InventoryManager.TryGiveItemstack(block.OnPickBlock(Api.World, blockSel.Position)))
            {
                Api.World.BlockAccessor.SetBlock(0, blockSel.Position);
                MarkDirty(true);
                return true;
            }
            return false;
        }

       

        //protected override float[][] genTransformationMatrices()
        //{
        //    tfMatrices = new float[Inventory.Count][];
        //    Cuboidf[] selectionBoxes = (base.Block as FrameRack).selectionboxes;
        //    for (int i = 0; i < Inventory.Count; i++)
        //    {
        //        Cuboidf obj = selectionBoxes[i];
        //        float midX = obj.MidX;
        //        float midY = 0.069f;
        //        float midZ = obj.MidZ;
        //        Vec3f vec3f = new Vec3f(midX, midY, midZ);
        //        vec3f = new Matrixf().RotateY(MeshAngleRad).TransformVector(vec3f.ToVec4f(0f)).XYZ;
        //        tfMatrices[i] = new Matrixf().Translate(vec3f.X, vec3f.Y, vec3f.Z).Translate(0.5f, 0f, 0.5f).RotateY(MeshAngleRad - MathF.PI).Values;
        //    }

        //    return tfMatrices;
        //}


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

            initLangstrothBase();
            RedrawAfterReceivingTreeAttributes(worldForResolving);
        }

        protected virtual void RedrawAfterReceivingTreeAttributes(IWorldAccessor worldForResolving)
        {
            if (worldForResolving.Side == EnumAppSide.Client && Api != null)
            {
                MarkDirty(redrawOnClient: true);
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
                base.GetBlockInfo(forPlayer, sb);
        }

        public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
        {
            MeshAngleRad = tree.GetFloat("meshAngleRad");
            MeshAngleRad -= (float)degreeRotation * (MathF.PI / 180f);
            tree.SetFloat("meshAngleRad", MeshAngleRad);
        }
    }
}
