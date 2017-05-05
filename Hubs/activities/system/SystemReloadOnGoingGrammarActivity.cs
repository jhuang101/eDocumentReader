using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemReloadOnGoingGrammarActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalReloadOnGoingGrammarActivity)
            {
                Grammar g = ((InternalReloadOnGoingGrammarActivity)relActivity).getGrammar();
                Command comm = new Command(CommandType.RELOAD_ONGOING_GRAMMAR);
                comm.addData(g);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_RELOAD_ONGOING_GRAMMAR;
        }
    }
}