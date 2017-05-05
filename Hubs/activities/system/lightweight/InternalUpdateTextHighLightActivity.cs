using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalUpdateTextHighLightActivity : Activity
    {
        string htmlText;
        public InternalUpdateTextHighLightActivity(string htmlText)
        {
            this.htmlText = htmlText;
        }
        public string getHTMLText() { return htmlText; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_UPDATE_HTML_TEXT;
        }
    }
}