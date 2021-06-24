using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
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
        public async Task TestFetch() {
            var result = await 
                FetchSub("https://dl.opensubtitles.org/en/download/src-api/vrf-19a80c53/filead/1955211963.gz");
            Console.WriteLine(result);
            Assert.True(result.Length > 512);
        }
        
        [Test]
        public void TestTimeline() {
            string[] resources = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resources[0]);
            StreamReader reader = new StreamReader(stream);
            string srtString = reader.ReadToEnd();
            List<TimelineEvent> events = SRTDecoder.DecodeSrtIntoTimelineEvents(srtString);
            Timeline tl = new Timeline(events);
            
            StreamWriter outFile = new StreamWriter("output.txt", append: true);
            
            events = new List<TimelineEvent>();
            for (long timecode = 60 * 1000; timecode < 60 * 10 * 1000; ++timecode) { 
                events = tl.ScrubToTime(timecode); 
                foreach (TimelineEvent ccEvent in events) { 
                    if (ccEvent.type == TimelineEvent.EVENT_TYPE.CC_START) { 
                        outFile.WriteLine("***");
                        outFile.WriteLine(ccEvent.eventText);
                        outFile.Flush();
                    } else if (ccEvent.type == TimelineEvent.EVENT_TYPE.CC_END) { 
                        outFile.WriteLine("\n\n\n\n\n\n");    
                        outFile.Flush();
                    }
                }
                Thread.Sleep(100);
                timecode += 100;
            }
        }
    }
}