using eDocumentReader.Hubs.activities.system.lightweight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemAudioLevelChangeActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;
        public SystemAudioLevelChangeActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            InternalAudioLevelChangeActivity ialc = (InternalAudioLevelChangeActivity)relActivity;
            storyManager.updateAcousticHypothesis(ialc.getAudioState(), ialc.getAudioStartTime());
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_AUDIO_LEVEL_CHANGE;
        }
    }
}