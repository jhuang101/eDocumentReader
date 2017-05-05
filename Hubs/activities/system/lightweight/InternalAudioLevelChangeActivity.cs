using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalAudioLevelChangeActivity : Activity
    {
        private int audioLevel;
        private int audioState;
        private double startTime;

        public InternalAudioLevelChangeActivity(int audioLevel, int audioState, double startTime)
        {
            this.audioLevel = audioLevel;
            this.audioState = audioState;
            this.startTime = startTime;
        }
        public int getAudioLevel() { return audioLevel; }
        public int getAudioState() { return audioState; }
        public double getAudioStartTime() { return startTime; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_AUDIO_LEVEL_CHANGE;
        }
    }
}