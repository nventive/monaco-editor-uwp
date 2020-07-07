﻿using Monaco.Editor;
using Monaco.Helpers;
using Newtonsoft.Json;

namespace Monaco.Monaco.Editor
{
    /// <summary>
    /// The initial editor dimension (to avoid measuring the container).
    /// </summary>
    public sealed class Dimension : IDimension
    {
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public uint Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public uint Width { get; set; }
    }
}
