using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserSelectStoryModeActivity : AbstractUserActivity
    {
        private Mode mode;
        private bool initNstart;
        private string recordVoiceName;
        public UserSelectStoryModeActivity(Mode mode, bool initNstart)
        {
            this.mode = mode;
            this.initNstart = initNstart;
        }
        public UserSelectStoryModeActivity(Mode mode, bool initNstart, string recordVoiceName)
        {
            this.mode = mode;
            this.initNstart = initNstart;
            this.recordVoiceName = recordVoiceName;
        }
        public Mode getMode() { return mode; }

        public bool startAfterInit() { return initNstart; }

        public string getRecordVoiceName() { return recordVoiceName; }

        public override string getPropertyId()
        {
            return EBookConstant.USER_SELECT_STORY_MODE;
        }
    }
}