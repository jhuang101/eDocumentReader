using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalReplayAudioActivity : Activity
    {
        private string audioName;
        private int audioIndex;

        public InternalReplayAudioActivity(string audioName, int audioIndex)
        {
            this.audioName = audioName;
            this.audioIndex = audioIndex;
        }
        public string getAudioName() { return audioName; }
        public int getAudioIndex() { return audioIndex; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_REPLAY_ADUIO;
        }
    }
}