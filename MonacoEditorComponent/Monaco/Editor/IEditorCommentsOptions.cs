﻿using Newtonsoft.Json;

namespace Monaco.Editor
{
    /// <summary>
    /// Configuration options for editor comments
    /// </summary>
    public sealed class IEditorCommentsOptions
    {
        /// <summary>
        /// Insert a space after the line comment token and inside the block comments tokens.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("insertSpace", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InsertSpace { get; set; }
    }
}
