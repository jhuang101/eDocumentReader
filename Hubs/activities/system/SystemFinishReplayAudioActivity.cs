using eDocumentReader.Hubs.activities.system.lightweight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemFinishReplayAudioActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;

        public SystemFinishReplayAudioActivity(StoryManager sm)
        {
            storyManager = sm;
        }

        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalFinishReplayAudioActivity)
            {
                int audioIndex = ((InternalFinishReplayAudioActivity)relActivity).getAudioIndex();
                storyManager.finishReplayAudio(audioIndex);
            }
        }
        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_FINISH_REPLAY_AUDIO;
        }
    }
}