﻿using Newtonsoft.Json;

namespace Monaco.Languages
{
    /// <summary>
    /// A command that should be run upon acceptance of this item.
    /// </summary>
    public sealed class Command
    {
        [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Arguments { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tooltip", NullValueHandling = NullValueHandling.Ignore)]
        public string Tooltip { get; set; }
    }
}
