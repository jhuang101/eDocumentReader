using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemChangeGrammarPriorityActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalChangeGrammarPriorityActivity)
            {
                int priority = ((InternalChangeGrammarPriorityActivity)relActivity).getPriority();
                Command comm = new Command(CommandType.CHANGE_GRAMMAR_PRIORITY);
                comm.addData(priority);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_CHANGE_GRAMMAR_PRIORITY;
        }
    }
}