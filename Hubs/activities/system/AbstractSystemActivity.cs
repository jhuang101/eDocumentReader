using eDocumentReader.Hubs.activities;
using eDocumentReader.Hubs.activities.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// This class provides a skeletal of system activity. It
    /// represents the actions that the system will take if a user
    /// interact with the GUI.
    /// Any sub classes inherit this class need to have a set of 
    /// process implemented inside the execute method.
    /// </summary>
    public abstract class AbstractSystemActivity : Activity
    {
        protected Activity relActivity;

        public AbstractSystemActivity()
        {
        }

        public void attach(Activity userActivity)
        {
            this.relActivity = userActivity;
        }
   

        public override ActivityType getActivityType()
        {
            return ActivityType.SYSTEM_ACTIVITY;
        }

        public abstract void execute(List<Activity> historyList);


    }
}