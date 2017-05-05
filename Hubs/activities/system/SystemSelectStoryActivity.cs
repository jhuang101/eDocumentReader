using eDocumentReader.Hubs.activities.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemSelectStoryActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemSelectStoryActivity(StoryManager sm)
        {
            this.storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is UserSelectStoryActivity)
            {
                string storyName = ((UserSelectStoryActivity)relActivity).getStoryName();
                storyManager.SetStory(storyName);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_SELECT_STORY_NAME;
        }
    }
}