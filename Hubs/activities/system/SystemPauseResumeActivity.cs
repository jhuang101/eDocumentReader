using eDocumentReader.Hubs.activities.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemPauseResumeActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemPauseResumeActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            UserPauseResumeActivity activity = (UserPauseResumeActivity)relActivity;
            if (activity.isPause())
            {
                storyManager.pause();
            }
            else
            {
                storyManager.resume();
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_PAUSE_RESUME;
        }
    }
}