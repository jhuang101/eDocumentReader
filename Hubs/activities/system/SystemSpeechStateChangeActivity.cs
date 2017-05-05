using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemSpeechStateChangeActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemSpeechStateChangeActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            InternalSpeechStateChangeActivity act = (InternalSpeechStateChangeActivity)relActivity;
            storyManager.updateSpeechState(act.getSpeechState());

            Command comm = new Command(CommandType.LOG_SPEECH_STATE);
            comm.addData(act.getSpeechState());
            AbstractDeviceManager.executeCommand(comm);
            //StoryLogger.logSpeechState(act.getSpeechState());
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_SPEECH_STATE_CHANGE;
        }
    }
}