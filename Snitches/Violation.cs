using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace snitches
{
    
        
    public class Violation
    {
        public const string TRESSPASSED = "tresspassed ";
        public const string ESCAPED = "escaped ";

        public const string BLOCK_USED = "used ";
        public const string BLOCK_PLACED = "placed ";
        public const string BLOCK_BROKE = "broke ";

        public const string ENTITY_INTERACTED = "interacted with ";
        public const string ENTITY_HIT = "hit ";
        public const string ENTITY_KILLED = "killed ";
        public const string ENTITY_SPAWNED = "spawned ";

        public const string COLLECTIBLE_TOOK = "took ";
        public const string COLLECTIBLE_PICKEDUP = "picked up ";
        public const string COLLECTIBLE_DROPPED = "dropped ";


        ICoreAPI api;

       
        public string violationType { get; private set; }

        
        public string date { get; private set; }

        public string time { get; private set; }
        public string playerName { get; private set; }
        public string playerUID { get; private set; }
        public BlockPos pos { get; private set; }

        public string blockName { get; private set; }

        public string entityName { get; private set; }

        public string collectibleName { get; private set; }

        public string violationFinal = null;

        public Violation(string violationType, IServerPlayer player, BlockPos pos, string date, string time, Block block = null, Entity entity = null, CollectibleObject colObj = null)
        {
            api = player.Entity.Api;
            
            this.violationType = violationType;
            this.playerName = player.PlayerName;
            this.playerUID = player.PlayerUID;

            this.pos = pos;
            blockName = block?.GetPlacedBlockName(player.Entity.World, pos);
            entityName = entity?.Code.Domain + ":item-creature-" + entity?.Code.Path;
            collectibleName = colObj?.Code.GetName();

            this.date = date;
            this.time = time;

        }

        public Violation(string violationType, EntityPlayer player, BlockPos pos, string date, string time, Block block = null, Entity entity = null, CollectibleObject colObj = null) {

            api = player.Api;

            this.violationType = violationType;
            this.playerName = player.Player.PlayerName;
            this.playerUID = player.PlayerUID;

            this.pos = pos;
            blockName = block?.GetPlacedBlockName(player.World, pos);
            entityName = entity?.Code.Domain + ":item-creature-" + entity?.Code.Path;
            collectibleName = colObj?.Code.GetName();

            this.date = date;
            this.time = time;
        }

        public Violation(ITreeAttribute tree, IWorldAccessor world)
        {
            playerName = tree.GetString("playername");
            playerUID = tree.GetString("playerUID");
            api = world.Api;

            

            violationType = tree.GetString("violationType");
            date = tree.GetString("date");
            time = tree.GetString("time");
            pos = tree.GetBlockPos("pos");
            blockName = tree.GetString("blockName");
            entityName = tree.GetString("entityName");
            collectibleName = tree.GetString("collectibleName");
            violationFinal = tree.GetString("violationFinal");
        }

        

        public void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("playername", playerName);
            tree.SetString("playerUID", playerUID);
            //tree.SetString("violationType", type.ToString());
            tree.SetString("violationType", violationType);
            tree.SetString("date", date);
            tree.SetString("time", time);
            tree.SetBlockPos("pos", pos);
            tree.SetString("blockName", blockName);
            tree.SetString("entityName", entityName);
            tree.SetString("collectibleName", collectibleName);
            tree.SetString("violationFinal", violationFinal);
        }

        public override string ToString()
        {
            if(violationFinal == null)
            {
                string temp = date + " " + playerName + " " + violationType;

                switch (violationType)
                {
                    case TRESSPASSED:
                        
                    case ESCAPED:
                        temp += "at: " + pos.ToLocalPosition(api).X + ", " + pos.ToLocalPosition(api).Y + ", " + pos.ToLocalPosition(api).Z;// + "\n";
                        break;

                    case BLOCK_USED:
                        
                    case BLOCK_PLACED:
                                
                    case BLOCK_BROKE:
                        temp += (Lang.GetMatching(blockName) + " at: " + pos.ToLocalPosition(api).X + ", " + pos.ToLocalPosition(api).Y + ", " + pos.ToLocalPosition(api).Z); // + "\n");
                        break;


                    case ENTITY_INTERACTED:

                        
                    case ENTITY_HIT:

                        
                    case ENTITY_KILLED:

                        
                    case ENTITY_SPAWNED:
                        temp += (Lang.GetMatching(entityName) + " at: " + pos.ToLocalPosition(api).X + ", " + pos.ToLocalPosition(api).Y + ", " + pos.ToLocalPosition(api).Z); // + "\n");
                        break;

                    case COLLECTIBLE_TOOK: 
                                                
                    case COLLECTIBLE_PICKEDUP:
                                                
                    case COLLECTIBLE_DROPPED:
                        temp += (Lang.GetMatching("item-" + collectibleName) + " at: " + pos.ToLocalPosition(api).X + ", " + pos.ToLocalPosition(api).Y + ", " + pos.ToLocalPosition(api).Z); // + "\n");
                        break;


                    default:
                        break;
                    
                }
           
                
                violationFinal = temp;
            }


            return violationFinal;

        }


    }
}
