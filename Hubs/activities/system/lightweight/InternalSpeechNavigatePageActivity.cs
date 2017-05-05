using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalSpeechNavigatePageActivity : Activity
    {
        PageAction pageAction;
        int pageNumber;
        public InternalSpeechNavigatePageActivity(PageAction pa)
        {
            pageAction = pa;

        }
        public InternalSpeechNavigatePageActivity(PageAction pa, int pageN)
        {
            pageAction = pa;
            pageNumber = pageN;

        }
        public PageAction getPageAction() { return pageAction; }
        public int getPageNumber() { return pageNumber; }


        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_SPEECH_NAVIGATE_PAGE;
        }
    }
}