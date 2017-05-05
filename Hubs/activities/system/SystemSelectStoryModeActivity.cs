using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemSelectStoryModeActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemSelectStoryModeActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is UserSelectStoryModeActivity){
            
                UserSelectStoryModeActivity act = (UserSelectStoryModeActivity)relActivity;
                Mode mode = act.getMode();
            
                if (mode == Mode.REALTIME)
                {
                    Command comm = new Command(CommandType.INIT_DEVICES_FOR_REALTIME);
                    comm.addData(EBookInteractiveSystem.STREAM_TYPE);
                    AbstractDeviceManager.executeCommand(comm);

                    Command comm2 = new Command(CommandType.ENABLE_STORY_LOGGER);
                    comm2.addData(false);
                    AbstractDeviceManager.executeCommand(comm2);

                    //EBookSpeechRecognizer.setAudioStreamType(1);
                    //StoryLogger.enable(false);
                    List<string> grammarList = storyManager.getCommandGrammars();
                    Command comm3 = new Command(CommandType.LOAD_COMMAND_GRAMMAR);
                    comm3.addData(grammarList);
                    AbstractDeviceManager.executeCommand(comm3);
                    storyManager.changeStoryMode(Mode.REALTIME);
                    storyManager.start();
                    //storyManager.changeStoryMode(Mode.REALTIME,null, true);
                    //AbstractEBookEvent.raise(new ChangeStoryModeEvent(Mode.REALTIME, true));

                }
                else if (mode == Mode.REPLAY)
                {
                    string voicePath = storyManager.getCurrentStoryPath() + "\\" + EBookInteractiveSystem.voice_dir + "\\" + act.getRecordVoiceName();
                    Command comm = new Command(CommandType.ASK_USER_CHOOSE_VOICE);
                    comm.addData(voicePath);
                    AbstractDeviceManager.executeCommand(comm);

                    Command comm2 = new Command(CommandType.ENABLE_STORY_LOGGER);
                    comm2.addData(false);
                    AbstractDeviceManager.executeCommand(comm2);

                    //EBookBrowserDisplayDevice.chooseVoice();
                    //StoryLogger.enable(false);

                }
                else if (mode == Mode.RECORD)
                {
                    string voiceName = act.getRecordVoiceName();
                    if (Directory.Exists(storyManager.getCurrentStoryPath() + "\\" + EBookInteractiveSystem.voice_dir + "\\" + voiceName))
                    {
                        //same voice already exist, ask user to overwrite or choose other name
                        //EBookBrowserDisplayDevice.askOverwriteExistVoiceName();

                        Command comm = new Command(CommandType.ASK_USER_OVERWRITE_VOICE);
                        AbstractDeviceManager.executeCommand(comm);
                    }
                    else
                    {
                        //OverwriteAndRecord();
                        ActivityExecutor.add(new UserStartRecordingActivity());
                    }
                }
            }
        }
        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_CHANGE_STORY_MODE;
        }


    }
}