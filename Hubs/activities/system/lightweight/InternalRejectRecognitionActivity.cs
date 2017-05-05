using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
	public class InternalRejectRecognitionActivity : Activity
	{
        private double creationTime;
        public InternalRejectRecognitionActivity()
        {
            creationTime = EBookUtil.GetUnixTimeMillis();
        }
        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_REJECT_RECOGNITION;
        }
    }
}