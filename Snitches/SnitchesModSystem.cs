using Vintagestory.API.Common;

namespace snitches
{
    public class SnitchesModSystem : ModSystem
    {
        ICoreAPI _api;

        internal SnitchesServerConfig config;

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
            api.RegisterBlockEntityClass("BESnitch", typeof(BESnitch));

            _api = api;

            base.Start(api);
        }
    }
}
