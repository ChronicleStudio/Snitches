using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace snitches
{
    internal class BlockSnitch : Block
    {
        WorldInteraction[] interactions;

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BESnitch snitch = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BESnitch;

            return snitch?.OnInteract(byPlayer) == true;

        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            List<ItemStack> bookStackList = new List<ItemStack>();

            foreach (Item item in api.World.Items)
            {
                if (item.Code == null) continue;

                if (item is ItemBook)
                {
                    bookStackList.Add(new ItemStack(item));
                }

            }

            interactions = ObjectCacheUtil.GetOrCreate(api, "snitchBlockInteractions", () =>
            {
                return new WorldInteraction[]{
                    new WorldInteraction() {
                        ActionLangCode = "blockhelp-snitch-activate",
                        HotKeyCode = "ctrl",
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) => {
                            BESnitch bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BESnitch;
                            return bes?.activated != true;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-snitch-5mostrecentviolations",
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) => {
                            BESnitch bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BESnitch;
                            return bes?.activated == true;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-snitch-writeviolations",
                        HotKeyCode = "ctrl",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = bookStackList.ToArray(),

                        ShouldApply = (wi, bs, es) => {
                            BESnitch bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BESnitch;
                            return bes?.activated == true;
                        }
                    }
                };

            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
