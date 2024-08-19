using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace snitches
{
    public class SnitchPlayer : IPlayer
    {
        public IPlayerRole Role { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PlayerGroupMembership[] Groups => throw new NotImplementedException();

        public List<Entitlement> Entitlements => throw new NotImplementedException();

        public BlockSelection CurrentBlockSelection => throw new NotImplementedException();

        public EntitySelection CurrentEntitySelection => throw new NotImplementedException();

        public string PlayerName => playerName;

        public string playerName;

        public string PlayerUID => playerUID;

        public string playerUID;

        public int ClientId => throw new NotImplementedException();

        public EntityPlayer Entity => throw new NotImplementedException();

        public IWorldPlayerData WorldData => throw new NotImplementedException();

        public IPlayerInventoryManager InventoryManager => throw new NotImplementedException();

        public string[] Privileges => throw new NotImplementedException();

        public bool ImmersiveFpMode => throw new NotImplementedException();

        public PlayerGroupMembership GetGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public PlayerGroupMembership[] GetGroups()
        {
            throw new NotImplementedException();
        }

        public bool HasPrivilege(string privilegeCode)
        {
            throw new NotImplementedException();
        }
    }


    public class BESnitch : BlockEntity
    {
        SnitchPlayer snitchPlayer;

        public int radius, vertRange, trueSightRange;

        public string currentOwnerUID = "";
        public string currentOwnerName = "";

        private Queue<Violation> violationsQueue = new Queue<Violation>();
        private Queue<Violation> tempQueue = new Queue<Violation>();



        public List<string> playersPinged;
        public List<string> playersTracked;

        public SnitchModSystem snitchMod;
        public ModSystemEditableBook bookMod;
        public ModSystemBlockReinforcement reinforceMod;

        long? OnPlayerEnterListenerID;
        long? RecordCrimesListenerID;


        public bool activated = false;

        private SnitchesServerConfig config => Api.ModLoader.GetModSystem<SnitchesModSystem>().config;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            snitchMod = api.ModLoader.GetModSystem<SnitchModSystem>();
            bookMod = api.ModLoader.GetModSystem<ModSystemEditableBook>();
            reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();


            if (Block.Attributes["radius"].Exists)
            {
                radius = Block.Attributes["radius"].AsInt(16);
            }
            else { radius = 16; }

            if (Block.Attributes["vertRange"].Exists)
            {
                vertRange = Block.Attributes["vertRange"].AsInt(16);
            }
            else { vertRange = 16; }

            if (!config.snitchSneakable)
            {
                trueSightRange = radius;
            }
            else if (Block.Attributes["trueSightRange"].Exists)
            {
                trueSightRange = Block.Attributes["trueSightRange"].AsInt(8);
            }
            else { trueSightRange = 8; }

            if (activated)
            {
                TryActivate();
            }

            playersTracked = new List<string>();

        }


        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            foreach (string player in playersTracked)
            {

                foreach (List<BESnitch> snitches in snitchMod.trackedPlayers.Values)
                {
                    if (snitches.Contains(this))
                    {
                        snitches.Remove(this);
                    }
                }
            }

        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            foreach (List<BESnitch> snitches in snitchMod.trackedPlayers.Values)
            {
                if (snitches.Contains(this))
                {
                    snitches.Remove(this);
                }
            }

        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            foreach (List<BESnitch> snitches in snitchMod.trackedPlayers.Values)
            {
                if (snitches.Contains(this))
                {
                    snitches.Remove(this);
                }
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            if (activated)
            {
                dsc.AppendLine("This Snitch is Activated!");
            }

            foreach (string player in playersTracked)
            {
                dsc.AppendLine(player + " currently being tracked!");
            }

            if (violationsQueue.Count > 0)
            {
                dsc.AppendLine(violationsQueue.Count + " violations!");

            }


        }

        public bool OnInteract(IPlayer byPlayer)
        {
            if (Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.CtrlKey && TryActivate())
            {
                currentOwnerUID = byPlayer.PlayerUID;
                currentOwnerName = byPlayer.PlayerName;
                MarkDirty();
                return false;
            };

            if (Api.Side == EnumAppSide.Server & !byPlayer.Entity.Controls.CtrlKey && !byPlayer.Entity.Controls.ShiftKey && activated)
            {
                int counter = 0;
                string tempString = "*----------5 Most Recent Breakins----------*";

                (Api as ICoreServerAPI).SendMessage(byPlayer, 0, tempString, EnumChatType.Notification);
                foreach (Violation violation in tempQueue)
                {
                    counter++;
                    tempString = violation.ToString();
                    (Api as ICoreServerAPI).SendMessage(byPlayer, 0, tempString, EnumChatType.Notification);
                    if (counter >= 5) break;
                }

                return false;

            }

            if (byPlayer.Entity.Controls.CtrlKey)
            {
                if (TryWriteViolations(byPlayer))
                {
                    MarkDirty();
                }
            }

            //if (byPlayer.Entity.Controls.ShiftKey)
            //{
            //    ConnectBook(byPlayer);
            //}

            return true;

        }

        //private bool ConnectBook(IPlayer player)
        //{
        //    if (player == null) return false;

        //    ItemSlot lefthand = player.Entity.LeftHandItemSlot;
        //    ItemSlot righthand = player.Entity.RightHandItemSlot;

        //    if(!(lefthand.Empty || righthand.Empty) && lefthand.Itemstack.Item is ItemTemporalGear && righthand.Itemstack.Item is ItemBook)
        //    {
        //        lefthand.TakeOut(1);
        //        lefthand.MarkDirty();                      

        //        righthand.Itemstack.Attributes.SetBlockPos("SnitchBlockConnection", Pos);
        //    }


        //    return false;
        //}


        public bool TryWriteViolations(IPlayer byPlayer)
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

            int maxLogSize = 50 < violationsQueue.Count ? 50 : violationsQueue.Count;

            for (int i = 0; i < maxLogSize; i++)
            {
                text += (violationsQueue.Dequeue() + "\n");
            }



            bookMod.EndEdit(snitchPlayer, text, title, true);

            return true;

        }

        public bool AddViolation(ITreeAttribute tree, IWorldAccessor world)
        {
            return AddViolation(new Violation(tree, world), true);

        }


        public bool AddViolation(Violation violation, bool fromTree = false)
        {
            if (violation == null) return false;


            tempQueue.Enqueue(violation);
            violationsQueue.Enqueue(violation);

            while (tempQueue.Count > 5) tempQueue.Dequeue();


            MarkDirty();
            return true;
        }

        private void RecordCrimes(float obj)
        {

        }

        private bool TryActivate()
        {

            if (OnPlayerEnterListenerID == null)
            {
                OnPlayerEnterListenerID = RegisterGameTickListener(OnPingPlayers, 50);
            }
            if (RecordCrimesListenerID == null)
            {
                RecordCrimesListenerID = RegisterGameTickListener(RecordCrimes, 6000);
            }
            if (activated)
            {
                MarkDirty();
                return false;
            }

            activated = true;
            MarkDirty();

            return true;
        }


        private void OnPingPlayers(float obj)
        {

            IPlayer[] players = Api.World.GetPlayersAround(Pos.ToVec3d(), radius * radius, vertRange, (IPlayer player) =>
            {
                return ShouldPingPlayer(player);

            });
            playersPinged = new List<string>();

            foreach (IPlayer player in players)
            {

                if (snitchMod.trackedPlayers.TryGetValue(player.PlayerName, out List<BESnitch> snitches))
                {
                    if (!snitches.Contains(this))
                    {
                        snitches.Add(this);
                    }
                }
                else
                {
                    var sn = new List<BESnitch>();
                    sn.Add(this);
                    snitchMod.trackedPlayers.Add(player.PlayerName, sn);
                }
                playersPinged.Add(player.PlayerName);

                if (!playersTracked.Contains(player.PlayerName))
                {
                    playersTracked.Add(player.PlayerName);
                    string date = Api.World.Calendar.PrettyDate();
                    string time = Api.World.Calendar.ElapsedSeconds.ToString();
                    if (Api.Side == EnumAppSide.Server)
                    {
                        AddViolation(new Violation(Violation.TRESSPASSED, player as IServerPlayer, player.Entity.Pos.AsBlockPos, date, time));
                    }
                }

            }

            List<string> ps = new List<string>();

            foreach (string playerName in playersTracked)
            {
                if (playersPinged.Contains(playerName)) { continue; }

                if (snitchMod.trackedPlayers.TryGetValue(playerName, out List<BESnitch> snitches))
                {
                    snitches.Remove(this);

                }

                ps.Add(playerName);

            }

            foreach (string playerName in ps)
            {
                playersTracked.Remove(playerName);
            }


            if (Api.Side == EnumAppSide.Server)
            {
                MarkDirty();
            }

        }

        private bool ShouldPingPlayer(IPlayer player)
        {
            if (player.PlayerUID == currentOwnerUID) return false;

            if (reinforceMod.IsReinforced(Pos))
            {
                if (player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null)
                {
                    return false;
                }
            }

            if (player.Entity.Controls.Sneak && Pos.DistanceTo(player.Entity.Pos.AsBlockPos) > trueSightRange) return false;

            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            activated = tree.GetBool("activated");
            currentOwnerName = tree.GetString("currentOwnerName");
            currentOwnerUID = tree.GetString("currentOwnerUID");
            int violationCount = tree.GetInt("violationCount");
            int tempQueueCount = tree.GetInt("tempQueue");

            var violationTree = tree.GetOrAddTreeAttribute("violations");

            if (activated)
            {
                tempQueue = new Queue<Violation>();
                violationsQueue = new Queue<Violation>();
                for (int counter = 0; counter < violationCount; counter++)
                {

                    ITreeAttribute violation = violationTree.GetTreeAttribute(counter.ToString());
                    AddViolation(violation, worldAccessForResolve);
                }


            }


        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("activated", activated);
            tree.SetString("currentOwnerName", currentOwnerName);
            tree.SetString("currentOwnerUID", currentOwnerUID);
            tree.SetInt("violationCount", violationsQueue.Count);
            tree.SetInt("tempQueue", tempQueue.Count);

            var violationTree = tree.GetOrAddTreeAttribute("violations");

            if (activated && violationsQueue.Count > 0)
            {
                int counter = 0;

                foreach (Violation violation in violationsQueue)
                {
                    ITreeAttribute count = violationTree.GetOrAddTreeAttribute(counter.ToString());
                    violation.ToTreeAttributes(count);

                    counter++;
                }


            }

            if (activated && tempQueue.Count > 0)
            {
                int counter = 0;
                foreach (Violation violation in tempQueue)
                {
                    ITreeAttribute count = violationTree.GetOrAddTreeAttribute("temp" + counter.ToString());
                    violation.ToTreeAttributes(count);

                    counter++;
                }
            }

        }





    }
}
