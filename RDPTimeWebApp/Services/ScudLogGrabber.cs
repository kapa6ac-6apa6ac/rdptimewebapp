using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RDPTimeWebApp.Services.ScudLogGrabber.ScudLog;

namespace RDPTimeWebApp.Services
{
    public class ScudLogGrabber
    {
        private readonly OrionContext _orion;
        private readonly SigurLogsContext _sigur;
        private readonly AppDbContext _context;
        private readonly int[] _doorsSlv;
        private readonly int[] _doorsUfa;
        private readonly CalendarService _calendar;

        public ScudLogGrabber(OrionContext orion, SigurLogsContext sigur, AppDbContext context, CalendarService calendar, IConfiguration configuration)
        {
            _orion = orion;
            _sigur = sigur;
            _context = context;
            _doorsSlv = configuration.GetSection("SCUD:Doors:Salavat").Get<int[]>();
            _doorsUfa = configuration.GetSection("SCUD:Doors:Ufa").Get<int[]>();
            _calendar = calendar;
        }

        /// <summary>
        /// Получает все логи за день
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllDay(DateTime date)
        {
            var slvLogs = await GetLogsSlv(l => l.TimeVal.Date == date.Date);
            var ufaLogs = await GetLogsUfa(l => l.LogTime.Value.Date == date.Date);
            return new List<ScudLog>().Concat(slvLogs).Concat(ufaLogs).OrderBy(l => l.Time).ToList();
        }

        /// <summary>
        /// Получает все логи за период
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllPeriod(DateTime from, DateTime to)
        {
            var slvLogs = await GetLogsSlv(l => l.TimeVal.Date >= from.Date && l.TimeVal.Date <= to.Date);
            var ufaLogs = await GetLogsUfa(l => l.LogTime.Value.Date >= from.Date && l.LogTime.Value.Date <= to.Date);
            return new List<ScudLog>().Concat(slvLogs).Concat(ufaLogs).OrderBy(l => l.Time).ToList();
        }

        /// <summary>
        /// Получает логи сотрудника за период
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="user">Сотрудник</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllPeriod(DateTime from, DateTime to, Models.UserModel user)
        {
            IEnumerable<ScudLog> logs = new List<ScudLog>();
            if (user.ScudSlvId.HasValue && user.ScudSlvId.Value != 0)
                logs = logs.Concat(await GetLogsSlv(l => l.TimeVal.Date >= from.Date && l.TimeVal.Date <= to.Date && l.HozOrgan == user.ScudSlvId));
            if (user.ScudUfaId.HasValue && user.ScudUfaId.Value != 0)
                logs = logs.Concat(await GetLogsUfa(l => l.LogTime.Value.Date >= from.Date && l.LogTime.Value.Date <= to.Date && l.EmpHint == user.ScudUfaId));
            return logs.OrderBy(l => l.Time).ToList();
        }

        /// <summary>
        /// Получает логи сотрудника за день
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="user">Сотрудник</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllDay(DateTime date, Models.UserModel user)
        {
            IEnumerable<ScudLog> logs = new List<ScudLog>();
            if (user.ScudSlvId.HasValue && user.ScudSlvId.Value != 0)
                logs = logs.Concat(await GetLogsSlv(l => l.TimeVal.Date == date.Date && l.HozOrgan == user.ScudSlvId.Value));
            if (user.ScudUfaId.HasValue && user.ScudUfaId.Value != 0)
                logs = logs.Concat(await GetLogsUfa(l => l.LogTime.Value.Date == date.Date && l.EmpHint == user.ScudUfaId.Value));
            return logs.ToList();
        }

        /// <summary>
        /// Получает все логи за месяц
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllMonth(DateTime date)
        {
            var slvLogs = await GetLogsSlv(l => l.TimeVal.Year == date.Year && l.TimeVal.Month == date.Month);
            var ufaLogs = await GetLogsUfa(l => l.LogTime.Value.Year == date.Year && l.LogTime.Value.Month == date.Month);
            return new List<ScudLog>().Concat(slvLogs).Concat(ufaLogs).OrderBy(l => l.Time).ToList();
        }

        /// <summary>
        /// Получает логи сотрудника за месяц
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="user">Сотрудник</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsAllMonth(DateTime date, Models.UserModel user)
        {
            IEnumerable<ScudLog> logs = new List<ScudLog>();
            if (user.ScudSlvId.HasValue && user.ScudSlvId.Value != 0)
                logs = logs.Concat(await GetLogsSlv(l => l.TimeVal.Year == date.Year && l.TimeVal.Month == date.Month && l.HozOrgan == user.ScudSlvId.Value));
            if (user.ScudUfaId.HasValue && user.ScudUfaId.Value != 0)
                logs = logs.Concat(await GetLogsUfa(l => l.LogTime.Value.Year == date.Year && l.LogTime.Value.Month == date.Month && l.EmpHint == user.ScudUfaId.Value));
            return logs.ToList();
        }

