using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.devices
{
    /// <summary>
    /// This class will receive gesture input signals from client. Gesture signals include
    /// include button-clicking, text-typing, etc.
    /// 
    /// The user activity will be sent to ActivityExecutor. 
    /// </summary>
    public class EBookGestureInputDevice : AbstractDevice
    {

        public void AcceptSpeech()
        {
            generateUserActivity(new UserConfirmRecordedSpeechActivity(true));
        }
 
        public void RejectSpeech()
        {
            generateUserActivity(new UserConfirmRecordedSpeechActivity(false));
        }
     
        public void goBack()
        {
            generateUserActivity(new UserChangePageActivity(PageAction.PREVIOUS));
        }
        
        public void goNext()
        {
            generateUserActivity(new UserChangePageActivity(PageAction.NEXT));
        }

      
        public void RealTimeMode()
        {
            generateUserActivity(new UserSelectStoryModeActivity(Mode.REALTIME, true));
        }

        public void ReplayMode()
        {
            generateUserActivity(new UserSelectStoryModeActivity(Mode.REPLAY, false));

        }

        public void setVoice(string voice)
        {
            generateUserActivity(new UserStartReplayingActivity(voice));
        }

        public void SetStory(string storyName)
        {
            generateUserActivity(new UserSelectStoryActivity(storyName));
        }
        public void pause()
        {
            generateUserActivity(new UserPauseResumeActivity(true));
        }
        public void resume()
        {
            generateUserActivity(new UserPauseResumeActivity(false));
        }

        public void record(string voiceN)
        {
            generateUserActivity(new UserSelectStoryModeActivity(Mode.RECORD, true, voiceN));
        }

        public void OverwriteAndRecord()
        {
            generateUserActivity(new UserStartRecordingActivity());
        }

        private void generateUserActivity(Activity act)
        {
            if (active)
            {
                ActivityExecutor.add(act);
            }
        }

        public override string getDeviceName()
        {
            return "gesture device";
        }
    }
}