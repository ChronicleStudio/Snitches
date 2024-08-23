using Snitches.BlockEntities;
using Snitches.Violation;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace Snitches.Events
{
	public static class SnitchEventsServer
	{
		public static void DidBreakBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel)
		{
			ICoreAPI sapi = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						// Check if reinforced and add Reinforcement Breaking Violation
						if (sapi.World.BlockAccessor.GetBlock(blockSel.Position).Id == oldblockId)
						{
							SnitchViolation violation = new SnitchViolation(EnumViolationType.ReinforcementBroke, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlock(oldblockId));

							s.AddViolation(violation);

							

						}
						else
						{
							SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockBroke, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlock(oldblockId));

							s.AddViolation(violation);
							
						}
					}
				}
			}
		}

		public static void DidPlaceBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel, ItemStack withItemStack)
		{
			ICoreAPI sapi = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			ItemSlot itemSlot = byPlayer?.InventoryManager?.GetHotbarInventory()?[10];
			EnumHandHandling handling = EnumHandHandling.Handled;
			BlockPos position = blockSel.Position;
			(itemSlot?.Itemstack?.Item as ItemPlumbAndSquare)?.OnHeldInteractStart(itemSlot, byPlayer.Entity, blockSel, null, firstEvent: true, ref handling);

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockPlaced, byPlayer, blockSel.Position, prettyDate, day, time, year, withItemStack.Block);

						s.AddViolation(violation);
						
					}
				}
			}
		}

		public static void DidUseBlock(IServerPlayer byPlayer, BlockSelection blockSel)
		{			
			ICoreAPI sapi = byPlayer.Entity.Api;					

			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockUsed, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlockAccessor(false, false, false).GetBlock(blockSel.Position));

						s.AddViolation(violation);
						
					}
				}
			}
		}

		private static void UseBlockHelper(float obj)
		{
			throw new NotImplementedException();
		}

		public static void OnEntityDeath(Entity entity, DamageSource damageSource)
		{		

			if (damageSource == null) return;
			if (damageSource.GetCauseEntity() == null || !(damageSource.GetCauseEntity() is EntityPlayer)) return;
			IServerPlayer byPlayer = (damageSource.SourceEntity as EntityPlayer).Player as IServerPlayer;
			ICoreAPI sapi = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerName, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.EntityKilled, byPlayer, entity.Pos.AsBlockPos, prettyDate, day, time, year, null, entity);

						s.AddViolation(violation);						
					}
				}
			}
		}

		public static void OnPlayerInteractEntity(Entity entity, IPlayer byPlayer, ItemSlot slot, Vec3d hitPosition, int mode, ref EnumHandling handling)
		{
			throw new NotImplementedException();
		}
	}
}
