using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalReloadOnGoingGrammarActivity : Activity
    {
        private Grammar grammar;
        public InternalReloadOnGoingGrammarActivity(Grammar g)
        {
            grammar = g;
        }

        public Grammar getGrammar() { return grammar; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_RELOAD_ONGOING_GRAMMAR;
        }
    }
}