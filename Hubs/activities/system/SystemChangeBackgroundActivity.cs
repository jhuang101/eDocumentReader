using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemChangeBackgroundActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalChangeBackgroundActivity)
            {
                string bg = ((InternalChangeBackgroundActivity)relActivity).getBackgroundImage();
                Command comm = new Command(CommandType.CHANGE_BACKGROUND);
                comm.addData(bg);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_CHANGE_BACKGROUND;
        }
    }
}