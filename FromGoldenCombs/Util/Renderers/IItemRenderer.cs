using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Util.Renderers
{
    internal interface IItemRenderer
    {
        public MeshData createMesh(ItemStack stack, int index);
        public bool canHandle(ItemStack stack);
        public int getPriority();
        public EnumItemClass getRendererClass();
        public bool shouldCache(ItemStack stack);


    }
}
