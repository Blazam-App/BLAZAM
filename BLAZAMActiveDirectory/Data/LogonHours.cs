using BLAZAM.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Data
{
    public class LogonHours
    {
   
        private int timeZoneOffset;

        private const int TotalBits = 168;
        private const int BytesCount = TotalBits / 8;
        private bool[,] schedule = new bool[7, 24];
        /// <summary>
        /// Creates a new logon schedule that allows logon at all times
        /// </summary>
        public LogonHours()
        {
            timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;

            // Initialize schedule with default values (all true, meaning logon allowed)
            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    schedule[day, hour] = true;
                }
            }
        }

        public LogonHours(byte[]? rawData) : this()
        {
            // Initialize schedule with default values (all true, meaning logon allowed)
            //for (int day = 0; day < 7; day++)
            //{
            //    for (int hour = 0; hour < 24; hour++)
            //    {
            //        schedule[day, hour] = true;
            //    }
            //}
            if(rawData !=null)
            {
                DecodeLogonHours(rawData);

            }
        }
 
        private void AdjustFromTimeZoneOffset(ref DayOfWeek day, ref int hour)
        {
            if (0 > hour - timeZoneOffset)
            {
                if (day == DayOfWeek.Sunday)
                {
                    day = DayOfWeek.Saturday;
                }
                else
                {
                    day--;
                }
                hour = 24 + hour - timeZoneOffset;
            }
            else if (hour - timeZoneOffset > 23)
            {
                if (day == DayOfWeek.Saturday)
                {
                    day = DayOfWeek.Sunday;
                }
                else
                {
                    day++;
                }
                hour = hour - 24 - timeZoneOffset;
            }
            else
            {
                hour=hour - timeZoneOffset;
            }
        }
      

        public void SetLogonHour(DayOfWeek day, int hour, bool allowed)
        {
            if (hour < 0 || hour >= 24)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
            AdjustFromTimeZoneOffset(ref day, ref hour);

            schedule[(int)day, hour] = allowed;
        }

        public bool GetLogonHour(DayOfWeek day, int hour)
        {
            if (hour < 0 || hour >= 24)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
            AdjustFromTimeZoneOffset(ref day, ref hour);
            return schedule[(int)day, hour];
        }

        public byte[] EncodeLogonHours()
        {
            if (schedule.GetLength(0) != 7 || schedule.GetLength(1) != 24)
            {
                throw new ArgumentException($"Schedule must be a 7x24 array.");
            }

            byte[] logonHours = new byte[21];

            int bitIndex = 0;

            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    int byteIndex = bitIndex / 8;
                    int bitPosition = bitIndex % 8;
                    if (schedule[day, hour])
                    {
                        logonHours[byteIndex] |= (byte)(1 << bitPosition);
                    }
                    bitIndex++;
                }
            }

            return logonHours;
        }

        public void DecodeLogonHours(byte[] logonHours)
        {
            if (logonHours == null) return;
            if (logonHours.Length != BytesCount)
                throw new ArgumentException($"Logon hours must be {BytesCount} bytes long.");

            int bitIndex = 0;

            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    int byteIndex = bitIndex / 8;
                    int bitPosition = bitIndex % 8;
                    schedule[day, hour] = (logonHours[byteIndex] & (1 << bitPosition)) != 0;
                    bitIndex++;
                }
            }
        }

        public string ToHexString(byte[] logonHours)
        {
            return BitConverter.ToString(logonHours).Replace("-", "");
        }

        public byte[] FromHexString(string hex)
        {
            if (hex.Length != BytesCount * 2)
                throw new ArgumentException($"Hex string must be {BytesCount * 2} characters long.");

            byte[] logonHours = new byte[BytesCount];
            for (int i = 0; i < logonHours.Length; i++)
            {
                logonHours[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return logonHours;
        }
        public void ToggleHour(DayOfWeek day, int hour)
        {
            if (hour < 0 || hour >= 24)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");

            schedule[(int)day, hour] = !schedule[(int)day, hour];
        }
        public string PrintSchedule()
        {
            var sb = new StringBuilder();
            var daysOfWeek = Enum.GetNames(typeof(DayOfWeek));

            for (int day = 0; day < 7; day++)
            {
                sb.Append(daysOfWeek[day] + ": ");
                for (int hour = 0; hour < 24; hour++)
                {
                    sb.Append(schedule[day, hour] ? "1" : "0");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
