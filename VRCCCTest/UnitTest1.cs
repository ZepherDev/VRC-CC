using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MelonLoader;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using VRCCC;
using static VRCCC.SubtitlesApi;

namespace VRCCCTest
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public async Task TestFetch()
        {
            SubtitlesApi api = new SubtitlesApi();
            var result = await api.FetchSub("https://dl.opensubtitles.org/en/download/src-api/vrf-19a80c53/filead/1955211963.gz");
            Console.WriteLine(result);
            Assert.True(result.Length > 512);
        }
    }
}