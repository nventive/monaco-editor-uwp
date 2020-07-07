﻿using Newtonsoft.Json;

namespace Monaco.Editor
{
    public sealed class IRelatedInformation
    {
        [JsonProperty("endColumn")]
        public uint EndColumn { get; set; }

        [JsonProperty("endLineNumber")]
        public uint EndLineNumber { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("resource")]
        public Uri Resource { get; set; }

        [JsonProperty("startColumn")]
        public uint StartColumn { get; set; }

        [JsonProperty("startLineNumber")]
        public uint StartLineNumber { get; set; }
    }
}