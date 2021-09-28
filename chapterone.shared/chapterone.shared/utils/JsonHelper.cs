using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System;

namespace chapterone.shared.utils
{
    /// <summary>
    /// A helper class containing extension methods useful for converting to and from json
    /// </summary>
    public static class JsonHelper
    {
        private static JsonSerializerSettings _jsonSettings = null;

        /// <summary>
        /// Json serializer settings configured for interpretting nodatime types
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_jsonSettings == null)
                {
                    _jsonSettings = new JsonSerializerSettings();
                    _jsonSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                }

                return _jsonSettings;
            }
        }

        /// <summary>
        /// Converts this object to json
        /// </summary>
        public static string ToJsonString(this Object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        }


        /// <summary>
        /// Converts this json string to an object of type T
        /// </summary>
        public static T FromJsonString<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
        }
    }
}
