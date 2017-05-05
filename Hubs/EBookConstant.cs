using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// A unify class to hold all constants in this system
    /// </summary>
    public class EBookConstant
    {

        //for user activities
        public static readonly string USER_CHANGE_PAGE = "USER_CHANGE_PAGE";
        public static readonly string USER_PAUSE_RESUME = "USER_PAUSE_RESUME";
        public static readonly string USER_ACCEPT_RECORDED_SPEECH = "USER_ACCEPT_RECORDED_SPEECH";
        public static readonly string USER_SELECT_STORY_MODE = "USER_CHANGE_STORY_MODE";
        public static readonly string USER_SELECT_STORY_NAME = "USER_SELECT_STORY_NAME";
        public static readonly string USER_SET_RECORD_VOICE_NAME = "USER_SET_RECORD_VOICE_NAME";
        public static readonly string USER_START_RECORDING = "USER_START_RECORDING";
        public static readonly string USER_START_REPLAYING_VOICE = "USER_START_REPLAYING_VOICE";
        public static readonly string USER_RESTART_SYSTEM = "USER_RESTART_SYSTEM";

        //for system activities
        public static readonly string SYSTEM_CHANGE_PAGE = "SYSTEM_CHANGE_PAGE";
        public static readonly string SYSTEM_PAUSE_RESUME = "SYSTEM_PAUSE_RESUME";
        public static readonly string SYSTEM_ACCEPT_RECORDED_SPEECH = "SYSTEM_ACCEPT_RECORDED_SPEECH";
        public static readonly string SYSTEM_CHANGE_STORY_MODE = "SYSTEM_CHANGE_STORY_MODE";
        public static readonly string SYSTEM_SELECT_STORY_NAME = "SYSTEM_SELECT_STORY_NAME";
        public static readonly string SYSTEM_SET_RECORD_VOICE_NAME = "SYSTEM_SET_RECORD_VOICE_NAME";
        public static readonly string SYSTEM_AUDIO_LEVEL_CHANGE = "SYSTEM_AUDIO_LEVEL_CHANGE";
        public static readonly string SYSTEM_SPEECH_STATE_CHANGE = "SYSTEM_SPEECH_STATE_CHANGE";
        public static readonly string SYSTEM_SPEECH_RECOGNITION_RESULT = "SYSTEM_SPEECH_RECOGNITION_RESULT";
        public static readonly string SYSTEM_UPDATE_PAGE_TEXT = "SYSTEM_UPDATE_PAGE_TEXT";
        public static readonly string SYSTEM_UPDATE_HTML_TEXT = "SYSTEM_UPDATE_HTML_TEXT";
        public static readonly string SYSTEM_START_RECORDING = "SYSTEM_START_RECORDING";
        public static readonly string SYSTEM_START_REPLAYING = "SYSTEM_START_REPLAYING";
        public static readonly string SYSTEM_FINISH_REPLAY_AUDIO = "SYSTEM_FINISH_REPLAY_AUDIO";
        public static readonly string SYSTEM_PLAY_ANIMATION = "SYSTEM_PLAY_ANIMATION";
        public static readonly string SYSTEM_CHANGE_BACKGROUND = "SYSTEM_CHANGE_BACKGROUND";
        public static readonly string SYSTEM_REPLAY_AUDIO = "SYSTEM_REPLAY_AUDIO";
        public static readonly string SYSTEM_SIMULATE_RECOGNITION_RESULT = "SYSTEM_SIMULATE_RECOGNITION_RESULT";
        public static readonly string SYSTEM_RESTART = "SYSTEM_RESTART";
        public static readonly string SYSTEM_REJECT_RECOGNITION = "SYSTEM_REJECT_RECOGNITION";
        public static readonly string SYSTEM_RELOAD_ONGOING_GRAMMAR = "SYSTEM_RELOAD_ONGOING_GRAMMAR";
        public static readonly string SYSTEM_CHANGE_GRAMMAR_PRIORITY = "SYSTEM_CHANGE_GRAMMAR_PRIORITY";
        public static readonly string SYSTEM_DISPLAY_MAIN_PAGE = "SYSTEM_DISPLAY_MAIN_PAGE";
        public static readonly string SYSTEM_RELOAD_STORY_GRAMMAR = "SYSTEM_RELOAD_STORY_GRAMMAR";

        //for lightweight internal activities
        public static readonly string INTERNAL_REJECT_RECOGNITION = "INTERNAL_REJECT_RECOGNITION";
        public static readonly string INTERNAL_AUDIO_LEVEL_CHANGE = "INTERNAL_AUDIO_LEVEL_CHANGE";
        public static readonly string INTERNAL_SPEECH_STATE_CHANGE="INTERNAL_SPEECH_STATE_CHANGE";
        public static readonly string INTERNAL_SPEECH_RECOGNITION_RESULT = "INTERNAL_SPEECH_RECOGNITION_RESULT";
        public static readonly string INTERNAL_UPDATE_PAGE_TEXT = "INTERNAL_UPDATE_PAGE_TEXT";
        public static readonly string INTERNAL_UPDATE_HTML_TEXT = "INTERNAL_UPDATE_HTML_TEXT";
        public static readonly string INTERNAL_SPEECH_NAVIGATE_PAGE = "INTERANL_SPEECH_NAVIGATE_PAGE";
        public static readonly string INTERNAL_FINISH_PAGE_ACTIVITY = "INTERNAL_FINISH_PAGE_ACTIVITY";
        public static readonly string INTERNAL_FINISH_REPLAY_AUDIO = "INTERNAL_FINISH_REPLAY_AUDIO";
        public static readonly string INTERNAL_PLAY_ANIMATION = "INTERNAL_PLAY_ANIMATION";
        public static readonly string INTERNAL_CHANGE_BACKGROUND = "INTERNAL_CHANGE_BACKGROUND";
        public static readonly string INTERNAL_REPLAY_ADUIO = "INTERNAL_REPLAY_ADUIO";
        public static readonly string INTERNAL_SIMULATE_RECOGNITION_RESULT = "INTERNAL_SIMULATE_RECOGNITION_RESULT";
        public static readonly string INTERNAL_RELOAD_ONGOING_GRAMMAR = "INTERNAL_RELOAD_ONGOING_GRAMMAR";
        public static readonly string INTERNAL_CHANGE_GRAMMAR_PRIORITY = "INTERNAL_CHANGE_GRAMMAR_PRIORITY";
        public static readonly string INTERNAL_DISPLAY_MAIN_PAGE = "INTERNAL_DISPLAY_MAIN_PAGE";
        public static readonly string INTERNAL_RELOAD_STORY_GRAMMAR = "INTERNAL_RELOAD_STORY_GRAMMAR";


        public static readonly float DEFAULT_WEIGHT = 0.5f;
        public static readonly int DEFAULT_PRIORITY = 0;

        public static readonly float NEXT_WORD_WEIGHT = 1f;
        public static readonly int NEXT_WORD_PRIORITY = 1;

        public static readonly string[] PAUSE_PUNCTUATION = {"?", "!",".",","};
        public static readonly string[] EOS_PUNCTUATION = { "?", "!", "."};
        
    }
}