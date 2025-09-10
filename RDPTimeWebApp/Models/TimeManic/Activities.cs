using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDPTimeWebApp.Models.TimeManic
{
    public class ActivitiesBase
    {
        public partial class Activities
        {
            [JsonProperty("timelineKey")]
            public Guid TimelineKey { get; set; }

            [JsonProperty("publishKey")]
            public Guid PublishKey { get; set; }

            [JsonProperty("owner")]
            public Owner Owner { get; set; }

            [JsonProperty("schema")]
            public Schema Schema { get; set; }

            [JsonProperty("homeEnvironment")]
            public Environment HomeEnvironment { get; set; }

            [JsonProperty("deviceDisplayName")]
            public string DeviceDisplayName { get; set; }

            [JsonProperty("lastUpdate")]
            public LastUpdate LastUpdate { get; set; }

            [JsonProperty("lastChangeId")]
            public string LastChangeId { get; set; }

            [JsonProperty("updateProtocol")]
            public string UpdateProtocol { get; set; }

            [JsonProperty("timestamp")]
            public string Timestamp { get; set; }

            [JsonProperty("links")]
            public Link[] Links { get; set; }

            [JsonProperty("entities")]
            public Entity[] Entities { get; set; }
        }

        public partial class Entity
        {
            [JsonProperty("entityType")]
            public string EntityType { get; set; }

            [JsonProperty("entityId")]
            public long EntityId { get; set; }

            [JsonProperty("values")]
            public Values Values { get; set; }
        }

        public partial class Values
        {
            [JsonProperty("groupId", NullValueHandling = NullValueHandling.Ignore)]
            public long? GroupId { get; set; }

            [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsActive { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("timeInterval", NullValueHandling = NullValueHandling.Ignore)]
            public TimeInterval TimeInterval { get; set; }

            [JsonProperty("remoteClientAddress", NullValueHandling = NullValueHandling.Ignore)]
            public string RemoteClientAddress { get; set; }

            [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
            public string Color { get; set; }

            [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
            public string Key { get; set; }

            [JsonProperty("skipColor", NullValueHandling = NullValueHandling.Ignore)]
            public bool? SkipColor { get; set; }
        }

        public partial class TimeInterval
        {
            [JsonProperty("start")]
            public DateTimeOffset Start { get; set; }

            [JsonProperty("duration")]
            public long Duration { get; set; }
        }

        public partial class Environment
        {
            [JsonProperty("environmentId")]
            public Guid EnvironmentId { get; set; }

            [JsonProperty("deviceName")]
            public string DeviceName { get; set; }
        }

        public partial class LastUpdate
        {
            [JsonProperty("updatedUtcTime")]
            public DateTimeOffset UpdatedUtcTime { get; set; }

            [JsonProperty("environment")]
            public Environment Environment { get; set; }
        }

        public partial class Link
        {
            [JsonProperty("rel")]
            public string Rel { get; set; }

            [JsonProperty("href")]
            public Uri Href { get; set; }
        }

        public partial class Owner
        {
            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }
        }

        public partial class Schema
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("baseSchema")]
            public BaseSchema BaseSchema { get; set; }
        }

        public partial class BaseSchema
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }
        }
    }
}
