using System.Text.RegularExpressions;

namespace SharpDepartmentBot.Tests
{
    public class DataUtilsFacts : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly DataUtils _dataUtils;

        public DataUtilsFacts(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _dataUtils = new DataUtils(DatabaseFixture.ConnectionString);
        }

        [Theory]
        [InlineData("1111", "TestUrl1")]
        [InlineData("1112", "TestUrl2")]
        public void FindScheduleTheorySuccsess(string roleName, string expected)
        {
            _fixture.SetupShedule();
            var result = _dataUtils.FindSchedule(roleName);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1113")]
        [InlineData("")]
        [InlineData("not-a-number")]
        public void FindScheduleTheoryFailure(string roleName)
        {
            _fixture.SetupShedule();
            var result = _dataUtils.FindSchedule(roleName);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void FindLinksReturnsAllResourcesInMarkdownFormat()
        {
            _fixture.SetupRescources();
            _dataUtils.InvalidateCache();
            var result = _dataUtils.FindLinks();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));

            // Parse the Discord-flavoured markdown: one row per line in the
            // form "[Name](<Url>)". Asserting against structured output is
            // far more robust than the previous multi-Replace/Split chain.
            var rowPattern = new Regex(@"^\[(?<name>[^\]]+)\]\(<(?<url>[^>]+)>\)$");
            var rows = result.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, rows.Length);

            var matches = rows.Select(r => rowPattern.Match(r)).ToList();
            Assert.All(matches, m => Assert.True(m.Success, "Row did not match expected markdown format"));

            Assert.Equal("TestName1", matches[0].Groups["name"].Value);
            Assert.Equal("TestUrl1", matches[0].Groups["url"].Value);
            Assert.Equal("TestName2", matches[1].Groups["name"].Value);
            Assert.Equal("TestUrl2", matches[1].Groups["url"].Value);
        }
    }
}
