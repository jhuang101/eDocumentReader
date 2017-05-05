using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /**
     * Device represents the IO interface. The Device class here already
     * extends the Hub, so all subclass does not need to do that.
     * Any subclass that gets data from client must implement methods that
     * can receive calls from the client.
     */
    public abstract class AbstractDevice : Hub
    {
        protected bool active = true; //deivces are enabled by default
        public void enable(bool act)
        {
            active = act;
        }
        public abstract string getDeviceName();
    }
}