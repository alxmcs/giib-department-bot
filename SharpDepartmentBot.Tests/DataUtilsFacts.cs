using System.Data.SQLite;

namespace SharpDepartmentBot.Tests
{
    public class DatabaseFixture: IDisposable
    {
        private readonly SQLiteConnection _Connection;
        private static readonly string _CreateSchedule = "CREATE TABLE IF NOT EXISTS \"Schedule\" (\"Id\" INTEGER NOT NULL UNIQUE,\"Group\" INTEGER,\"Url\"\tTEXT,PRIMARY KEY(\"Id\" AUTOINCREMENT));";
        private static readonly string _CreateRescources = "CREATE TABLE IF NOT EXISTS \"Resources\" (\"Id\" INTEGER NOT NULL UNIQUE, \"Name\" TEXT, \"Url\" TEXT,PRIMARY KEY(\"Id\" AUTOINCREMENT));";
        private static readonly string _InsertSchedule = "INSERT INTO \"Schedule\"(\"Group\", \"Url\") VALUES (@firstGroup,@firstUrl),(@secondGroup,@secondUrl);";
        private static readonly string _InsertRescources = "INSERT INTO \"Resources\"(\"Name\", \"Url\") VALUES (@firstName,@firstUrl),(@secondName,@secondUrl);";
        private static readonly string _CountSchedule = "SELECT COUNT(1) FROM \"Schedule\";";
        private static readonly string _CountRescources = "SELECT COUNT(1) FROM \"Resources\";";
        public DatabaseFixture()
        {
            _Connection = new SQLiteConnection("Data Source=TestDatabase;Mode=Memory;Cache=Shared");
            _Connection.Open();
        }
        public void SetupShedule()
        {
            using var createScheduleCmd = new SQLiteCommand(_CreateSchedule, _Connection);
            createScheduleCmd.ExecuteNonQuery();

            using var countScheduleCmd = new SQLiteCommand(_CountSchedule, _Connection);
            var res = countScheduleCmd.ExecuteScalar();
            if (res != null && int.Parse(res.ToString()) == 0)
            {
                using var insertScheduleCmd = new SQLiteCommand(_InsertSchedule, _Connection);
                insertScheduleCmd.Parameters.AddWithValue("@firstGroup", 1111);
                insertScheduleCmd.Parameters.AddWithValue("@firstUrl", "TestUrl1");
                insertScheduleCmd.Parameters.AddWithValue("@secondGroup", 1112);
                insertScheduleCmd.Parameters.AddWithValue("@secondUrl", "TestUrl2");
                insertScheduleCmd.ExecuteNonQuery();
            }

        }
        public void SetupRescources()
        {
            using var createRescourcesCmd = new SQLiteCommand(_CreateRescources, _Connection);
            createRescourcesCmd.ExecuteNonQuery();
            using var countRescourcesCmd = new SQLiteCommand(_CountRescources, _Connection);
            var res = countRescourcesCmd.ExecuteScalar();
            if (res != null && int.Parse(res.ToString()) == 0)
            {
                using var insertRescourcesCmd = new SQLiteCommand(_InsertRescources, _Connection);
                insertRescourcesCmd.Parameters.AddWithValue("@firstName", "TestName1");
                insertRescourcesCmd.Parameters.AddWithValue("@firstUrl", "TestUrl1");
                insertRescourcesCmd.Parameters.AddWithValue("@secondName", "TestName2");
                insertRescourcesCmd.Parameters.AddWithValue("@secondUrl", "TestUrl2");
                insertRescourcesCmd.ExecuteNonQuery();
            }
        }
        void IDisposable.Dispose()
        {
            _Connection.Close();
            _Connection.Dispose();
        }
    }
    public class DataUtilsFacts : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _Fixture;
        public DataUtilsFacts(DatabaseFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void FindScheduleFact()
        {
            _Fixture.SetupShedule();
            var result = DataUtils.FindSchedule("1111");
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Equal("TestUrl1", result);
            result = DataUtils.FindSchedule("1112");
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Equal("TestUrl2", result);
            result = DataUtils.FindSchedule("1113");
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result));
            result = DataUtils.FindSchedule(string.Empty);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void FindLinksFact()
        {
            _Fixture.SetupRescources();
            var result = DataUtils.FindLinks();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            var items = result.Replace("\n", "").Replace("<", " ").Replace(">", " ").Trim().Split(" ");
            Assert.NotNull(items);
            Assert.Equal(4, items.Length);
            Assert.NotNull(items[0]);
            Assert.False(string.IsNullOrEmpty(items[0]));
            Assert.Equal("TestName1", items[0]);
            Assert.NotNull(items[1]);
            Assert.False(string.IsNullOrEmpty(items[0]));
            Assert.Equal("TestUrl1", items[1]);
            Assert.NotNull(items[2]);
            Assert.False(string.IsNullOrEmpty(items[2]));
            Assert.Equal("TestName2", items[2]);
            Assert.NotNull(items[3]);
            Assert.False(string.IsNullOrEmpty(items[3]));
            Assert.Equal("TestUrl2", items[3]);
        }
    }
}