using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserPauseResumeActivity : AbstractUserActivity
    {
        private bool pause;
        /*
         * 
         */
        public UserPauseResumeActivity(bool pause)
        {
            this.pause = pause;
        }
        public bool isPause()
        {
            return pause;
        }
        public override string getPropertyId()
        {
            return EBookConstant.USER_PAUSE_RESUME;
        }
    }
}