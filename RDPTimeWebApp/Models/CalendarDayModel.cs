using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Models
{
    public class CalendarDayModel
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        public DayTypes Type { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }

        public enum DayTypes : byte
        {
            /// <summary>
            /// Рабочий день
            /// </summary>
            Working = 0,
            /// <summary>
            /// Сокращенный
            /// </summary>
            HalfHoliday = 1,
            /// <summary>
            /// Выходной
            /// </summary>
            Holiday = 2
        }
    }
}
