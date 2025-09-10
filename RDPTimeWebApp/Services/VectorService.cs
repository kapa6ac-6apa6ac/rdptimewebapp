using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Services
{
    public class VectorService
    {
        private const string _login = "admin";
        private const string _pass = "Admin456";

        private const string _apiBase = "http://vector.snhpro.ru/api/";
        private readonly IHttpClientFactory _clientFactory;
        
        private static Token JWTToken { get; set; }


        public VectorService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<Dictionary<string, int>> GetTimeMonth(DateTime day)
        {
            var d = DateTime.DaysInMonth(day.Year, day.Month);
            return await GetTime(new DateTime(day.Year, day.Month, 1), new DateTime(day.Year, day.Month, d));
        }

        public async Task<Dictionary<string, int>> GetTime(DateTime day)
        {
            return await GetTime(day, day);
        }

        public async Task<Dictionary<string, int>> GetTime(DateTime from, DateTime to)
        {
            if (!await IsAuth())
                await Auth();

            var report = await GetReport(from, to);
            var users = new Dictionary<string, int>();
            foreach (var line in report)
            {
                var d = line.Split('|');
                if (d.Length == 5 && d[4] != "00:00")
                {
                    var domain = d[0] == "snhpro.ru" ? "SNHPRO" : (d[0] == "ufa.snhpro.ru" ? "UFA" : "local");
                    var user = $"{domain}\\{d[1]}";
                    var time = GetMinutes(d[4]);
                    if (users.ContainsKey(user))
                        users[user] += time;
                    else
                        users.Add(user, time);
                }
            }

            return users;
        }

        public async Task<Dictionary<DateTime, int>> GetTimeUser(Models.UserModel user, DateTime from, DateTime to)
        {
            if (!await IsAuth())
                await Auth();

            var report = await GetReport(from, to);
            var days = new Dictionary<DateTime, int>();
            foreach (var line in report)
            {
                var d = line.Split('|');
                if (d.Length == 5)
                {
                    var domain = d[0] == "snhpro.ru" ? "SNHPRO" : (d[0] == "ufa.snhpro.ru" ? "UFA" : "local");
                    var username = $"{domain}\\{d[1]}";
                    if (username == user.Login)
                    {
                        var day = DateTime.Parse(d[3]);
                        var time = GetMinutes(d[4]);
                        days.Add(day, time);
                    }
                }
            }

            return days;
        }

        private static int GetMinutes(string time)
        {
            var s = time.Split(':');
            return int.Parse(s[0]) * 60 + int.Parse(s[1]);
        }

        private async Task<string[]> GetReport(DateTime from, DateTime to)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JWTToken.AccessToken}");

            var response = await client.GetAsync(_apiBase + $"reports/actual-hours?from={from.ToString("yyyy-MM-dd")}&to={to.ToString("yyyy-MM-dd")}");
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadAsStringAsync()).Split('\n');
            }
            return new string[0];
        }

        private async Task Auth()
        {
            if (JWTToken == null)
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(_apiBase + "auth/login", new StringContent(JsonConvert.SerializeObject(new { Username = _login, Password = _pass }), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    JWTToken = JsonConvert.DeserializeObject<Token>(data);
                }
                else
                {
                    throw new Exception("Vector Auth Failed!");
                }
            }
            else
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JWTToken.AccessToken}");
                var response = await client.PostAsync(_apiBase + "auth/refreshToken", new StringContent(JsonConvert.SerializeObject(JWTToken.RefreshToken), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    JWTToken = JsonConvert.DeserializeObject<Token>(data);
                }
                else
                {
                    JWTToken = null;
                    throw new Exception("Vector Auth Failed!");
                }
            }
        }

        private async Task<bool> IsAuth()
        {
            if (JWTToken == null)
                return false;
            
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JWTToken.AccessToken}");
            var response = await client.GetAsync(_apiBase + "auth/authenticated");
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public class Token
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
        }
    }
}
