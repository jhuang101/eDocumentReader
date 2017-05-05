using eDocumentReader.Hubs.activities.system.lightweight;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.devices
{
    public class EBookAudioPlayer : AbstractDevice
    {

        public void playAudio(string audioPath, int audioIndex)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();
            context.Clients.All.playAudio(audioPath, "audio/wav", audioIndex);
        }

        public void finishPlayingAudio(int audioIndex)
        {
            ActivityExecutor.add(new InternalFinishReplayAudioActivity(audioIndex));

        }
        //
        public void audioUpdateTime(double time)
        {

        }

        public override string getDeviceName()
        {
            return "audio output device";
        }
    }
}