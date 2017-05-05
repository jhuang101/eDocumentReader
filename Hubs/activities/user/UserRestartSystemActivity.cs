using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserRestartSystemActivity : AbstractUserActivity
    {

        public override string getPropertyId()
        {
            return EBookConstant.USER_RESTART_SYSTEM;
        }
    }
}