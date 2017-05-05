using eDocumentReader.Hubs.devices.command;
using eDocumentReader.Hubs.devices.speech;
using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using System.Web;

namespace eDocumentReader.Hubs.devices
{
    /// <summary>
    /// An EBook Device Manager encapsulate all the io devices.
    /// 
    /// </summary>
    public class EBookDeviceManager : AbstractDeviceManager
    {
        private List<AbstractDevice> list = new List<AbstractDevice>();
        private static EBookSpeechRecognitionDevice sr_device;
        private static EBookGestureInputDevice gesture;
        private static EBookBrowserDisplayDevice browserDisplay;
        private static EBookAudioPlayer audioPlayer;
        private static StoryLoggingDevice storyLogger;
        protected override List<AbstractDevice> deviceList
        {
            get
            {
                return list;
            }
        }

        public EBookDeviceManager()
        {
            init();
        }

        /// <summary>
        /// Initialize all the devices
        /// </summary>
        public override void init()
        {
            sr_device = new EBookSpeechRecognitionDevice();
            gesture = new EBookGestureInputDevice();
            browserDisplay = new EBookBrowserDisplayDevice();
            audioPlayer = new EBookAudioPlayer();
            storyLogger = new StoryLoggingDevice();

            list.Add(sr_device);
            list.Add(gesture);
            list.Add(browserDisplay);
            list.Add(audioPlayer);
            list.Add(storyLogger);
        }

