using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalUpdatePageTextActivity : Activity
    {
        string pageText;
        int pageNum;
        public InternalUpdatePageTextActivity(string pageText, int pageNum)
        {
            this.pageText = pageText;
            this.pageNum = pageNum;
        }
        public string getPageText() { return pageText; }
        public int getPageNum() { return pageNum; }

        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_UPDATE_PAGE_TEXT;
        }
    }
}