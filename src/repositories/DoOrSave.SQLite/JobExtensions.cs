using System;

using DoOrSave.Core;

using Newtonsoft.Json;

namespace DoOrSave.SQLite
{
    internal static class JobExtensions
    {
        static JobExtensions()
        {
        }

        public static string ToBase64String(this Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            return JsonConvert.SerializeObject(job, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static Job FromBase64String(this string bson)
        {
            return JsonConvert.DeserializeObject<Job>(bson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static TJob FromBase64String<TJob>(this string bson) where TJob : Job
        {
            return JsonConvert.DeserializeObject<TJob>(bson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
