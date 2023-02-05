namespace SharpDepartmentBot.Tests
{ 
    public class DataUtilsFacts : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _Fixture;
        public DataUtilsFacts(DatabaseFixture fixture) => _Fixture = fixture;

        [Theory]
        [InlineData("1111", "TestUrl1")]
        [InlineData("1112", "TestUrl2")]
        public void FindScheduleTheorySuccsess(string roleName, string expected)
        {
            _Fixture.SetupShedule();
            var result = DataUtils.FindSchedule(roleName);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1113")]
        [InlineData("")]
        public void FindScheduleTheoryFailure(string roleName)
        {
            _Fixture.SetupShedule();
            var result = DataUtils.FindSchedule(roleName);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Theory]
        [InlineData(0, "TestName1")]
        [InlineData(1, "TestUrl1")]
        [InlineData(2, "TestName2")]
        [InlineData(3, "TestUrl2")]
        public void FindLinksTheory(int index, string expexted)
        {
            _Fixture.SetupRescources();
            var result = DataUtils.FindLinks();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            var items = result.Replace("\n", "").Replace("<", " ").Replace(">", " ").Trim().Split(" ");
            Assert.NotNull(items);
            Assert.Equal(4, items.Length);
            Assert.NotNull(items[index]);
            Assert.False(string.IsNullOrEmpty(items[index]));
            Assert.Equal(expexted, items[index]);
        }
    }
}