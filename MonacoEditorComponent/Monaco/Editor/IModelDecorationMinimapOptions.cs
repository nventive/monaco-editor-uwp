﻿using Newtonsoft.Json;

namespace Monaco.Editor
{
    public sealed class IModelDecorationMinimapOptions
    {
        /// <summary>
        /// CSS color to render.
        /// e.g.: rgba(100, 100, 100, 0.5) or a color from the color registry
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        /// <summary>
        /// CSS color to render.
        /// e.g.: rgba(100, 100, 100, 0.5) or a color from the color registry
        /// </summary>
        [JsonProperty("darkColor", NullValueHandling = NullValueHandling.Ignore)]
        public string DarkColor { get; set; }

        /// <summary>
        /// The position in the overview ruler.
        /// </summary>
        [JsonProperty("position")]
        public int Position { get; set; }
    }
}