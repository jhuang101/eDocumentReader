using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.activities.user;
using eDocumentReader.Hubs.activities.system.lightweight;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// An interactive eDocument reader system. The system allow 
    /// reader to read a story using a microphone; the system will play
    /// the animations corresponding to what reader read. The system also
    /// highlights what text the reader has been read.
    /// The system has three mode {read the story now, record my voice, replay my voice}
    /// The "read the story now" mode allows the reader to reader a story in realtime.
    /// The "record my voice" mode allows the reader to record his/her voice for later.
    /// The "replay my voice" mode allows the reader to replay the recorded voice.
    /// </summary>
    public class EBookInteractiveSystem : AbstractInteractiveSystem
    {
        //Use this parameter to limit number of words in one single speech. 
        public static readonly INPUT_STREAM_TYPE STREAM_TYPE = INPUT_STREAM_TYPE.WEB_RTC;//stream audio from web or default internal mic 
        public static readonly int MAX_WORDS_IN_SPEECH = 30;
        public static readonly string configFileName = "setting.config";
        public static readonly string voice_dir = "voice";

        //private StoryManager storyManager;
        private static EBookActivityModel activityModel;
        private static EBookDeviceManager deviceManager;

        public static int lookAheadDivider = 4; //default is 4
        public static int initialLookAhead;
        public static int initialNoiseSensitivity;
        public static int commandConfidenceThreshold;

        public EBookInteractiveSystem()
        {
            activityModel = new EBookActivityModel();
            deviceManager = new EBookDeviceManager();
        }
        /// <summary>
        /// Initiate the system
        /// </summary>
        /// <param name="storyDirectory">The path to the story folder</param>
        public void init(string storyDirectory)
        {
            activityModel.init(storyDirectory);
            fillParameters(storyDirectory);
        }

        /// <summary>
        /// Parse the config file and extract parameters.
        /// </summary>
        /// <param name="storyDirectory">the path to the story folder</param>
        private void fillParameters(string storyDirectory)
        {
            //load parameters from config file
            string cfn = storyDirectory + "\\" + configFileName;
            XmlDocument doc = new XmlDocument();
            doc.Load(cfn);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name.CompareTo("lookAheadDivider") == 0)
                {
                    if (node.Attributes != null)
                    {
                        for (int i = 0; i < node.Attributes.Count; i++)
                        {
                            if (node.Attributes.Item(i).Name.CompareTo("value") == 0)
                            {
                                lookAheadDivider = Convert.ToInt32(node.Attributes.Item(i).Value);
                            }
                        }
                    }
                }
                else if (node.Name.CompareTo("initialLookAhead") == 0)
                {
                    if (node.Attributes != null)
                    {
                        for (int i = 0; i < node.Attributes.Count; i++)
                        {
                            if (node.Attributes.Item(i).Name.CompareTo("value") == 0)
                            {
                                initialLookAhead = Convert.ToInt32(node.Attributes.Item(i).Value);
                            }
                        }
                    }
                }
                else if (node.Name.CompareTo("initialNoiseSensitivity") == 0)
                {
                    if (node.Attributes != null)
                    {
                        for (int i = 0; i < node.Attributes.Count; i++)
                        {
                            if (node.Attributes.Item(i).Name.CompareTo("value") == 0)
                            {
                                initialNoiseSensitivity = Convert.ToInt32(node.Attributes.Item(i).Value);
                            }
                        }
                    }
                }
                else if (node.Name.CompareTo("commandConfidenceThreshold") == 0)
                {
                    if (node.Attributes != null)
                    {
                        for (int i = 0; i < node.Attributes.Count; i++)
                        {
                            if (node.Attributes.Item(i).Name.CompareTo("value") == 0)
                            {
                                commandConfidenceThreshold = Convert.ToInt32(node.Attributes.Item(i).Value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restart the system
        /// </summary>
        public void restart()
        {
            ActivityExecutor.add(new UserRestartSystemActivity()); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>activity model for the system</returns>
        public override AbstractActivityModel createActivityModel()
        {
            return activityModel;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>deivce manager for the system</returns>
        public override AbstractDeviceManager createDeviceManager()
        {
            return deviceManager;
        }
    }
}