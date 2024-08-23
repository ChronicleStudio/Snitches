using Snitches.BlockEntities;
using Snitches.Blocks;
using Snitches.Config;
using Snitches.Events;
using Snitches.Violation;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;


namespace Snitches
{
    public class SnitchesModSystem : ModSystem
    {
        ICoreAPI _api;

        public SnitchesServerConfig config;

        public Dictionary<string, List<BlockEntitySnitch>> trackedPlayers;

        public override void StartPre(ICoreAPI api)
        {
            config = new SnitchesServerConfig(api);
            config.Load();
            config.Save();

            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockSnitch", typeof(BlockSnitch));
            api.RegisterBlockEntityClass("BESnitch", typeof(BlockEntitySnitch));

            trackedPlayers = new Dictionary<string, List<BlockEntitySnitch>>();

            _api = api;

            base.Start(api);
        }
       
        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.OnPlayerInteractEntity += SnitchEventsServer.OnPlayerInteractEntity;
            api.Event.OnEntityDeath += SnitchEventsServer.OnEntityDeath;

            api.Event.DidUseBlock += SnitchEventsServer.DidUseBlock;
            api.Event.DidPlaceBlock += SnitchEventsServer.DidPlaceBlock;
            api.Event.DidBreakBlock += SnitchEventsServer.DidBreakBlock;

            base.StartServerSide(api);
        }                

        
        
    }
}
