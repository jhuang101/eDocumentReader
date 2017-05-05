using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserSelectStoryActivity : AbstractUserActivity
    {
        private string storyName;
        public UserSelectStoryActivity(string storyName)
        {
            this.storyName = storyName;
        }
        public string getStoryName() { return storyName; }

        public override string getPropertyId()
        {
            return EBookConstant.USER_SELECT_STORY_NAME;
        }
    }
}