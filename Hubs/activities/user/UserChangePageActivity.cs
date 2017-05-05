using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserChangePageActivity : AbstractUserActivity
    {
        public PageAction action;
        public int pageNum;

        public UserChangePageActivity(PageAction pa)
        {
            action = pa;
        }
        public UserChangePageActivity(PageAction pa, int pageNum)
        {
            action = pa;
            this.pageNum = pageNum;
        }
        public PageAction getPageAction()
        {
            return action;
        }
        public int getPageNumber()
        {
            return pageNum;
        }

        public override string getPropertyId()
        {
            return EBookConstant.USER_CHANGE_PAGE;
        }
    }
}