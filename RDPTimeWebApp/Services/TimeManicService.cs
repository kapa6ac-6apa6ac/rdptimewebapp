using Newtonsoft.Json;
using RDPTimeWebApp.Models.TimeManic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Services
{
    public class TimeManicService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _clientSalavat;
        private readonly HttpClient _clientUfa;

        private const string _apiBaseSalavat = "http://eros.snhpro.ru:8080/api/";
        // private const string _apiBaseUfa = "http://hecate.ufa.snhpro.ru:8080/api/";

        public TimeManicService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;

            _clientSalavat = _clientFactory.CreateClient("timemanic");
            _clientSalavat.BaseAddress = new Uri(_apiBaseSalavat);

            // _clientUfa = _clientFactory.CreateClient("timemanic");
            // _clientUfa.BaseAddress = new Uri(_apiBaseUfa);
        }

        public async Task<long> GetUserTimeRange(Models.UserModel user, DateTime from, DateTime to)
        {
            long time = 0;

            var timelinesSlv = await GetTimelinesSalavat();
            foreach (var timeline in timelinesSlv.TimelinesArray)
            {
                if (timeline.Schema.Name != "ManicTime/ComputerUsage")
                    continue;

                if (user.Login == timeline.Owner.Username)
                {
                    var activities = await GetActivitesSalavat(timeline.TimelineKey, from, to);
                    foreach (var entity in activities.Entities)
                    {
                        if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
                        {
                            time += entity.Values.TimeInterval.Duration;
                        }
                    }
                }
            }

            // var timelinesUfa = await GetTimelinesUfa();
            // foreach (var timeline in timelinesUfa.TimelinesArray)
            // {
            //     if (timeline.Schema.Name != "ManicTime/ComputerUsage")
            //         continue;

            //     if (user.Login == timeline.Owner.Username)
            //     {
            //         var activities = await GetActivitesUfa(timeline.TimelineKey, from, to);
            //         foreach (var entity in activities.Entities)
            //         {
            //             if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
            //             {
            //                 time += entity.Values.TimeInterval.Duration;
            //             }
            //         }
            //     }
            // }

            return time;
        }

        public async Task<Dictionary<DateTime,long>> GetUserTimePerDay(Models.UserModel user, DateTime from, DateTime to)
        {
            var timelinesSlv = await GetTimelinesSalavat();
            // var timelinesUfa = await GetTimelinesUfa();

            var days = new Dictionary<DateTime, long>();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                long time = 0;
                var date2 = date.AddDays(1);

                foreach (var timeline in timelinesSlv.TimelinesArray)
                {
                    if (timeline.Schema.Name != "ManicTime/ComputerUsage")
                        continue;

                    if (user.Login == timeline.Owner.Username)
                    {
                        var activities = await GetActivitesSalavat(timeline.TimelineKey, date, date2);
                        foreach (var entity in activities.Entities)
                        {
                            if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
                            {
                                time += entity.Values.TimeInterval.Duration;
                            }
                        }
                    }
                }

                // foreach (var timeline in timelinesUfa.TimelinesArray)
                // {
                //     if (timeline.Schema.Name != "ManicTime/ComputerUsage")
                //         continue;

                //     if (user.Login == timeline.Owner.Username)
                //     {
                //         var activities = await GetActivitesUfa(timeline.TimelineKey, date, date2);
                //         foreach (var entity in activities.Entities)
                //         {
                //             if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
                //             {
                //                 time += entity.Values.TimeInterval.Duration;
                //             }
                //         }
                //     }
                // }

                days.Add(date, time);
            }

            return days;
        }

        public async Task<Dictionary<string, long>> GetTimeRange(DateTime from, DateTime to)
        {
            var users = new Dictionary<string, long>();

            //SLV
            var timelinesSlv = await GetTimelinesSalavat();
            foreach (var timeline in timelinesSlv.TimelinesArray)
            {
                if (timeline.Schema.Name != "ManicTime/ComputerUsage")
                    continue;

                var user = timeline.Owner.Username;
                var activities = await GetActivitesSalavat(timeline.TimelineKey, from, to);
                var time = 0L;
                foreach (var entity in activities.Entities)
                {
                    if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
                    {
                        time += entity.Values.TimeInterval.Duration;
                    }
                }
                if (users.ContainsKey(user))
                {
                    users[user] = users[user] + time;
                }
                else
                {
                    users.Add(user, time);
                }
            }

            // var timelinesUfa = await GetTimelinesUfa();
            // foreach (var timeline in timelinesUfa.TimelinesArray)
            // {
            //     if (timeline.Schema.Name != "ManicTime/ComputerUsage")
            //         continue;

            //     var user = timeline.Owner.Username;
            //     var activities = await GetActivitesUfa(timeline.TimelineKey, from, to);
            //     var time = 0L;
            //     foreach (var entity in activities.Entities)
            //     {
            //         if (entity.Values.IsActive.HasValue && entity.Values.IsActive.Value)
            //         {
            //             time += entity.Values.TimeInterval.Duration;
            //         }
            //     }
            //     if (users.ContainsKey(user))
            //     {
            //         users[user] = users[user] + time;
            //     }
            //     else
            //     {
            //         users.Add(user, time);
            //     }
            // }
            return users;
        }

        public async Task<Timelines> GetTimelinesSalavat()
        {
            return JsonConvert.DeserializeObject<Timelines>(await _clientSalavat.GetStringAsync("timelines"));
        }

        public async Task<ActivitiesBase.Activities> GetActivitesSalavat(Guid timelineKey, DateTime from, DateTime to)
        {
            return JsonConvert.DeserializeObject<ActivitiesBase.Activities>(await _clientSalavat.GetStringAsync($"timelines/{timelineKey}/activities?fromTime={from.ToString("yyyy-MM-ddTHH:mm:ss")}&toTime={to.ToString("yyyy-MM-ddTHH:mm:ss")}"));
        }

        // public async Task<Timelines> GetTimelinesUfa()
        // {
        //     return JsonConvert.DeserializeObject<Timelines>(await _clientUfa.GetStringAsync("timelines"));
        // }

        // public async Task<ActivitiesBase.Activities> GetActivitesUfa(Guid timelineKey, DateTime from, DateTime to)
        // {
        //     return JsonConvert.DeserializeObject<ActivitiesBase.Activities>(await _clientUfa.GetStringAsync($"timelines/{timelineKey}/activities?fromTime={from.ToString("yyyy-MM-ddTHH:mm:ss")}&toTime={to.ToString("yyyy-MM-ddTHH:mm:ss")}"));
        // }
    }
}
