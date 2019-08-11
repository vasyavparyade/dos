using System;

using DoOrSave.Core;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

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

            return Convert.ToBase64String(job.ToBson());
        }

        public static Job FromBase64String(this string bson)
        {
            return BsonSerializer.Deserialize<Job>(Convert.FromBase64String(bson));
        }

        public static TJob FromBase64String<TJob>(this string bson) where TJob : Job
        {
            return BsonSerializer.Deserialize<TJob>(Convert.FromBase64String(bson));
        }
    }
}
