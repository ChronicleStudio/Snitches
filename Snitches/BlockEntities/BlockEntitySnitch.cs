using Snitches.Config;
using Snitches.Players;
using Snitches.Violation;
using Snitches.Violations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Snitches.BlockEntities
{
    public class BlockEntitySnitch : BlockEntity
    {
        SnitchPlayer snitchPlayer;

        public int Radius { get; private set; }
        public int VertRange { get; private set; }
        public int TrueSightRange { get; private set; }
        public int MaxPaperLog { get; private set; }
        public int MaxBookLog { get; private set; }
        public bool Sneakable { get; private set; }
        public string CurrentOwnerUID { get; private set; }
		       
		public int violationCount { get; set; }

        private List<string> playersPinged;
        private List<string> playersTracked;

        private SnitchesModSystem snitchMod;
        private ModSystemEditableBook bookMod;
        private ModSystemBlockReinforcement reinforceMod;

        long? OnPlayerEnterListenerID;

		public bool Activated { get; private set; } = false;

        private SnitchesServerConfig config => Api.ModLoader.GetModSystem<SnitchesModSystem>().config;

		ViolationLogger violationLogger;

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
		{
			base.FromTreeAttributes(tree, worldAccessForResolve);
		    Activated = tree.GetBool("activated");
			
			CurrentOwnerUID = tree.GetString("currentOwnerUID");				

			int playersTrackedCount = tree.GetInt("playersTrackedCount");
						
			var playersTrackedTree = tree.GetOrAddTreeAttribute("playersTracked");

			violationCount = tree.GetInt("violationCount");
					
			if (Activated)
			{
				playersTracked = new List<string>();

				for (int counter = 0; counter < playersTrackedCount; counter++)
				{
					playersTracked.Add(playersTrackedTree.GetString("player" + counter));
				}

			}
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetBool("activated", Activated);
			tree.SetString("currentOwnerUID", CurrentOwnerUID);				
			tree.SetInt("playersTrackedCount", playersTracked.Count);
			tree.SetInt("violationCount", violationCount);
			
			var playersTrackedTree = tree.GetOrAddTreeAttribute("playersTracked");	
			
			if (Activated && playersTracked.Count > 0)
			{
				int counter = 0;
				foreach (var p in playersTracked)
				{
					//ITreeAttribute player = playersTrackedTree.GetOrAddTreeAttribute("player" + counter.ToString());
					playersTrackedTree.SetString("player" + counter, p);
				}

			}

		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			snitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();
			bookMod = api.ModLoader.GetModSystem<ModSystemEditableBook>();
			reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();

			Radius = config.snitchRadius;
			VertRange = config.snitchVert;

			Sneakable = config.snitchSneakable;
			TrueSightRange = Sneakable == true ? (int)(Radius * config.snitchTruesightRange) : Radius;

			MaxBookLog = config.maxBookLog;
			MaxPaperLog = config.maxPaperLog;

			if (Activated)
			{
				TryActivate();
			}

			playersTracked = new List<string>();

			if (api.Side == EnumAppSide.Server)
			{
				violationLogger = new ViolationLogger(this, api as ICoreServerAPI);
			}

		}
		public override void OnBlockUnloaded()
		{
			base.OnBlockUnloaded();
			RemoveSnitches();
		}

		public override void OnBlockRemoved()
		{
			base.OnBlockRemoved();
			RemoveSnitches();	
		}

		public override void OnBlockBroken(IPlayer byPlayer = null)
		{
			base.OnBlockBroken(byPlayer);
			RemoveSnitches();			
		}

		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
		{
			base.GetBlockInfo(forPlayer, dsc);

			if (Activated)
			{
				dsc.AppendLine("This Snitch is Activated!");
			}

			foreach (string player in playersTracked)
			{
				dsc.AppendLine(player + " currently being tracked!");
			}

			dsc.AppendLine($"Current Violations: {violationCount}");

		}

		public bool OnInteract(IPlayer byPlayer)
		{
			if (Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.ShiftKey && TryActivate())
			{
				CurrentOwnerUID = byPlayer.PlayerUID;				
				MarkDirty();
				return false;
			};			

			if (Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.CtrlKey)
			{
				if (TryWriteViolations(byPlayer))
				{
					MarkDirty();
				}
			}

			return true;

		}
		// Maybe allow callback to allow the interact to happen after Book log is pulled
		private bool TryWriteViolations(IPlayer byPlayer)
		{
			snitchPlayer = new SnitchPlayer()
			{
				playerName = "Snitch_" + Pos.ToLocalPosition(Api).ToString(),
				playerUID = "Snitch_" + Pos.ToLocalPosition(Api).ToString()
			};

			ItemSlot bookSlot = byPlayer.Entity.ActiveHandItemSlot;
			ItemSlot penSlot = byPlayer.Entity.LeftHandItemSlot;
			if (!(bookSlot.Itemstack?.Item is ItemBook)) return false;
			if (penSlot.Empty || !(penSlot.Itemstack.Class == EnumItemClass.Item)) return false;
			if (!(penSlot.Itemstack.Item.Attributes["writingTool"].Exists) || penSlot.Itemstack.Item.Attributes["writingTool"].AsBool() == false)
			{
				return false;
			}

			bookMod.BeginEdit(snitchPlayer, bookSlot);

			string text = "";
			string title = "Violations pulled on " + Api.World.Calendar.PrettyDate();
			int maxLogSize = 0;

			if (bookSlot.Itemstack.Collectible.Code.ToString().Contains("parchment")) maxLogSize = MaxPaperLog;
			if (bookSlot.Itemstack.Collectible.Code.ToString().Contains("book")) maxLogSize = MaxBookLog;

			var log = violationLogger.GetViolations(maxLogSize);

			int tempCount = log.Count - 1;
			for (int i = 0; (i <= maxLogSize && i <= tempCount); i++)
			{
				text += (log.Dequeue().LogbookFormat(Api) + "\n");				
			}

			bookMod.EndEdit(snitchPlayer, text, title, true);
			

			return true;
		}

		public void AddViolation(SnitchViolation violation)
		{
			violationLogger.AddViolation(violation);			
		}

		private bool TryActivate()
		{
			if (OnPlayerEnterListenerID == null)
			{
				OnPlayerEnterListenerID = RegisterGameTickListener(OnPingPlayers, 50);
			}
			if (Activated)
			{
				MarkDirty();
				return false;
			}

			Activated = true;
			MarkDirty();

			return true;
		}

		private void OnPingPlayers(float obj)
		{

			List<IPlayer> players = Api.World.GetPlayersAround(Pos.ToVec3d(), Radius, VertRange, (IPlayer player) =>
			{
				return ShouldPingPlayer(player);

			}).ToList<IPlayer>();
			playersPinged = new List<string>();

			foreach (IPlayer player in players)
			{

				if (snitchMod.trackedPlayers.TryGetValue(player.PlayerUID, out List<BlockEntitySnitch> snitches))
				{
					if (!snitches.Contains(this))
					{
						snitches.Add(this);
					}
				}
				else
				{
					var sn = new List<BlockEntitySnitch>();
					sn.Add(this);
					snitchMod.trackedPlayers.Add(player.PlayerUID, sn);
				}
				playersPinged.Add(player.PlayerUID);

				if (!playersTracked.Contains(player.PlayerUID))
				{
					playersTracked.Add(player.PlayerUID);
					
					if (Api.Side == EnumAppSide.Server)
					{
						string prettyDate = Api.World.Calendar.PrettyDate();
						double day = Api.World.Calendar.ElapsedDays;
						long time = Api.World.Calendar.ElapsedSeconds;
						int year = Api.World.Calendar.Year;
						AddViolation(new SnitchViolation(EnumViolationType.Trespassed, player as IServerPlayer, player.Entity.Pos.AsBlockPos, prettyDate, day, time, year));
					}
				}
			}

			List<string> ps = new List<string>();

			foreach (string playerUID in playersTracked)
			{
				if (playersPinged.Contains(playerUID)) { continue; }

				if (snitchMod.trackedPlayers.TryGetValue(playerUID, out List<BlockEntitySnitch> snitches))
				{
					var player = Api.World.PlayerByUid(playerUID);

					string prettyDate = Api.World.Calendar.PrettyDate();
					double day = Api.World.Calendar.ElapsedDays;
					long time = Api.World.Calendar.ElapsedSeconds;
					int year = Api.World.Calendar.Year;
					AddViolation(new SnitchViolation(EnumViolationType.Escaped, player as IServerPlayer, player.Entity.Pos.AsBlockPos, prettyDate, day, time, year));

					snitches.Remove(this);
				}
				ps.Add(playerUID);
			}

			foreach (string playerUID in ps)
			{
				playersTracked.Remove(playerUID);
			}

			if (Api.Side == EnumAppSide.Server)
			{
				MarkDirty();
			}

		}

		private bool ShouldPingPlayer(IPlayer player)
		{
			//if (player.PlayerUID == CurrentOwnerUID) return false;

			if (reinforceMod.IsReinforced(Pos))
			{
				if (player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null)
				{
					return false;
				}
			}

			if (Sneakable && player.Entity.Controls.Sneak && Pos.DistanceTo(player.Entity.Pos.AsBlockPos) > TrueSightRange) return false;

			return true;
		}

		private void RemoveSnitches()
		{
			foreach (List<BlockEntitySnitch> snitches in snitchMod.trackedPlayers.Values)
			{
				if (snitches.Contains(this))
				{
					snitches.Remove(this);
				}
			}
		}

		
	}
}
