using iBoxDB.LocalServer;
using Plugins.Countly.Persistance;
using Plugins.Countly.Persistance.Entities;
using UnityEngine;

public class CountlyBoxDbHelper
{
    public static DB BuildDatabase(long dbNumber)
    {
        DB.Root(Application.persistentDataPath);
        var db = new DB(dbNumber);

        db.GetConfig().EnsureTable<RequestEntity>(EntityType.Requests.ToString(), "Id");
        db.GetConfig().EnsureTable<EventEntity>(EntityType.ViewEvents.ToString(), "Id");
        db.GetConfig().EnsureTable<EventEntity>(EntityType.NonViewEvents.ToString(), "Id");
        db.GetConfig().EnsureTable<SegmentEntity>(EntityType.ViewEventSegments.ToString(), "Id");
        db.GetConfig().EnsureTable<SegmentEntity>(EntityType.NonViewEventSegments.ToString(), "Id");
        db.GetConfig().EnsureTable<SegmentEntity>(EntityType.Configs.ToString(), "Id");

        return db;
    }
}