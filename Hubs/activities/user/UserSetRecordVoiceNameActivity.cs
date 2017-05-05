using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserSetRecordVoiceNameActivity : AbstractUserActivity
    {
        string voiceName;
        public UserSetRecordVoiceNameActivity(string voiceName)
        {
            this.voiceName = voiceName;
        }
        public string getVoiceName() { return voiceName; }

        public override string getPropertyId()
        {
            return EBookConstant.USER_SET_RECORD_VOICE_NAME; 
        }
    }
}