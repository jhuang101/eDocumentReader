using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserStartReplayingActivity : AbstractUserActivity
    {
        string voiceName;
        public UserStartReplayingActivity(string voiceName)
        {
            this.voiceName = voiceName;
        }
        public string getVoiceName() { return voiceName; }
        public override string getPropertyId()
        {
            return EBookConstant.USER_START_REPLAYING_VOICE;
        }
    }
}