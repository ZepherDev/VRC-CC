using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VRCCC
{
    public static class SRTDecoder 
    {
        private static List<TimelineError> errors = new List<TimelineError>();
        
        public static List<TimelineError> GETErrorsEncounteredDuringDecoding() { 
                return errors;
        }

        private enum States : ushort { 
            CARD_NUM,
            TIMECODE,
            CONTENT,
        }
        
        public static List<TimelineEvent> DecodeSrtIntoTimelineEvents(String srtString) {
            List<TimelineEvent> events = new List<TimelineEvent>();
            
            StringReader sr = new StringReader(srtString);
            States state = States.CARD_NUM; 
            string line = "";
            int cardNum = 0;
            long startTime = 0;
            long endTime = 0;
            string content = "";
            TimelineEvent te = null;
            Regex timecodeRegex = new Regex(@"^([\d:,\.]+) --> ([\d:,\.]+)$");
            // 00:00:06,000 --> 00:00:12.074
            Regex timeformatRegex = new Regex(@"^(\d\d):(\d\d):(\d\d)[,\.](\d\d\d)$");
            // 00:00:06,000 
            // HH:MM:SS,sss
            
            while ((line = sr.ReadLine()) != null) { 
                switch (state) { 
                    case States.CARD_NUM: 
                        int.TryParse(line.Trim(), out cardNum);
                        state = States.TIMECODE;
                        break;
                    case States.TIMECODE:
                        Match match = timecodeRegex.Match(line);
                        Match match2 = timeformatRegex.Match(match.Groups[1].Value);
                        startTime = int.Parse(match2.Groups[1].Value) * 60 * 60 * 1000;
                        startTime += int.Parse(match2.Groups[2].Value) * 60 * 1000;
                        startTime += int.Parse(match2.Groups[3].Value) * 1000;
                        startTime += int.Parse(match2.Groups[4].Value);
                        
                        match2 = timeformatRegex.Match(match.Groups[2].Value);
                        endTime = int.Parse(match2.Groups[1].Value) * 60 * 60 * 1000;
                        endTime += int.Parse(match2.Groups[2].Value) * 60 * 1000;
                        endTime += int.Parse(match2.Groups[3].Value) * 1000;
                        endTime += int.Parse(match2.Groups[4].Value);
                        
                        state = States.CONTENT;
                        break;
                    case States.CONTENT:
                        if (line.Trim() == "") { 
                            te = new TimelineEvent(TimelineEvent.EVENT_TYPE.CC_START, content,
                                cardNum, startTime);
                            events.Add(te);
                            
                            te = new TimelineEvent(TimelineEvent.EVENT_TYPE.CC_END, "",
                                cardNum, endTime);
                            events.Add(te);
                            state = States.CARD_NUM;
                            content = "";
                        } else { 
                            content += '\n' + line;
                        }
                        break;
                    default:
                        break;
                }
            }
            return events;
        }
    }
}