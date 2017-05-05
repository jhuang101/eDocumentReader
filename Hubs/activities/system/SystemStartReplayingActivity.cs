using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemStartReplayingActivity : AbstractSystemActivity
    {
        StoryManager storyManager;
        public SystemStartReplayingActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is UserStartReplayingActivity)
            {
                //EBookBrowserDisplayDevice.createPauseResumeButton();
                AbstractDeviceManager.executeCommand(new Command(CommandType.CREATE_PAUSE_RESUME_BUTTON));
                
                string voiceName = ((UserStartReplayingActivity)relActivity).getVoiceName();
                string path = storyManager.getCurrentStoryPath()+"\\"+EBookInteractiveSystem.voice_dir+"\\"+voiceName;

                Command comm = new Command(CommandType.INIT_DEVICE_FOR_REPLAY);
                AbstractDeviceManager.executeCommand(comm);
                storyManager.changeStoryMode(Mode.REPLAY);
                storyManager.startReplay(path);
                //storyManager.changeStoryMode(Mode.REPLAY, path, true);
                
            }
        }
        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_START_REPLAYING;
        }
    }
}