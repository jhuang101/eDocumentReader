using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemChangePageActivity : AbstractSystemActivity
    {
        private StoryManager storyManager;

        public SystemChangePageActivity(StoryManager sm)
        {
            storyManager = sm;
        }
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is UserChangePageActivity)
            {
                UserChangePageActivity activity = (UserChangePageActivity)relActivity;
                PageAction pa = activity.getPageAction();
                int pageNum = activity.getPageNumber();
                storyManager.changePage(pa, pageNum);
            }
            else if (relActivity is InternalSpeechNavigatePageActivity)
            {
                InternalSpeechNavigatePageActivity activity = (InternalSpeechNavigatePageActivity)relActivity;
                PageAction pa = activity.getPageAction();
                int pageNum = activity.getPageNumber();
                storyManager.changePage(pa, pageNum);
            }
            else if (relActivity is InternalFinishPageActivity)
            {
                InternalFinishPageActivity activity = (InternalFinishPageActivity)relActivity;
                storyManager.changePage(PageAction.NEXT,0);
                //AbstractEBookEvent.raise(new FinishPageEvent());
                if (storyManager.getStoryMode() == Mode.RECORD)
                {
                    AbstractDeviceManager.executeCommand(new Command(CommandType.LOG_PAGE_END));
                }
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_CHANGE_PAGE;
        }
    }
}