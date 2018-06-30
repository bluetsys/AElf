﻿using System.Collections.Generic;
using AElf.Configuration;
using Newtonsoft.Json;

namespace AElf.Kernel.Node.Config
{
    public class MinersInfo : ConfigBase<MinersInfo>
    {
        [JsonProperty("producers")]
        public Dictionary<string, Dictionary<string, string>> Producers { get; set; }

        public MinersInfo()
        {
            Producers = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "1", new Dictionary<string, string>
                    {
                        {"address", "0491c7b8074a634a40bbcc30d90b72a4506b"}
                    }
                },
                {
                    "2", new Dictionary<string, string>
                    {
                        {"address", "040e80d9ea7fd2cfcd07d25d67ecf2568a73"}
                    }
                },
                {
                    "3", new Dictionary<string, string>
                    {
                        {"address", "047ed3ac820d8e8709979c1d7cbd75514f13"}
                    }
                },
                {
                    "4", new Dictionary<string, string>
                    {
                        {"address", "04d64740c44965a5ddd0def56d9addd54e00"}
                    }
                }
            };
        }
    }
}
