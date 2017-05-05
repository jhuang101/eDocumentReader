using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemSpeechRecognitionResultActivity : AbstractSystemActivity
    {
        StoryManager storyManager;
        public SystemSpeechRecognitionResultActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            InternalSpeechRecognitionResultActivity act = (InternalSpeechRecognitionResultActivity)relActivity;
            storyManager.updateRecognizedText(
                act.getConfidenceScore(),
                act.getTextResult(),
                act.isHypothesisResult(),
                act.getSemanticResult(),
                act.getGrammarName(),
                act.getRuleName(),
                act.getAudioDuration(),
                act.getWavPath());

            /*
            StoryLogger.logRecognitionResult(
                act.getConfidenceScore(),
                act.getTextResult(),
                act.isHypothesisResult(),
                act.getSemanticResult(),
                act.getGrammarName(),
                act.getRuleName(),
                act.getAudioDuration(),
                act.getWavPath());
             * */

            Command comm = new Command(CommandType.LOG_RECOGNITION_RESULT);
            comm.addData(act.getConfidenceScore());
            comm.addData(act.getTextResult());
            comm.addData(act.isHypothesisResult());
            comm.addData(act.getSemanticResult());
            comm.addData(act.getGrammarName());
            comm.addData(act.getRuleName());
            comm.addData(act.getAudioDuration());
            comm.addData(act.getWavPath());

            AbstractDeviceManager.executeCommand(comm);

            if (!act.isHypothesisResult())
            {
                if (storyManager.getStoryMode() == Mode.RECORD)
                {
                    Command comm2 = new Command(CommandType.ENABLE_ACCEPT_REJECT_BUTTON);
                    comm2.addData(true);
                    AbstractDeviceManager.executeCommand(comm2);
                    //EBookBrowserDisplayDevice.disableAcceptRejectButton(false);
                }
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_SPEECH_RECOGNITION_RESULT;
        }


    }
}