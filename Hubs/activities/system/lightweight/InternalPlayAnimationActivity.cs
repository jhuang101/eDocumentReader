using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalPlayAnimationActivity : Activity
    {
        private int animationId;
        public InternalPlayAnimationActivity(int animationId){
            this.animationId = animationId;
        }

        public int getAnimationId() { return animationId; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_PLAY_ANIMATION;
        }
    }
}