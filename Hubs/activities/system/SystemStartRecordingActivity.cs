using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemStartRecordingActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemStartRecordingActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is UserStartRecordingActivity)
            {
                for (int i = historyList.Count; --i >= 0; )
                {
                    Activity act = historyList.ElementAt(i);
                    if (act is UserSelectStoryModeActivity)
                    {
                        UserSelectStoryModeActivity uact = (UserSelectStoryModeActivity)act;
                        if (uact.getMode() == Mode.RECORD)
                        {
                            string path = storyManager.getCurrentStoryPath() + "\\" + EBookInteractiveSystem.voice_dir + "\\" + uact.getRecordVoiceName();
                            Command comm = new Command(CommandType.INIT_DEVICES_FOR_RECORD);
                            comm.addData(path);
                            comm.addData(EBookInteractiveSystem.STREAM_TYPE);
                            AbstractDeviceManager.executeCommand(comm);
                            storyManager.changeStoryMode(Mode.RECORD);
                            storyManager.start();
                            //storyManager.changeStoryMode(uact.getMode(),path,true);
                            //createAcceptRejectButtons();
                            AbstractDeviceManager.executeCommand(new Command(CommandType.CREATE_ACCEPT_REJECT_BUTTON));
                            break;
                        }
                    }
                }
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_START_RECORDING;
        }
    }
}