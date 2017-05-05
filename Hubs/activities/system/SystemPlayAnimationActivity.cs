using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemPlayAnimationActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalPlayAnimationActivity)
            {
                int animationId = ((InternalPlayAnimationActivity)relActivity).getAnimationId();
                Command comm = new Command(CommandType.PLAY_ANIMATION);
                comm.addData(animationId);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_PLAY_ANIMATION;
        }
    }
}