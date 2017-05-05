using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemUpdatePageTextActivity : AbstractSystemActivity
    {
        public SystemUpdatePageTextActivity()
        {
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalUpdatePageTextActivity)
            {
                InternalUpdatePageTextActivity act = (InternalUpdatePageTextActivity)relActivity;
                //AbstractEBookEvent.raise(new UpdatePageTextEvent(act.getPageText(), act.getPageNum()));
                Command comm = new Command(CommandType.UPDATE_PAGE_TEXT);
                comm.addData(act.getPageText());
                comm.addData(act.getPageNum());
                AbstractDeviceManager.executeCommand(comm);
            }
            else if (relActivity is InternalUpdateTextHighLightActivity)
            {
                InternalUpdateTextHighLightActivity act = (InternalUpdateTextHighLightActivity)relActivity;
                //AbstractEBookEvent.raise(new UpdateSpeechTextEvent(act.getHTMLText()));
                Command comm = new Command(CommandType.UPDATE_PAGE_TEXT);
                comm.addData(act.getHTMLText());
                comm.addData(-1);
                AbstractDeviceManager.executeCommand(comm);
                //EBookBrowserDisplayDevice.DisplayStoryText(act.getHTMLText(), -1);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_UPDATE_PAGE_TEXT;
        }
    }
}