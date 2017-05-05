using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.devices
{
    /*
     * This class responsible for calling client's browser to update graphic & text.
     */
    public class EBookBrowserDisplayDevice : AbstractDevice
    {
        public void enableAcceptRejectButton(bool b)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();
            context.Clients.All.disableAcceptRejectButton(!b);
        }

        public void chooseStory(string[] storyName)
        {
            //string[] storyName = eBookSystem.getStories();
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.chooseStory(storyName);
        }

        public void createAcceptRejectButtons()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.createAcceptRejectButtons();
        }

        public void createPauseResumeButton()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.createPauseResumeButton();
        }

        public void DisplayStoryText(string text, int pageNum)
        {
            text = text.Replace("\\s+", " ");
            text = text.Trim();
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.setPageText(text, pageNum);
        }

        public void changeBackgroundImage(string path)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.setBackgroundImage(path);
        }

        public void playAnimation(int ani)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.playAnimation(ani);
        }



        public void chooseVoice(string voicePath)
        {
            List<string> listVoice = new List<string>();
            foreach (string d in Directory.GetDirectories(voicePath))
            {
                string dName = new DirectoryInfo(d).Name;
                listVoice.Add(dName);
            }
            string[] arr = new string[listVoice.Count];
            for (int i = 0; i < listVoice.Count; i++)
            {
                arr[i] = listVoice.ElementAt(i);
            }
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.chooseVoice(arr);
        }

        public void askOverwriteExistVoiceName()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();

            context.Clients.All.voiceDirExist();
        }

        public override string getDeviceName()
        {
            return "browser display device";
        }
    }
}