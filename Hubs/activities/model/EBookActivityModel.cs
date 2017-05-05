using eDocumentReader.Hubs.activities.system;
using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace eDocumentReader.Hubs
{
    /*
     * An activity model for the Interactive eBook.
     * It contains specific knowledge on what system action (system activity)
     * should be used when a user performs something on the browser interface.
     */
    public class EBookActivityModel : AbstractActivityModel
    {

        private StoryManager storyManager;

        public EBookActivityModel()
        {
        }

        public void init(string storyDirectory)
        {
            storyManager.init(storyDirectory);
        }


        //any resource that a system activity is needed should be passing in the construct
        public override Dictionary<string, AbstractSystemActivity> initializeActivityMap()
        {
            if (storyManager == null)
            {
                storyManager = new StoryManager();
            }
            Dictionary<string, AbstractSystemActivity> map = new Dictionary<string, AbstractSystemActivity>();
            //gesture related activities
            map.Add(EBookConstant.USER_SELECT_STORY_NAME, new SystemSelectStoryActivity(storyManager));
            map.Add(EBookConstant.USER_SELECT_STORY_MODE, new SystemSelectStoryModeActivity(storyManager));
            map.Add(EBookConstant.USER_START_REPLAYING_VOICE, new SystemStartReplayingActivity(storyManager));
            map.Add(EBookConstant.USER_ACCEPT_RECORDED_SPEECH, new SystemConfirmRecordedSpeechActivity(storyManager));
            map.Add(EBookConstant.USER_CHANGE_PAGE, new SystemChangePageActivity(storyManager));
            map.Add(EBookConstant.USER_PAUSE_RESUME, new SystemPauseResumeActivity(storyManager));
            map.Add(EBookConstant.USER_START_RECORDING, new SystemStartRecordingActivity(storyManager));
            map.Add(EBookConstant.USER_RESTART_SYSTEM, new SystemRestartActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_DISPLAY_MAIN_PAGE, new SystemDisplayMainPageActivity(storyManager));

            //ASR related activities
            map.Add(EBookConstant.INTERNAL_REJECT_RECOGNITION, new SystemRejectRecognitionActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_AUDIO_LEVEL_CHANGE, new SystemAudioLevelChangeActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_SPEECH_STATE_CHANGE, new SystemSpeechStateChangeActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_SPEECH_RECOGNITION_RESULT, new SystemSpeechRecognitionResultActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_RELOAD_ONGOING_GRAMMAR, new SystemReloadOnGoingGrammarActivity());
            map.Add(EBookConstant.INTERNAL_CHANGE_GRAMMAR_PRIORITY, new SystemChangeGrammarPriorityActivity());
            map.Add(EBookConstant.INTERNAL_RELOAD_STORY_GRAMMAR, new SystemReloadStoryGrammarActivity());

            //screen display related activities
            map.Add(EBookConstant.INTERNAL_UPDATE_PAGE_TEXT, new SystemUpdatePageTextActivity());
            map.Add(EBookConstant.INTERNAL_UPDATE_HTML_TEXT, new SystemUpdatePageTextActivity());
            map.Add(EBookConstant.INTERNAL_SPEECH_NAVIGATE_PAGE, new SystemChangePageActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_FINISH_PAGE_ACTIVITY, new SystemChangePageActivity(storyManager));
            map.Add(EBookConstant.INTERNAL_PLAY_ANIMATION, new SystemPlayAnimationActivity());
            map.Add(EBookConstant.INTERNAL_CHANGE_BACKGROUND, new SystemChangeBackgroundActivity());

            //audio related activities
            map.Add(EBookConstant.INTERNAL_REPLAY_ADUIO, new SystemReplayAudioActivity());
            map.Add(EBookConstant.INTERNAL_FINISH_REPLAY_AUDIO, new SystemFinishReplayAudioActivity(storyManager));
            return map;
        }
    }
}