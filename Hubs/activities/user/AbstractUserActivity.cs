using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{

    /// <summary>
    /// The user activity represents the actions that a user
    /// can be perform on the system.
    /// </summary>
    public abstract class AbstractUserActivity : Activity
    {
        public AbstractUserActivity()
        {
        }

        public override ActivityType getActivityType()
        {
            return ActivityType.USER_ACTIVITY;
        }
    }
}