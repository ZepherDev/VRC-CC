namespace VRCCC
{
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
}