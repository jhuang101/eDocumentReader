using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.user
{
    public class UserStartRecordingActivity : AbstractUserActivity
    {

        public override string getPropertyId()
        {
            return EBookConstant.USER_START_RECORDING;
        }
    }
}