using eDocumentReader.Hubs.activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    public abstract class Activity
    {
        private double creationTime;
        public Activity()
        {
            creationTime = EBookUtil.GetUnixTimeMillis();
        }
        public double getActivityStartTime()
        {
            return creationTime;
        }
        public abstract ActivityType getActivityType();
        public abstract string getPropertyId();
    }
}