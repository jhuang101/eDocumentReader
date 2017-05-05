using eDocumentReader.Hubs.devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// An Abstract class of the interactive system. 
    /// All interactive system should have a activity model and a 
    /// device manager. Any derived class must implement createAcivityModel() 
    /// and createDeviceManager() methods.
    /// </summary>
    public abstract class AbstractInteractiveSystem
    {
        private ActivityExecutor executor;

        public AbstractInteractiveSystem()
        {
        }
        public abstract AbstractActivityModel createActivityModel();
        public abstract AbstractDeviceManager createDeviceManager();

        /// <summary>
        /// Create and start all neccessary components in the system.
        /// </summary>
        public void start()
        {
            AbstractDeviceManager deviceManager = createDeviceManager();
            deviceManager.start();

            AbstractActivityModel activityModel = createActivityModel();
            executor = new ActivityExecutor(activityModel);
            executor.start();
        }
    }
}