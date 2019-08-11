using System;
using System.IO;

using DoOrSave.Core;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

using Newtonsoft.Json;

using ProtoBuf;

namespace DoOrSave.SQLite
{
    internal static class JobExtensions
    {
        static JobExtensions()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Local));
        }

        public static string ToBase64String(this Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            return JsonConvert.SerializeObject(job);

            // using (var ms = new MemoryStream())
            // {
            //     Serializer.Serialize(ms, job);
            //     return Convert.ToBase64String(ms.ToArray());
            // }
        }

        public static Job FromBase64String(this string bson)
        {
            // using (var ms = new MemoryStream(Convert.FromBase64String(bson)))
            // {
            //     return Serializer.Deserialize<Job>(ms);
            // }
            return JsonConvert.DeserializeObject<Job>(bson);
        }

        public static TJob FromBase64String<TJob>(this string bson) where TJob : Job
        {
            // using (var ms = new MemoryStream(Convert.FromBase64String(bson)))
            // {
            //     return Serializer.Deserialize<TJob>(ms);
            // }
            
            return JsonConvert.DeserializeObject<TJob>(bson);
        }
    }
}
