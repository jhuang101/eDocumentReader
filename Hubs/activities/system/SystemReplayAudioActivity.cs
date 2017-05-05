using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.devices;
using eDocumentReader.Hubs.devices.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.activities.system
{
    public class SystemReplayAudioActivity : AbstractSystemActivity
    {
        public override void execute(List<Activity> historyList)
        {
            if (relActivity is InternalReplayAudioActivity)
            {
                InternalReplayAudioActivity act = (InternalReplayAudioActivity)relActivity;
                string audioName = act.getAudioName();
                int audioIndex = act.getAudioIndex();
                Command comm = new Command(CommandType.REPLAY_AUDIO);
                comm.addData(audioName);
                comm.addData(audioIndex);
                AbstractDeviceManager.executeCommand(comm);
            }
        }

        public override string getPropertyId()
        {
            return EBookConstant.SYSTEM_REPLAY_AUDIO;
        }
    }
}