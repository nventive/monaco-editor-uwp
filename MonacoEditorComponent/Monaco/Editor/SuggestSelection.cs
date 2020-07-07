﻿using Newtonsoft.Json;
using System;

namespace Monaco.Editor
{

    /// <summary>
    /// The history mode for suggestions.
    /// </summary>
    [JsonConverter(typeof(SuggestSelectionConverter))]
    public enum SuggestSelection { First, RecentlyUsed, RecentlyUsedByPrefix };

    internal class SuggestSelectionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SuggestSelection) || t == typeof(SuggestSelection?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "first":
                    return SuggestSelection.First;
                case "recentlyUsed":
                    return SuggestSelection.RecentlyUsed;
                case "recentlyUsedByPrefix":
                    return SuggestSelection.RecentlyUsedByPrefix;
            }
            throw new Exception("Cannot unmarshal type SuggestSelection");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SuggestSelection)untypedValue;
            switch (value)
            {
                case SuggestSelection.First:
                    serializer.Serialize(writer, "first");
                    return;
                case SuggestSelection.RecentlyUsed:
                    serializer.Serialize(writer, "recentlyUsed");
                    return;
                case SuggestSelection.RecentlyUsedByPrefix:
                    serializer.Serialize(writer, "recentlyUsedByPrefix");
                    return;
            }
            throw new Exception("Cannot marshal type SuggestSelection");
        }
    }
}