        /// <summary>
        /// execute the command in a separate thread
        /// </summary>
        /// <param name="obj"></param>
        protected override void doWork(Object obj)
        {
            Command command = (Command)obj;
            List<Object> pl = command.getPayload();

            if (command.getType() == CommandType.UPDATE_PAGE_TEXT)
            {
                if (pl.Count == 2)
                {
                    string text = (string)pl.ElementAt(0);
                    int pageNum = (int)pl.ElementAt(1);
                    lock (browserDisplay)
                    {
                        browserDisplay.DisplayStoryText(text, pageNum);
                    }
                }
            }
            else if (command.getType() == CommandType.CREATE_PAUSE_RESUME_BUTTON)
            {
                lock (browserDisplay)
                {
                    browserDisplay.createPauseResumeButton();
                }
            }
            else if (command.getType() == CommandType.CREATE_ACCEPT_REJECT_BUTTON)
            {
                lock (browserDisplay)
                {
                    browserDisplay.createAcceptRejectButtons();
                }
            }
            else if (command.getType() == CommandType.LOG_SPEECH_STATE)
            {
                if (pl.Count == 1)
                {
                    SpeechState ss = (SpeechState)pl.ElementAt(0);
                    lock (storyLogger)
                    {
                        storyLogger.logSpeechState(ss);
                    }
                }
            }
            else if (command.getType() == CommandType.LOG_RECOGNITION_RESULT)
            {
                if (pl.Count == 8)
                {
                    float confidence = (float)pl.ElementAt(0);
                    string textResult = (string)pl.ElementAt(1);
                    bool isHypothesis = (bool)pl.ElementAt(2);
                    KeyValuePair<string, SemanticValue>[] semanticResult = (KeyValuePair<string, SemanticValue>[])pl.ElementAt(3);
                    string grammarName = (string)pl.ElementAt(4);
                    string rulename = (string)pl.ElementAt(5);
                    double audioDuration = (double)pl.ElementAt(6);
                    string wavPath = (string)pl.ElementAt(7);
                    lock (storyLogger)
                    {
                        storyLogger.logRecognitionResult(confidence, textResult, isHypothesis, semanticResult,
                            grammarName, rulename, audioDuration, wavPath);
                    }
                }
            }
            else if (command.getType() == CommandType.ENABLE_ACCEPT_REJECT_BUTTON)
            {
                if (pl.Count == 1)
                {
                    bool enable = (bool)pl.ElementAt(0);
                    lock (browserDisplay)
                    {
                        browserDisplay.enableAcceptRejectButton(enable);
                    }
                }
            }
            else if (command.getType() == CommandType.INIT_DEVICES_FOR_REALTIME)
            {
                if (pl.Count == 1)
                {
                    INPUT_STREAM_TYPE st = (INPUT_STREAM_TYPE)pl.ElementAt(0);
                    lock (sr_device)
                    {
                        sr_device.SetAudioPath(null);
                        sr_device.initializeAudioStream(st);
                        
                    }
                }
            }
            else if (command.getType() == CommandType.ENABLE_STORY_LOGGER)
            {
                if (pl.Count == 1)
                {
                    bool enable = (bool)pl.ElementAt(0);
                    lock (storyLogger)
                    {
                        storyLogger.enable(enable);
                    }
                }
            }
            else if (command.getType() == CommandType.ASK_USER_CHOOSE_VOICE)
            {
                if (pl.Count == 1)
                {
                    string voicePath = (string)pl.ElementAt(0);
                    lock (browserDisplay)
                    {
                        browserDisplay.chooseVoice(voicePath);
                    }
                }
            }
            else if (command.getType() == CommandType.ASK_USER_OVERWRITE_VOICE)
            {
                lock (browserDisplay)
                {
                    browserDisplay.askOverwriteExistVoiceName();
                }
            }
            else if (command.getType() == CommandType.REPLAY_AUDIO)
            {
                if (pl.Count == 2)
                {
                    string audioName = (string)pl.ElementAt(0);
                    int audioIndex = (int)pl.ElementAt(1);
                    lock (audioPlayer)
                    {
                        audioPlayer.playAudio(audioName, audioIndex);
                    }
                }
            }
            else if (command.getType() == CommandType.LOG_REJECT_LAST_RECOGNITION)
            {
                lock (storyLogger)
                {
                    storyLogger.logRejectLastRecognition();
                }
            }
            else if (command.getType() == CommandType.PLAY_ANIMATION)
            {
                if (pl.Count == 1)
                {
                    int animationId = (int)pl.ElementAt(0);
                    lock (browserDisplay)
                    {
                        browserDisplay.playAnimation(animationId);
                    }
                }
            }
            else if (command.getType() == CommandType.CONFIRM_AND_SAVE_SPEECH)
            {
                lock (sr_device)
                {
                    sr_device.confirmAndSaveAudio();
                }
                lock (storyLogger)
                {
                    storyLogger.acceptSpeech();
                }
            }
            else if (command.getType() == CommandType.CLEAN_UNCONFIRMED_SPEECH)
            {
                lock (sr_device)
                {
                    sr_device.cleanUnconfirmAudio();
                }
                lock (storyLogger)
                {
                    storyLogger.rejectSpeech();
                }
            }
            else if (command.getType() == CommandType.LOG_PAGE_END)
            {
                lock (storyLogger)
                {
                    storyLogger.finishPage();
                }
            }
            else if (command.getType() == CommandType.RESTART_SYSTEM)
            {
                lock (sr_device)
                {
                    sr_device.restart();
                }
                
            }
            else if (command.getType() == CommandType.LOAD_COMMAND_GRAMMAR)
            {
                if (pl.Count == 1)
                {
                    List<string> grammarList = (List<string>)pl.ElementAt(0);
                    lock (sr_device)
                    {
                        sr_device.LoadCommandGrammars(grammarList);
                    }
                }
            }
            else if (command.getType() == CommandType.INIT_DEVICES_FOR_RECORD)
            {
                if (pl.Count == 2)
                {
                    string path = (string)pl.ElementAt(0);
                    INPUT_STREAM_TYPE type = (INPUT_STREAM_TYPE)pl.ElementAt(1);
                    lock (sr_device)
                    {
                        sr_device.SetAudioPath(path);
                        sr_device.initializeAudioStream(type);
 
                    }
                    lock (storyLogger)
                    {
                        storyLogger.enable(true);
                        storyLogger.reset();
                        storyLogger.setLogPath(path);
                    }
                }
            }
            else if (command.getType() == CommandType.INIT_DEVICE_FOR_REPLAY)
            {
                lock (sr_device)
                {
                    sr_device.enableSR(false);
                }
            }
            else if (command.getType() == CommandType.CHANGE_BACKGROUND)
            {
                if (pl.Count == 1)
                {
                    string bg = (string)pl.ElementAt(0);
                    lock (browserDisplay)
                    {
                        browserDisplay.changeBackgroundImage(bg);
                    }
                }
            }
            else if (command.getType() == CommandType.RELOAD_ONGOING_GRAMMAR)
            {
                if (pl.Count == 1)
                {
                    Grammar g = (Grammar)pl.ElementAt(0);
                    lock (sr_device)
                    {
                        sr_device.ReloadOnGoingGrammar(g);
                        sr_device.enableSR(true);
                    }
                }
            }
            else if (command.getType() == CommandType.CHANGE_GRAMMAR_PRIORITY)
            {
                if (pl.Count == 1)
                {
                    int priority = (int)pl.ElementAt(0);
                    lock (sr_device)
                    {
                        sr_device.ReloadAndChangeGrammarPriority(priority);
                    }
                }
            }
            else if (command.getType() == CommandType.DISPLAY_MAIN_PAGE)
            {
                if (pl.Count == 1)
                {
                    string[] list = (string[])pl.ElementAt(0);
                    lock (browserDisplay)
                    {
                        browserDisplay.chooseStory(list);
                    }
                }
            }
            else if (command.getType() == CommandType.RELOAD_STORY_GRAMMAR)
            {
                if (pl.Count == 1)
                {
                    List<Grammar> g = (List<Grammar>)pl.ElementAt(0);
                    lock (sr_device)
                    {
                        sr_device.ReloadStoryGrammars(g);
                        sr_device.enableSR(true);
                    }
                }
            }
        }
    }
}