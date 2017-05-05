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
    public class SystemReloadStoryGrammarActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalReloadStoryGrammarActivity)
            {
                List<Grammar> g = ((InternalReloadStoryGrammarActivity)relActivity).getGrammars();
                Command comm = new Command(CommandType.RELOAD_STORY_GRAMMAR);
                comm.addData(g);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_RELOAD_STORY_GRAMMAR;
        }
    }
}