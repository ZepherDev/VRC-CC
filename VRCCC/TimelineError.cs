using System;

namespace VRCCC
{
    public class TimelineError : Exception
    {
        Type type;
        String additionalInfo;
            
        public TimelineError(Type type, string additionalInfo = "") {
            this.type = type;
            this.additionalInfo = additionalInfo;
        }
        
        public enum Type { 
            CANT_PARSE_CARDNUM,
            TIMECODE_REGEX_OR_FORMAT_FAILURE,
        }
    }
}
