using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalReloadStoryGrammarActivity : Activity
    {
        private List<Grammar> storyGrammars;
        public InternalReloadStoryGrammarActivity(List<Grammar> g)
        {
            storyGrammars = g;
        }
        public List<Grammar> getGrammars() { return storyGrammars; }
        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_RELOAD_STORY_GRAMMAR;
        }
    }
}