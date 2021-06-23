using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace VRCCC
{
    /**
     * Implements a timeline that contains a series of events, to control displaying/hiding closed captions
     * Uses a List of TimelineEvents and expects to be called with the current playtime in ms
     * Will return the next Event and its text, if applicable
     */
    public class Timeline
    {
        /* Theory of operation:
          0:00     0:05                                 0:25
          | - - x - | x - - - - - - - - - - - - - - - - - |
          a b   c   d e                                   f
        
         a - first event in listOfEvents
         b - an interval of time, but with no event (not in list)
         c - second event in listOfEvents
         d - current value of lastTickMS
         e - the next event in listOfEvents
         f - the last event in listOfEvents
         
         TimelineEvents are essentially "show this text <text>" or "hide all text"
         
         The timeline is initialized with data
         listOfEvents is populated
         
         Execution of the timeline begins with a call to scrubToTime(time)
         The function detects `a` and sets lastTick to the current elapsed time in milliseconds
         It sets playOffset to lastTick
         It gets the next item in the list and sets nextEvent
         It returns the first TimelineEvent 
         
         Execution continues when scrubToTime(time) is called again (now at `b`)
         The function changes lastTick to the current elapsed time in ms
         The function checks if lastTick - playOffset >= nextEvent.time
            No, so null is returned
            
         Execution continues when scrubToTime(now) is called again and again, until `c`
         The function changes lastTick to the current elapsed time in ms
         The function checks if lastTimeMS - playOffset >= nextEvent.time
            Yes. It gets the next item in the list and sets nextEvent
            That event is returned
         */
        
        private long _lastTick = 0;
        private long _playOffset = 0;
        private int _lastIndex = -1;
        List<TimelineEvent> listOfEvents = new List<TimelineEvent>();
        
        /**
         * <summary>Returns a list of TimelineEvents that occurred between the last position in the timeline and
         * the provided time, in order. If this is the first time it's called, it bases the start time off
         * the beginning of the timeline.</summary>
         * <param name="currentElapsedTimeMS">The new time in the timeline to go to</param>
         * <returns>A List containing all the events between the previously run position and the provided position
         * </returns>
         */
        public List<TimelineEvent> ScrubToTime(long currentElapsedTimeMS) { 
            List<TimelineEvent> events = new List<TimelineEvent>();
            _lastTick = currentElapsedTimeMS;
            
            for (int i=_lastIndex+1; i < listOfEvents.Count-1; ++i) { 
                if (currentElapsedTimeMS >= listOfEvents[i].time) { 
                    events.Append(listOfEvents[i]);
                }
                _lastIndex = i;
            }
            return events;
        }
        
        public void ResetTimeline() { 
            _lastIndex = -1;    
            _playOffset = 0;
            _lastTick = 0;
        }
        
    }
    
    public class TimelineEvent 
    { 
        public enum EVENT_TYPE : ushort {
            SPEECH_DISPLAY,
            SPEECH_HIDE, 
            DESC_DISPLAY,
            DESC_HIDE,
            ALL_HIDE,
            CC_START,
            CC_END,
        }

        public TimelineEvent(EVENT_TYPE type, string eventText, int subNumber, long time)
        {
            this.type = type;
            this.event_text = eventText;
            this.sub_number = subNumber;
            this.time = time;
        }

        public long time;
        public EVENT_TYPE type;
        public string event_text;
        public int sub_number;
    }
    
    public static class SRTDecoder 
    {
        private enum States : ushort { 
            CARD_NUM,
            TIMECODE,
            CONTENT,
        }
        
        public static List<TimelineEvent> DecodeSRTIntoTimelineEvents(String SRTString) {
            List<TimelineEvent> events = new List<TimelineEvent>();
            
            StringReader sr = new StringReader(SRTString);
            States state = States.CARD_NUM; 
            string line = "";
            int card_num = 0;
            long start_time = 0;
            long end_time = 0;
            string content = "";
            TimelineEvent te = null;
            Regex timecode_regex = new Regex(@"^([\d:,\.]+) --> ([\d:,\.]+)$");
                                            // 00:00:06,000 --> 00:00:12.074
            Regex timeformat_regex = new Regex(@"^(\d\d):(\d\d):(\d\d)[,\.](\d\d\d)$");
                                            // 00:00:06,000 
                                            // HH:MM:SS,sss
            
            while ((line = sr.ReadLine()) != null) { 
                switch (state) { 
                    case States.CARD_NUM: 
                        int.TryParse(line.Trim(), out card_num);
                        state = States.TIMECODE;
                        break;
                    case States.TIMECODE:
                        Match match = timecode_regex.Match(line);
                        Match match2 = timeformat_regex.Match(match.Groups[1].Value);
                        start_time = int.Parse(match2.Groups[1].Value) * 60 * 60 * 1000;
                        start_time += int.Parse(match2.Groups[2].Value) * 60 * 1000;
                        start_time += int.Parse(match2.Groups[3].Value) * 1000;
                        start_time += int.Parse(match2.Groups[4].Value);
                        
                        match2 = timeformat_regex.Match(match.Groups[2].Value);
                        end_time = int.Parse(match2.Groups[1].Value) * 60 * 60 * 1000;
                        end_time += int.Parse(match2.Groups[2].Value) * 60 * 1000;
                        end_time += int.Parse(match2.Groups[3].Value) * 1000;
                        end_time += int.Parse(match2.Groups[4].Value);
                        
                        state = States.CONTENT;
                        break;
                    case States.CONTENT:
                        if (line.Trim() == "") { 
                            te = new TimelineEvent(TimelineEvent.EVENT_TYPE.CC_START, content,
                                card_num, start_time);
                            events.Append(te);
                            
                            te = new TimelineEvent(TimelineEvent.EVENT_TYPE.CC_END, "",
                                card_num, end_time);
                            events.Append(te);
                        } else { 
                            content += line;
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