        /// <summary>
        /// Получает логи из салаватского СКУД
        /// </summary>
        /// <param name="search">Параметры поиска</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsSlv(System.Linq.Expressions.Expression<Func<Models.OrionPro.PLogData, bool>> search)
        {
            return await _orion.PLogData.Where(l => _doorsSlv.Contains(l.DoorIndex.Value))
                                        .Where(search)
                                        .OrderBy(l => l.TimeVal)
                                        .Select(l => new ScudLog
                                        {
                                            Time = l.TimeVal,
                                            DoorId = l.DoorIndex.Value,
                                            Type = l.Mode == 1 ? ScudTypes.In : ScudTypes.Out,
                                            City = ScudCities.Salavat,
                                            UserId = l.HozOrgan.Value,
                                            Event = GetOrionProEvent(l.Event.Value)
                                        })
                                        .ToListAsync();
        }

        /// <summary>
        /// Получает логи из уфимского СКУД
        /// </summary>
        /// <param name="search">Параметры поиска</param>
        /// <returns>Логи</returns>
        public async Task<List<ScudLog>> GetLogsUfa(System.Linq.Expressions.Expression<Func<Models.Sigur.Logs, bool>> search)
        {
            return (await _sigur.Logs.Where(l => _doorsUfa.Contains(l.DevHint.Value) && l.LogTime.HasValue && l.EmpHint != 0 && l.DevHint != 0)
                                     .Where(search)
                                     .OrderBy(l => l.LogTime)
                                     .ToListAsync())
                                     .Where(l => l.LogData[0] == 0xFE && l.LogData[1] == 0x06)
                                     .Select(l => new ScudLog
                                     {
                                         Time = l.LogTime.Value,
                                         DoorId = l.DevHint.Value,
                                         Type = l.LogData[4] == 2 ? ScudTypes.In : ScudTypes.Out,
                                         City = ScudCities.Ufa,
                                         UserId = l.EmpHint.Value,
                                         Event = Events.Entry
                                     })
                                     .ToList();
        }

        /// <summary>
        /// Конвертирует ID в локальные
        /// </summary>
        /// <param name="logs">Логи</param>
        /// <param name="users">Список пользователей</param>
        /// <returns>Логи</returns>
        public List<ScudLog> ConvertIdsToLocal(List<ScudLog> logs, List<Models.UserModel> users)
        {
            return logs.Where(l => (l.City == ScudCities.Salavat && users.Any(u => u.ScudSlvId == l.UserId)) ||
                                   (l.City == ScudCities.Ufa && users.Any(u => u.ScudUfaId == l.UserId)))
                       .Select(l => new ScudLog
                       {
                           Time = l.Time,
                           DoorId = l.DoorId,
                           Event = l.Event,
                           Type = l.Type,
                           City = l.City,
                           UserIdType = IdTypes.Internal,
                           UserId = l.City == ScudCities.Salavat ? users.FirstOrDefault(u => u.ScudSlvId == l.UserId).Id : users.FirstOrDefault(u => u.ScudUfaId == l.UserId).Id
                       })
                       .ToList();
        }

