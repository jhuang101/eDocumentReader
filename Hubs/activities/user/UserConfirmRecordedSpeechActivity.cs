using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserConfirmRecordedSpeechActivity : AbstractUserActivity
    {
        private bool acceptSpeech;

        public UserConfirmRecordedSpeechActivity(Boolean accept)
        {
            acceptSpeech = accept;
        }
        public bool isAccept()
        {
            return acceptSpeech;
        }
        public override string getPropertyId()
        {
            return EBookConstant.USER_ACCEPT_RECORDED_SPEECH;
        }
    }
}