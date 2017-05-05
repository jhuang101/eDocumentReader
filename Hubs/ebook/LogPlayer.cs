using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Speech.Recognition;
using eDocumentReader.Hubs.activities.system.lightweight;

namespace eDocumentReader.Hubs
{
    public class LogPlayer
    {
        private List<string> audioPlayList; //store the browser playable path of the audio file
        private Dictionary<string, List<RecognitionResult>> eventList;
        private bool run;
        private System.Media.SoundPlayer player;
        private Thread threax;
        private int currentAudioIndex;

        public LogPlayer()
        {
            eventList = new Dictionary<string, List<RecognitionResult>>();
            audioPlayList = new List<string>();
        }

        public void processAudioFiles(string dir)
        {
            audioPlayList.Clear();
            foreach (string fileName in Directory.GetFiles(dir))
            {
                if (fileName.EndsWith(".wav"))
                {
                    //need to find a way not to hard code "Content" here
                    string[] stringSeparator = {"Content"};
                    string[] result = fileName.Split(stringSeparator, StringSplitOptions.None);
                    if(result.Length > 1){
                        audioPlayList.Add("../Content/" + result[1]);
                        Debug.WriteLine(fileName);
                    }
                }
                else if (fileName.EndsWith(StoryLoggingDevice.logFileName))
                {
                    eventList =  importResultData(fileName);
                }
            }

            //need to remove any audio files that are not appeared in the log file
            List<string> tobeRemoved = new List<string>();
            foreach (string wavPath in audioPlayList)
            {
                if (!eventList.ContainsKey(wavPath))
                {
                    tobeRemoved.Add(wavPath);
                }
            }
            foreach (string rm in tobeRemoved)
            {
                audioPlayList.Remove(rm);
                Debug.WriteLine("audio file: " + rm + " isn't belong to the recording. Removed.");
            }

            Thread thread = new Thread(new ThreadStart(replayAudioInClient));
            thread.Start();
        }

