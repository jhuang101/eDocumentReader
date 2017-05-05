using eDocumentReader.Hubs.activities;
using eDocumentReader.Hubs.activities.user;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace eDocumentReader.Hubs
{
    public class ActivityExecutor
    {
        private static readonly int IDLE_TIME = 5; //millisecond
        private static readonly int HISTORY_MAX_SIZE = 100; //the maximum size of activities will be keep in the history 

        private static ConcurrentQueue<Activity> queue;
        private AbstractActivityModel activityModel;
        private static List<Activity> historyList;
        private bool on;//indicate the executor is active

        public ActivityExecutor(AbstractActivityModel model)
        {
            activityModel = model;
            queue = new ConcurrentQueue<Activity>();
            historyList = new List<Activity>();
        }
        public void start()
        {
            on = true;
            Thread oThread = new Thread(new ThreadStart(execThread));
            oThread.Start();
        }
        private void execThread()
        {
            while (on)
            {
                while (queue.Count == 0)
                {
                    Thread.Sleep(IDLE_TIME);
                }
                
                
                Activity activity;
                if (queue.TryDequeue(out activity))
                {
                    AbstractSystemActivity sysActivity = null;
                    if (activity.getActivityType() == ActivityType.USER_ACTIVITY ||
                        activity.getActivityType() == ActivityType.LIGHTWEIGHT_ACTIVITY)
                    {
                        sysActivity = activityModel.getSystemActivity(activity);
                            
                    }
                    else if (activity.getActivityType() == ActivityType.SYSTEM_ACTIVITY)
                    {
                        sysActivity = (AbstractSystemActivity)activity;
                    }
                    Debug.WriteLine(EBookUtil.GetTimestamp()+" : "+activity + "->" + sysActivity);
                    historyList.Add(activity);
                    //TODO: run this in a separate thread?
                    if (sysActivity != null)
                    {
                        sysActivity.execute(historyList);
                        if (historyList.Count == HISTORY_MAX_SIZE)
                        {
                            historyList.RemoveAt(0);
                        }
                        historyList.Add(sysActivity);
                    }
                }
                
            }
        }
        public static void add(Activity userActivity)
        {
            queue.Enqueue(userActivity);
        }
    }
}