using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace eDocumentReader.Hubs.devices
{
    public abstract class AbstractDeviceManager
    {
        protected static bool on = true;
        private static readonly int IDLE_TIME = 5; //millisecond
        private static ConcurrentQueue<Command> queue;

        protected abstract List<AbstractDevice> deviceList
        {
            get;
        }

        public AbstractDeviceManager()
        {
            queue = new ConcurrentQueue<Command>();
        }
        public void start()
        {
            Thread oThread = new Thread(new ThreadStart(execThread));
            oThread.Start();
        }

        public void execThread()
        {
            while (on)
            {
                if (queue.Count == 0)
                {
                    Thread.Sleep(IDLE_TIME);
                }
                else
                {
                    Command comm;
                    if (queue.TryDequeue(out comm))
                    {
                        doWork(comm);
                    }
                }
            }
        }

        public void enableAllDevices(bool enable)
        {
            foreach (AbstractDevice d in deviceList)
            {
                d.enable(enable);
            }
        }
        public abstract void init();
        protected abstract void doWork(Object c);
        public static void executeCommand(Command c)
        {
            queue.Enqueue(c);
        }
    }
}