        private Dictionary<string, List<RecognitionResult>> importResultData(string fileName)
        {
            Dictionary<string, List<RecognitionResult>> eventList = new Dictionary<string, List<RecognitionResult>>();
            string line;
            double startTime = 0;
            StreamReader reader = new StreamReader(fileName);
            RecognitionResult recEvent = null;
            List<RecognitionResult> tempList = new List<RecognitionResult>();

            while ((line = reader.ReadLine()) != null)
            {
                string[] sp = line.Split(new string[]{"|"}, StringSplitOptions.RemoveEmptyEntries);


                    if (line.StartsWith("start:"))
                    {
                        string[] sp2 = sp[0].Split(new string[]{"::"}, StringSplitOptions.RemoveEmptyEntries);
                        startTime = Convert.ToDouble(sp2[1]);

                        

                    }
                    else if(line.StartsWith("time:")){
                        recEvent = new RecognitionResult();
                        List<KeyValuePair<string, SemanticValue>> semanticList = new List<KeyValuePair<string, SemanticValue>>();
                        int index = 0;
                        while(sp.Length > index)
                        {
                            string[] sp2 = sp[index].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                            if (sp2[0].CompareTo(StoryLoggingDevice.time_tag) == 0)
                            {
                                double time = Convert.ToDouble(sp2[1]);
                                time -= startTime;
                                recEvent.startTime = time;
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.confidence_tag) == 0)
                            {
                                recEvent.confidence = float.Parse(sp2[1]);
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.textResult_tag) == 0)
                            {
                                recEvent.textResult = sp2[1];
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.isHyp_tag) == 0)
                            {
                                recEvent.isHypothesis = Convert.ToBoolean(sp2[1]);
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.grammarName_tag) == 0)
                            {
                                if (sp2.Length > 1)
                                {
                                    recEvent.grammarName = sp2[1];
                                }
                                else
                                {
                                    recEvent.grammarName = "";
                                }
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.ruleName_tag) == 0)
                            {
                                if (sp2.Length > 1)
                                recEvent.ruleName = sp2[1];
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.duration_tag) == 0)
                            {
                                if (sp2.Length > 1)
                                recEvent.audioDuration = Convert.ToDouble(sp2[1]);
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.wavePath_tag) == 0)
                            {
                                if (sp2.Length > 1)
                                recEvent.wavPath = sp2[1];
                            }
                            else if (sp2[0].CompareTo(StoryLoggingDevice.key_tag) == 0)
                            {
                                string key = sp2[1];
                                index++;
                                string[] spx = sp[index].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                                if (spx[0].CompareTo(StoryLoggingDevice.value_tag) == 0 && spx.Length == 2)
                                {
                                    string value = spx[1];
                                    KeyValuePair<string, SemanticValue> kvp = new KeyValuePair<string, SemanticValue>(key,new SemanticValue(new String(value.ToCharArray())));
                                    semanticList.Add(kvp);
                                }

                            }

                            index++;
                        }
                        KeyValuePair<string, SemanticValue>[] tmp = new KeyValuePair<string, SemanticValue>[semanticList.Count];
                        if (semanticList.Count > 0)
                        {
                            for(int i=0;i<semanticList.Count;i++){
                                tmp[i] = semanticList.ElementAt(i);
                            }
                            
                        }
                        recEvent.semanticResult = tmp;

                        string path = recEvent.wavPath;
                        if (!recEvent.isHypothesis && path != null)
                        {
                            List<RecognitionResult> list;
                            eventList.TryGetValue(path, out list);
                            if (list == null)
                            {
                                list = new List<RecognitionResult>();
                                eventList.Add(path, list);
                            }
                            for (int i = 0; i < tempList.Count; i++)
                            {
                                list.Add(tempList.ElementAt(i));
                            }
                            tempList.Clear();
                            list.Add(recEvent);

                        }
                        else
                        {
                            //add the event to a temp list
                            tempList.Add(recEvent);
                        }

                    }
                    else if(line.StartsWith("finishPage")){
                        if (recEvent != null)
                        {
                            recEvent.endPage = true;
                        }
                    }
            }
            return eventList;
        }

        public void stop()
        {
            run = false;
            if (threax != null)
            {
                threax.Abort();
            }
            if (player != null) { 
                player.Stop();
            }
        }

        public void pause()
        {
            run = false;
        }
        public void resume()
        {
            run = true;
            playSyncAudio();
        }

        /*
         * It will replay the audio file within the same machine that runs the server.
         * see replayAudioInClient() for replay audio file in client browser
         */
        public void replayFile()
        {
            run = true;
            Thread.Sleep(2000);
            currentAudioIndex = 0;
            while (currentAudioIndex < audioPlayList.Count && run)
            {
                player = new System.Media.SoundPlayer(audioPlayList.ElementAt(currentAudioIndex));

                threax = new Thread(() => regenerateSREvent(currentAudioIndex));
                threax.Start();

                player.PlaySync();
                threax.Join();
                Thread.Sleep(500);
                currentAudioIndex++;
            }

        
        }

        public void replayAudioInClient()
        {
            run = true;
            Thread.Sleep(1500);
            currentAudioIndex = 0;
            playSyncAudio();
            
        }

        public void playSyncAudio()
        {
            if (run)
            {
                Thread.Sleep(500);
                if (threax != null && threax.IsAlive)
                {
                    threax.Join();
                }
                Debug.WriteLine("##start replay audio [" + currentAudioIndex + "]");
                if (currentAudioIndex < audioPlayList.Count && run)
                {
                    string audioPath = audioPlayList.ElementAt(currentAudioIndex);
                    int pas = currentAudioIndex;
                    threax = new Thread(() => regenerateSREvent(pas));
                    threax.Start();

                    ActivityExecutor.add(new InternalReplayAudioActivity(audioPath, currentAudioIndex));

                    currentAudioIndex++;
                }
            }
        }

        /*
         *re-generate the SR from the saved event file 
         */
        private void regenerateSREvent(int index)
        {
            Debug.WriteLine("##start re post SR event [" + index+"]");
            //The audio may take longer to be played in browser
            //force to wait some milliseconds 
            Thread.Sleep(100);

            double startTime = GetUnixTimestamp();
            List<RecognitionResult> events;
            eventList.TryGetValue(audioPlayList.ElementAt(index), out events);
            //generate recognition events
            if (events != null)
                while (events.Count > 0)
                {
                    RecognitionResult rrv = events.ElementAt(0);
                    events.Remove(rrv);
                    double hitTime = rrv.startTime + startTime;
                    while (hitTime - GetUnixTimestamp() > 10)
                    {
                        Thread.Sleep(5);
                    }
                    ActivityExecutor.add(new InternalSpeechRecognitionResultActivity(rrv.confidence, rrv.textResult, rrv.isHypothesis,
                rrv.semanticResult, rrv.grammarName, rrv.ruleName, rrv.audioDuration, rrv.wavPath));

                    Debug.WriteLine("raise time: " + GetUnixTimestamp() + "\t" + rrv.isHypothesis+"\tendPage: "+rrv.endPage);
                    if (rrv.endPage)
                    {
                        Thread.Sleep(1000);
                    }
                    
                }
        }
        public static double GetUnixTimestamp()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        public void finishReplayAudio(int audioIndex)
        {
            if (audioIndex + 1 != currentAudioIndex)
            {
                Debug.WriteLine("*** Audio index [" + audioIndex + "]mismatch***");
            }
            else
            {
                playSyncAudio();
            }
        }
    }
}