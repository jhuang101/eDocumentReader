using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR.Hubs;
using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.activities.system.lightweight;

namespace eDocumentReader.Hubs
{
    public class EBookHub : Hub
    {
        public static string story_path = "Content\\stories";
 
        private static EBookInteractiveSystem eBookSystem;

        private static ConcurrentQueue<byte[]> conQueue = new ConcurrentQueue<byte[]>();

        public void Init()
        {
            if (eBookSystem == null)
            {
                eBookSystem = new EBookInteractiveSystem();
                story_path = HttpContext.Current.Server.MapPath("~")+story_path;
                eBookSystem.init(story_path);
                eBookSystem.start();
            }
            else
            {
                eBookSystem.restart();
                
            }
            ActivityExecutor.add(new InternalDisplayMainPageActivity());

        }
    }
}