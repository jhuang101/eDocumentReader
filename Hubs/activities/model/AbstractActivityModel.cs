using eDocumentReader.Hubs.activities.user;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    public abstract class AbstractActivityModel
    {
        Dictionary<string, AbstractSystemActivity> map;

        public AbstractActivityModel()
        {
            map = initializeActivityMap();
        }

        /// <summary>
        /// all subclass must allocate system activities and add them to map
        /// 
        /// </sthe key of the dictionary is the corresponding property id of an user activityummary>
        /// <returns></returns>
        public abstract Dictionary<string, AbstractSystemActivity> initializeActivityMap();

        /// <summary>
        /// look the dictionary and return the corresponding system activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public AbstractSystemActivity getSystemActivity(Activity activity)
        {
            string id = activity.getPropertyId();
            AbstractSystemActivity sa;
            map.TryGetValue(id, out sa);
            sa.attach(activity);
            
            return sa;
        }
    }
}