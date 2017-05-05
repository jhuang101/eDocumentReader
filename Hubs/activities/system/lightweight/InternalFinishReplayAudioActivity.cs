using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalFinishReplayAudioActivity : Activity
    {
        private int audioIndex;
        public InternalFinishReplayAudioActivity(int audioIndex){
            this.audioIndex = audioIndex;
        }

        public int getAudioIndex() { return audioIndex; }


        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_FINISH_REPLAY_AUDIO;
        }
    }
}