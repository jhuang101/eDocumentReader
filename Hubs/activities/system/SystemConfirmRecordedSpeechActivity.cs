using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemConfirmRecordedSpeechActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemConfirmRecordedSpeechActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity != null)
            {
                UserConfirmRecordedSpeechActivity confirmRecAct = (UserConfirmRecordedSpeechActivity)relActivity;
                if (confirmRecAct.isAccept())
                {
                    //accept speech
                    storyManager.confirmHighlight();
                    Command comm = new Command(CommandType.CONFIRM_AND_SAVE_SPEECH);
                    AbstractDeviceManager.executeCommand(comm);
                }
                else
                {
                    //reject speech
                    storyManager.rollBackHighlight();

                    Command comm = new Command(CommandType.CLEAN_UNCONFIRMED_SPEECH);
                    AbstractDeviceManager.executeCommand(comm);
                }
            }

        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_ACCEPT_RECORDED_SPEECH;
        }
    }
}