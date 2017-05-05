using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    [Obsolete("This class is deprecated, use SystemUpdatePageTextActivity instead.")]
    public class SystemUpdateTextHighLightActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            InternalUpdateTextHighLightActivity act = (InternalUpdateTextHighLightActivity)relActivity;
            //AbstractEBookEvent.raise(new UpdateSpeechTextEvent(act.getHTMLText()));
            Command comm = new Command(CommandType.UPDATE_PAGE_TEXT);
            comm.addData(act.getHTMLText());
            comm.addData(-1);
            AbstractDeviceManager.executeCommand(comm);
            //EBookBrowserDisplayDevice.DisplayStoryText(act.getHTMLText(), -1);
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_UPDATE_HTML_TEXT;
        }
    }
}