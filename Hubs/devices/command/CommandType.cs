using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.devices.command
{
    public enum CommandType
    {
        CHANGE_BACKGROUND,
        UPDATE_PAGE_TEXT,
        REPLAY_AUDIO,
        CREATE_PAUSE_RESUME_BUTTON,
        CREATE_ACCEPT_REJECT_BUTTON,
        LOG_SPEECH_STATE,
        LOG_RECOGNITION_RESULT,
        ENABLE_ACCEPT_REJECT_BUTTON,
        INIT_DEVICES_FOR_REALTIME,
        ENABLE_STORY_LOGGER,
        ASK_USER_CHOOSE_VOICE,
        ASK_USER_OVERWRITE_VOICE,
        LOG_REJECT_LAST_RECOGNITION,
        PLAY_ANIMATION,
        CONFIRM_AND_SAVE_SPEECH,
        CLEAN_UNCONFIRMED_SPEECH,
        LOG_PAGE_END,
        RESTART_SYSTEM,
        LOAD_COMMAND_GRAMMAR,
        INIT_DEVICES_FOR_RECORD,
        INIT_DEVICE_FOR_REPLAY,
        RELOAD_ONGOING_GRAMMAR,
        CHANGE_GRAMMAR_PRIORITY,
        DISPLAY_MAIN_PAGE,
        RELOAD_STORY_GRAMMAR,

    }
}