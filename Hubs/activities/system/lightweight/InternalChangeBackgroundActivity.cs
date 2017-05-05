using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalChangeBackgroundActivity : Activity
    {

        private string backgroundImage;

        public InternalChangeBackgroundActivity(string bgImage)
        {
            backgroundImage = bgImage;
        }

        public string getBackgroundImage() { return backgroundImage; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_CHANGE_BACKGROUND;
        }
    }
}