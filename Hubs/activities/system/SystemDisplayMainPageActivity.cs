using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemDisplayMainPageActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemDisplayMainPageActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            string[] storyList = storyManager.getStoryNames();
            Command comm = new Command(CommandType.DISPLAY_MAIN_PAGE);
            comm.addData(storyList);
            AbstractDeviceManager.executeCommand(comm);
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_DISPLAY_MAIN_PAGE;
        }
    }
}