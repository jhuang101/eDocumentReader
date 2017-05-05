using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalSpeechStateChangeActivity : Activity
    {
        SpeechState speechState;
        public InternalSpeechStateChangeActivity(SpeechState speechState)
        {
            this.speechState = speechState;
        }

        public SpeechState getSpeechState() { return speechState; }



        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_SPEECH_STATE_CHANGE;
        }
    }
}