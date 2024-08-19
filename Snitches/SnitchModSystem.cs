using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace snitches
{
    public class SnitchModSystem : ModSystem
    {
        ICoreAPI _api;

        public Dictionary<string, List<BESnitch>> trackedPlayers;

        public override void Start(ICoreAPI api)
        {

            _api = api;

            trackedPlayers = new Dictionary<string, List<BESnitch>>();

            base.Start(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.OnPlayerInteractEntity += Event_OnPlayerInteractEntity;
            api.Event.OnEntityDeath += Event_OnEntityDeath;

            api.Event.DidUseBlock += Event_DidUseBlock;
            api.Event.DidPlaceBlock += Event_DidPlaceBlock;
            api.Event.DidBreakBlock += Event_DidBreakBlock;


            base.StartServerSide(api);
        }



        private void Event_DidBreakBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel)
        {

            if (trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BESnitch> snitches))
            {
                if (snitches != null)
                {
                    foreach (BESnitch s in snitches)
                    {
                        if (_api.World.BlockAccessor.GetBlock(blockSel.Position).Id == oldblockId) { return; }

                        string date = _api.World.Calendar.PrettyDate();
                        string time = _api.World.Calendar.ElapsedSeconds.ToString();
                        s.AddViolation(new Violation(Violation.BLOCK_BROKE, byPlayer, blockSel.Position, date, time, _api.World.GetBlock(oldblockId)));
                    }
                }
            }

        }

        private void Event_DidPlaceBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel, ItemStack withItemStack)
        {
            ItemSlot itemSlot = byPlayer?.InventoryManager?.GetHotbarInventory()?[10];
            EnumHandHandling handling = EnumHandHandling.Handled;
            BlockPos position = blockSel.Position;
            (itemSlot?.Itemstack?.Item as ItemPlumbAndSquare)?.OnHeldInteractStart(itemSlot, byPlayer.Entity, blockSel, null, firstEvent: true, ref handling);

            if (trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BESnitch> snitches))
            {
                if (snitches != null)
                {
                    foreach (BESnitch s in snitches)
                    {
                        string date = _api.World.Calendar.PrettyDate();
                        string time = _api.World.Calendar.ElapsedSeconds.ToString();
                        s.AddViolation(new Violation(Violation.BLOCK_PLACED, byPlayer, blockSel.Position, date, time, withItemStack.Block));
                    }
                }
            }

        }

        private void Event_DidUseBlock(IServerPlayer byPlayer, BlockSelection blockSel)
        {
            if (trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BESnitch> snitches))
            {
                if (snitches != null)
                {
                    foreach (BESnitch s in snitches)
                    {
                        string date = _api.World.Calendar.PrettyDate();
                        string time = _api.World.Calendar.ElapsedSeconds.ToString();
                        s.AddViolation(new Violation(Violation.BLOCK_USED, byPlayer, blockSel.Position, date, time, _api.World.GetBlockAccessor(false, false, false).GetBlock(blockSel.Position)));
                    }
                }
            }

        }

        private void Event_OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            if (damageSource == null) return;
            if (damageSource.GetCauseEntity() == null || !(damageSource.GetCauseEntity() is EntityPlayer)) return;
            IServerPlayer byPlayer = (damageSource.SourceEntity as EntityPlayer).Player as IServerPlayer;

            if (trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BESnitch> snitches))
            {
                if (snitches != null)
                {
                    foreach (BESnitch s in snitches)
                    {
                        string date = _api.World.Calendar.PrettyDate();
                        string time = _api.World.Calendar.ElapsedSeconds.ToString();
                        s.AddViolation(new Violation(Violation.ENTITY_KILLED, byPlayer, entity.Pos.AsBlockPos, date, time, null, entity));
                    }
                }
            }

        }

        private void Event_OnPlayerInteractEntity(Entity entity, IPlayer byPlayer, ItemSlot slot, Vintagestory.API.MathTools.Vec3d hitPosition, int mode, ref EnumHandling handling)
        {

        }



    }
}