        /// <summary>
        /// Получает время из логов
        /// </summary>
        /// <param name="logs">Логи</param>
        /// <param name="logTime">Писать время начала и окончания работы</param>
        /// <returns></returns>
        public async Task<List<ScudTime>> GetTimeFromLogs(List<ScudLog> logs, bool logTime = false)
        {
            if (logs.Count == 0)
                return new List<ScudTime>();

            var entries = new Dictionary<int, DateTime>();
            var list = new Dictionary<int, ScudTime>();
            var days = await _calendar.GetDaysAsync(logs.Min(l => l.Time), logs.Max(l => l.Time));
            if (logs.Any(l => l.UserIdType == IdTypes.SCUD))
                throw new Exception("Only internal IDs");

            foreach (var log in logs)
            {
                if (entries.ContainsKey(log.UserId))
                {
                    if (entries[log.UserId].Date != log.Time.Date)
                        entries.Remove(log.UserId);
                    else if (list.ContainsKey(log.UserId) && log.Type == ScudTypes.Out)
                    {
                        var time = (long)(log.Time - entries[log.UserId]).TotalSeconds;
                        list[log.UserId].Time += time;

                        if (days[log.Time.Date].Type != Models.CalendarDayModel.DayTypes.Working)
                        {
                            list[log.UserId].RTime += time;
                        }
                        else if (entries[log.UserId].Hour == 12 && log.Time.Hour == 12)
                        {
                            //Ничего
                        }
                        else if (entries[log.UserId].Hour < 12 && log.Time.Hour > 12)
                        {
                            list[log.UserId].RTime += time - (60 * 60);
                        }
                        else if (entries[log.UserId].Hour < 12 && log.Time.Hour == 12)
                        {
                            list[log.UserId].RTime += time - log.Time.Second - log.Time.Minute * 60;
                        }
                        else if (entries[log.UserId].Hour == 12 && log.Time.Hour > 12)
                        {
                            list[log.UserId].RTime += time - (59 - entries[log.UserId].Second) - (59 - entries[log.UserId].Minute) * 60;
                        }
                        else
                        {
                            list[log.UserId].RTime += time;
                        }


                        if (logTime)
                            list[log.UserId].EndTime = log.Time;
                        entries.Remove(log.UserId);
                    }
                }
                else if (log.Type == ScudTypes.In)
                {
                    if (!list.ContainsKey(log.UserId))
                        list.Add(log.UserId, new ScudTime { UserId = log.UserId, StartTime = logTime ? (DateTime?)log.Time : null });
                    else
                        list[log.UserId].EndTime = null;

                    entries.Add(log.UserId, log.Time);
                }
            }

            return list.Select(l => l.Value).ToList();
        }

        /// <summary>
        /// Получает событие по коду из OrionPro
        /// </summary>
        /// <param name="eventId">Код события</param>
        /// <returns>Событие</returns>
        private static Events GetOrionProEvent(int eventId)
        {
            switch (eventId)
            {
                case 28: return Events.AccessGranted;
                case 29: return Events.AccessDenied;
                case 32:
                default: return Events.Entry;
            }
        }

        public class ScudLog
        {
            /// <summary>
            /// Время
            /// </summary>
            public DateTime Time { get; set; }

            /// <summary>
            /// ID двери в СКУД
            /// </summary>
            public int DoorId { get; set; }

            /// <summary>
            /// Город
            /// </summary>
            public ScudCities City { get; set; }

            /// <summary>
            /// ID пользователя
            /// </summary>
            public int UserId { get; set; }

            /// <summary>
            /// Тип ID пользователя
            /// </summary>
            public IdTypes UserIdType { get; set; } = IdTypes.SCUD;

            /// <summary>
            /// Тип
            /// </summary>
            public ScudTypes Type { get; set; }

            /// <summary>
            /// Событие
            /// </summary>
            public Events Event { get; set; }

            /// <summary>
            /// СКУД
            /// </summary>
            public enum ScudCities
            {
                /// <summary>
                /// Салават
                /// </summary>
                Salavat = 1,
                /// <summary>
                /// Уфа
                /// </summary>
                Ufa = 2,
            }

            /// <summary>
            /// Статусы
            /// </summary>
            public enum ScudTypes
            {
                /// <summary>
                /// Вход
                /// </summary>
                In = 1,
                /// <summary>
                /// Выход
                /// </summary>
                Out = 2
            }

            /// <summary>
            /// События
            /// </summary>
            public enum Events
            {
                /// <summary>
                /// Вход
                /// </summary>
                Entry = 0,
                /// <summary>
                /// Доступ предоставлен
                /// </summary>
                AccessGranted = 1,
                /// <summary>
                /// Запрет доступа
                /// </summary>
                AccessDenied = 2,
            }

            /// <summary>
            /// Тип ID пользователей
            /// </summary>
            public enum IdTypes
            {
                /// <summary>
                /// СКУД
                /// </summary>
                SCUD = 0,
                /// <summary>
                /// Внутренний
                /// </summary>
                Internal = 1
            }
        }

        public class ScudTime
        {
            /// <summary>
            /// ID сотрудника
            /// </summary>
            public int UserId { get; set; }
            /// <summary>
            /// Время начала работы
            /// </summary>
            public DateTime? StartTime { get; set; } = null;
            /// <summary>
            /// Время окончания работы
            /// </summary>
            public DateTime? EndTime { get; set; } = null;
            /// <summary>
            /// Общее время работы (в секундах)
            /// </summary>
            public long Time { get; set; } = 0;
            /// <summary>
            /// Время работы без обеда (в секундах)
            /// </summary>
            public long RTime { get; set; } = 0;
        }
    }
}
