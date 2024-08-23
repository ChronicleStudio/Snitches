using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace Snitches.Config
{
    [JsonObject()]
    public class SnitchesServerConfig
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
        public int maxSnitchLog = 100000;
        [JsonProperty]
        public int maxBookLog = 25;
        [JsonProperty]
        public int maxPaperLog = 5;

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
                SnitchesServerConfig snitchesServerConfig = sapi.LoadModConfig<SnitchesServerConfig>("Snitches/server.json") ?? new SnitchesServerConfig(sapi);
                snitchSneakable = snitchesServerConfig.snitchSneakable;
                snitchRadius = snitchesServerConfig.snitchRadius;
                snitchVert = snitchesServerConfig.snitchVert;
                snitchTruesightRange = snitchesServerConfig.snitchTruesightRange;
                maxSnitchLog = snitchesServerConfig.maxSnitchLog;
                maxBookLog = snitchesServerConfig.maxBookLog;
                maxPaperLog = snitchesServerConfig.maxPaperLog;

            }
            catch (Exception ex)
            {
                sapi.Logger.Error("Malformed ModConfig file VintageRealms/server.json, Exception: \n {0}", ex.StackTrace);
            }
        }
    }
}
