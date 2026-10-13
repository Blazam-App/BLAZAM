using BLAZAM.ActiveDirectory.Data;

namespace BLAZAM.Tests.ActiveDirectory
{
    public class LogonHoursTests
    {
        [Fact]
        public void LogonHours_Default_AllowsAllHours()
        {
            var logonHours = new LogonHours();

            var encoded = logonHours.EncodeLogonHours();

            Assert.Equal(21, encoded.Length);
            Assert.All(encoded, b => Assert.Equal(0xFF, b));
        }

        [Fact]
        public void LogonHours_NullRawData_AllowsAllHours()
        {
            var logonHours = new LogonHours(null);

            Assert.All(logonHours.EncodeLogonHours(), b => Assert.Equal(0xFF, b));
        }

        [Fact]
        public void LogonHours_Constructor_DecodesRawData()
        {
            var raw = new byte[21];
            raw[0] = 0x01; // Sunday hour 0 only

            var logonHours = new LogonHours(raw);

            Assert.Equal(raw, logonHours.EncodeLogonHours());
        }

        [Fact]
        public void LogonHours_EncodeDecode_RoundTrips()
        {
            var raw = new byte[21];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = (byte)(i * 13 + 5);
            }

            var logonHours = new LogonHours();
            logonHours.DecodeLogonHours(raw);

            Assert.Equal(raw, logonHours.EncodeLogonHours());
        }

        [Fact]
        public void LogonHours_Decode_WrongLength_Throws()
        {
            var logonHours = new LogonHours();

            Assert.Throws<ArgumentException>(() => logonHours.DecodeLogonHours(new byte[20]));
        }

        [Fact]
        public void LogonHours_Decode_Null_DoesNothing()
        {
            var logonHours = new LogonHours();

            logonHours.DecodeLogonHours(null!);

            Assert.All(logonHours.EncodeLogonHours(), b => Assert.Equal(0xFF, b));
        }

        [Fact]
        public void LogonHours_ToggleHour_FlipsBit()
        {
            var logonHours = new LogonHours();

            logonHours.ToggleHour(DayOfWeek.Sunday, 0);
            Assert.Equal(0xFE, logonHours.EncodeLogonHours()[0]);

            logonHours.ToggleHour(DayOfWeek.Sunday, 0);
            Assert.Equal(0xFF, logonHours.EncodeLogonHours()[0]);
        }

        [Fact]
        public void LogonHours_ToggleHour_Monday_AffectsCorrectByte()
        {
            var logonHours = new LogonHours();

            logonHours.ToggleHour(DayOfWeek.Monday, 0); // bit index 24 => byte 3, bit 0

            var encoded = logonHours.EncodeLogonHours();
            Assert.Equal(0xFE, encoded[3]);
            Assert.Equal(0xFF, encoded[2]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(24)]
        public void LogonHours_InvalidHour_Throws(int hour)
        {
            var logonHours = new LogonHours();

            Assert.Throws<ArgumentOutOfRangeException>(() => logonHours.SetLogonHour(DayOfWeek.Monday, hour, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => logonHours.GetLogonHour(DayOfWeek.Monday, hour));
            Assert.Throws<ArgumentOutOfRangeException>(() => logonHours.ToggleHour(DayOfWeek.Monday, hour));
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, 0)]
        [InlineData(DayOfWeek.Monday, 12)]
        [InlineData(DayOfWeek.Saturday, 23)]
        public void LogonHours_SetThenGet_ReturnsValue(DayOfWeek day, int hour)
        {
            var logonHours = new LogonHours();

            logonHours.SetLogonHour(day, hour, false);
            Assert.False(logonHours.GetLogonHour(day, hour));

            logonHours.SetLogonHour(day, hour, true);
            Assert.True(logonHours.GetLogonHour(day, hour));
        }

        [Fact]
        public void LogonHours_SetLogonHour_OnlyAffectsOneSlot()
        {
            var logonHours = new LogonHours();

            logonHours.SetLogonHour(DayOfWeek.Wednesday, 10, false);

            int disallowed = 0;
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                for (int h = 0; h < 24; h++)
                {
                    if (!logonHours.GetLogonHour(day, h)) disallowed++;
                }
            }
            Assert.Equal(1, disallowed);
        }

        [Fact]
        public void LogonHours_HexString_RoundTrips()
        {
            var logonHours = new LogonHours();
            var raw = new byte[21];
            for (int i = 0; i < raw.Length; i++) raw[i] = (byte)(i * 11);

            var hex = logonHours.ToHexString(raw);

            Assert.Equal(42, hex.Length);
            Assert.Equal(raw, logonHours.FromHexString(hex));
        }

        [Fact]
        public void LogonHours_FromHexString_WrongLength_Throws()
        {
            var logonHours = new LogonHours();

            Assert.Throws<ArgumentException>(() => logonHours.FromHexString("FF"));
        }

        [Fact]
        public void LogonHours_PrintSchedule_ContainsAllDays()
        {
            var logonHours = new LogonHours();

            var text = logonHours.PrintSchedule();

            foreach (var name in Enum.GetNames<DayOfWeek>())
            {
                Assert.Contains(name + ": " + new string('1', 24), text);
            }
        }
    }
}