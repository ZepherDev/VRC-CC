using System;
using System.Collections.Generic;

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
         
         Execution of the timeline begins with a call to movePlayheadToTime(time)
         The function detects `a` and sets lastTick to the current elapsed time in milliseconds
         It sets playOffset to lastTick
         It gets the next item in the list and sets nextEvent
         It returns the first TimelineEvent 
         
         Execution continues when movePlayheadToTime(time) is called again (now at `b`)
         The function changes lastTick to the current elapsed time in ms
         The function checks if lastTick - playOffset >= nextEvent.time
            No, so null is returned
            
         Execution continues when movePlayheadToTime(now) is called again and again, until `c`
         The function changes lastTick to the current elapsed time in ms
         The function checks if lastTimeMS - playOffset >= nextEvent.time
            Yes. It gets the next item in the list and sets nextEvent
            That event is returned
         */
        
        private long lastTick = 0;
        private long playOffset = 0;
        List<TimelineEvent> listOfEvents = new List<TimelineEvent>();
        TimelineEvent nextEvent = null;
        
        public TimelineEvent movePlayheadToTime(long currentElapsedTimeMS) { 
            TimelineEvent te = null;
            lastTick = currentElapsedTimeMS;
            
            return te;
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
        
        public ushort time; 
        public EVENT_TYPE type;
        public string event_text;
    }
}