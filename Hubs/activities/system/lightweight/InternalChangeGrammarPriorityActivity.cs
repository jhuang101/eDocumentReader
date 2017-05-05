using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalChangeGrammarPriorityActivity : Activity
    {
        private int priority;
        public InternalChangeGrammarPriorityActivity(int p)
        {
            priority = p;
        }
        public int getPriority() { return priority; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_CHANGE_GRAMMAR_PRIORITY;
        }
    }
}