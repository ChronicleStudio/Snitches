using System;
using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace snitches
{
    [JsonObject()]
    internal class SnitchesServerConfig
    {
        [JsonIgnore]
        private ICoreAPI sapi;

        [JsonProperty]
        public bool snitchSneakable = false;
        [JsonProperty]
        public int snitchRadius = 16;
        [JsonProperty]
        public int snitchVert = 16;
        [JsonProperty]
        public float snitchTruesightRange = 0.5f;
        [JsonProperty]
        public int maxSnitchLog = 1000;
        [JsonProperty]
        public int maxBookLog = 500;
        [JsonProperty]
        public int maxPaperLog = 100;

        public SnitchesServerConfig(ICoreAPI api)
        {
            sapi = api;
        }

        public void Save()
        {
            sapi.StoreModConfig(this, "Snitches/server.json");
        }

        public void Load()
        {
            try
            {
                SnitchesServerConfig vintageRealmsServerConfig = sapi.LoadModConfig<SnitchesServerConfig>("Snitches/server.json") ?? new SnitchesServerConfig(sapi);
                snitchSneakable = vintageRealmsServerConfig.snitchSneakable;
                snitchRadius = vintageRealmsServerConfig.snitchRadius;
                snitchVert = vintageRealmsServerConfig.snitchVert;
                snitchTruesightRange = vintageRealmsServerConfig.snitchTruesightRange;
                maxSnitchLog = vintageRealmsServerConfig.maxSnitchLog;
                maxBookLog = vintageRealmsServerConfig.maxBookLog;
                maxPaperLog = vintageRealmsServerConfig.maxPaperLog;

            }
            catch (Exception ex)
            {
                sapi.Logger.Error("Malformed ModConfig file VintageRealms/server.json, Exception: \n {0}", ex.StackTrace);
            }
        }
    }
}
