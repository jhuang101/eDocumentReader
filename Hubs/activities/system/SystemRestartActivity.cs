using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemRestartActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemRestartActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            storyManager.stop();
            Command comm = new Command(CommandType.RESTART_SYSTEM);
            AbstractDeviceManager.executeCommand(comm);
        }
        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_RESTART;
        }
    }
}