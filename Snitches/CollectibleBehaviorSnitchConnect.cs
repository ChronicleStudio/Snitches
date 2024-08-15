using snitches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace snitches
{
    internal class CollectibleBehaviorSnitchConnect : CollectibleBehavior
    {
        public CollectibleBehaviorSnitchConnect(CollectibleObject collObj) : base(collObj)
        {
        }

        //public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        //{
        //    base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);

        //    if (slot.Itemstack.Attributes.HasAttribute("SnitchBlockConnection"))
        //    {
        //        handling = EnumHandling.PreventSubsequent;
                

        //    }

        //}

        //public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
        //{
        //    return base.OnHeldAttackStep(secondsPassed, slot, byEntity, blockSelection, entitySel, ref handling);
        //}

        //public override void OnHeldAttackStop(float secondsPassed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, ref EnumHandling handling)
        //{
        //    base.OnHeldAttackStop(secondsPassed, slot, byEntity, blockSelection, entitySel, ref handling);
        //}


        //public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        //{
        //    base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);

        //    if (slot.Itemstack.Attributes.HasAttribute("SnitchBlockConnection"))
        //    {
        //        BlockPos snitchPos = slot.Itemstack.Attributes.GetBlockPos("SnitchBlockConnection");
        //        if(byEntity.Pos.AsBlockPos.DistanceTo(snitchPos) < 20)
        //        {
        //            BESnitch snitch = byEntity.World.BlockAccessor.GetBlockEntity<BESnitch>(snitchPos);
        //            if(snitch != null)
        //            {
        //                IPlayer player = (byEntity as EntityPlayer).Player;

        //                snitch.TryWriteViolations(player);
        //            }
        //        }
        //    }
        //}

        //public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        //{
        //    return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
        //}

        //public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        //{
        //    base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
        //}
    }
}
