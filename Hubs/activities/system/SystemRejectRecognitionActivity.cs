using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemRejectRecognitionActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemRejectRecognitionActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            storyManager.removeLastHighlight();
            Command comm = new Command(CommandType.LOG_REJECT_LAST_RECOGNITION);
            AbstractDeviceManager.executeCommand(comm);
            //StoryLogger.logRejectLastRecognition();
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_REJECT_RECOGNITION;
        }
    }
}