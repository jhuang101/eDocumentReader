using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.IO;
using System.Speech.Recognition.SrgsGrammar;
using System.Threading;
using System.Xml;
using System.Collections.Concurrent;
using System.Speech.AudioFormat;
using eDocumentReader.Hubs.activities.system;
using eDocumentReader.Hubs.activities.system.lightweight;
using Microsoft.AspNet.SignalR;
using eDocumentReader.Hubs.structure;


namespace eDocumentReader.Hubs
{
    public enum INPUT_STREAM_TYPE
    {
        DEFAULT_AUDIO_DEVICE,
        WEB_RTC
    }
    /**
     * Singleton class, only one speech recognizer thru out the app
     */
    public class EBookSpeechRecognizer : AbstractDevice
    {

        public static readonly int UNAMBIGUOUS_SILENCE_TIMEOUT = 50;
        public static readonly int AMBIGUOUS_SILENCE_TIMEOUT = 500;

        private int MAX_WORDS_IN_SPEECH;

        //private static EBookSRDevice instance;
        private static SpeechRecognitionEngine recEngine;

        private static List<Grammar> currentStoryGrammars = new List<Grammar>();
        private static List<Grammar> onGoingGrammars = new List<Grammar>();
        private List<int> allDefaultStartIndexes = new List<int>();

        private Boolean recOn = false;
        //private double speechTurnAroundTime;
        //private List<List<double>> turnAroundTimes = new List<List<double>>();
        //private List<double> eachSpeechTimes;

        private string saveAudioPath;
        private bool loadGrammarCompleted = false; //used for debug purpose
        private int loadGrammarCount = 0; //used for debug purpose
        private int waveFileNameIndexer; 

        private List<string> confirmSaveAudioList = new List<string>();
        private List<string> unconfirmSaveAudioList = new List<string>();
        private static ConcurrentQueue<byte[]> conQueue = new ConcurrentQueue<byte[]>();

        private int preAudioLevel;
        //private double audioStartTime;

        private static INPUT_STREAM_TYPE audioStreamType;

        private EbookStream ebookStream;

        private bool startEnqueueAudio = true; //a flag to indicate whether to put received audio in the queue.

        public EBookSpeechRecognizer()
        {
            MAX_WORDS_IN_SPEECH = EBookInteractiveSystem.MAX_WORDS_IN_SPEECH;
            if (recEngine == null)
            {
                recEngine = new SpeechRecognitionEngine();
                recEngine.EndSilenceTimeout = TimeSpan.FromMilliseconds(UNAMBIGUOUS_SILENCE_TIMEOUT);
                recEngine.EndSilenceTimeoutAmbiguous = TimeSpan.FromMilliseconds(AMBIGUOUS_SILENCE_TIMEOUT);
                
                recEngine.SpeechRecognized += recEngine_SpeechRecognized;
                recEngine.SpeechHypothesized += recEngine_SpeechHypothesized;
                recEngine.SpeechRecognitionRejected += recEngine_SpeechRecognitionRejected;
                recEngine.SpeechDetected += recEngine_SpeechDetected;
                recEngine.AudioStateChanged += recEngine_AudioStateChanged;
                recEngine.LoadGrammarCompleted += recEngine_LoadGrammarCompleted;
                recEngine.RecognizeCompleted += recEngine_RecognizeCompleted;
                recEngine.AudioLevelUpdated += recEngine_AudioLevelUpdated;
            
            }
        }

        void recEngine_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            String timeStamp = GetTimestamp(DateTime.Now);
            Trace.WriteLine("audio level:"+e.AudioLevel+"\t"+timeStamp);
            if (e.AudioLevel > EBookInteractiveSystem.initialNoiseSensitivity)
            {
                int start = 0;
                double unixTime = EBookUtil.GetUnixTimeMillis();
                if (preAudioLevel == 0)
                {
                    //audio energy level jump
                    start = 1;
                    //audioStartTime = EBookUtil.GetUnixTimeMillis();
                }
                //AbstractEBookEvent.raise(new AudioLevelChangeEvent(e.AudioLevel, start, unixTime));
                ActivityExecutor.add(new InternalAudioLevelChangeActivity(e.AudioLevel,start,unixTime));
            }
            else if (e.AudioLevel == 0 && preAudioLevel > 0 && e.AudioLevel == 0)
            {
                //audio energy level drop
                //AbstractEBookEvent.raise(new AudioLevelChangeEvent(e.AudioLevel, -1, 0));
                ActivityExecutor.add(new InternalAudioLevelChangeActivity(e.AudioLevel, -1, 0));
            }
            preAudioLevel = e.AudioLevel;
        }

        /*
         * recognize the audio file from the given path.
         * 
         */
        public void recognizeAudioFile(string dir)
        {
            //recEngine.SetInputToWaveFile(dir);
            FileStream fs = new FileStream(dir,FileMode.Open);
            recEngine.SetInputToWaveStream(fs);
        }

        void recEngine_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            Debug.WriteLine("Rec completed");
        }

        void recEngine_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            Debug.WriteLine("Load grammar completed" + "\t" + EBookUtil.GetUnixTimeMillis());
            loadGrammarCount--;
            loadGrammarCompleted = true;
        }


        /*
         * Singlaton, allocate speech recognizer once
         */
        /*
        public static EBookSRDevice GetInstance()
        {
            if (instance == null)
            {
                instance = new EBookSRDevice();
            }
            return instance;
        }
        */
        public void setAudioStreamType(INPUT_STREAM_TYPE type)
        {
            audioStreamType = type;
        }
        //public static void setQueue(ref ConcurrentQueue<byte[]> q)
        //{
        //    conQueue = q;
        //}

        public void initializeAudioStream()
        {
            if (audioStreamType == INPUT_STREAM_TYPE.DEFAULT_AUDIO_DEVICE)
            {
                UseDefaultAudioDevice();
            }
            else if (audioStreamType == INPUT_STREAM_TYPE.WEB_RTC)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<EBookHub>();
                context.Clients.All.useBrowserAudio();
                UseAudioQueue();
            }
        }

        /*
         * Tell speech recognizer to use the default audio device.
         */
        public void UseDefaultAudioDevice()
        {
            Debug.WriteLine("SR is using default audio device");
            recEngine.SetInputToDefaultAudioDevice();
        }
        public void UseAudioQueue()
        {
            Debug.WriteLine("SR is using queued stream");
            ebookStream = new EbookStream(ref conQueue);
            SpeechAudioFormatInfo info = new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
            recEngine.SetInputToAudioStream(ebookStream, info);
        }
        /*
         * Enable/disable SR
         * Disable SR when you absolutely don't need to recognize a speech.
         * You can keep the SR running when activate/deactivate grammars
         */
        public void enableSR(bool b)
        {

            if (b)
            {
                if (!recOn)
                {
                    while (!loadGrammarCompleted && loadGrammarCount != 0)
                    {
                        Thread.Sleep(3);
                    }
                    Debug.WriteLine("is load grammar complete before enable SR? " +
                        loadGrammarCompleted + "\t" + EBookUtil.GetUnixTimeMillis());
                    ebookStream.enable(true);
                    recEngine.RecognizeAsync(RecognizeMode.Multiple);
                    recOn = true;
                    Debug.WriteLine("Rec on");
                }
            }
            else
            {

                if (recOn)
                {
                    ebookStream.enable(false);
                    recEngine.RecognizeAsyncCancel();//.RecognizeAsyncStop();
                    recOn = false;
                    Debug.WriteLine("Rec off");
                }
            }
            
        }

        /*
         * Set the audio path. 
         * The audio path will be used to save the wave files in the 'record my voice' mode
         */
        public void SetAudioPath(string path)
        {
            if (path != null)
            {
                Directory.CreateDirectory(path);
                Array.ForEach(Directory.GetFiles(@path), File.Delete);
            }
            this.saveAudioPath = path;
            waveFileNameIndexer = 0;
            confirmSaveAudioList.Clear();
            unconfirmSaveAudioList.Clear();
        }

        /*
         * This method will create the grammar from the given list of text and annotation, and activiate it on the fly.
         * This is happening in runtime, while the SR is running; it might downgrade the performance if the list increased.
         */
        public void GenerateAndLoadGrammars(string[] listText, string[] annotations)
        {

            String timeStamp = GetTimestamp(DateTime.Now);
            Debug.WriteLine("Before load grammar time:" + timeStamp);


            for (int i = 0; i < listText.Length; i++)
            {
                if (i == 0 || (i > 0 && (listText[i - 1].Contains("?") || 
                    listText[i - 1].Contains(".") || listText[i - 1].Contains("!"))))
                {

                    for (int j = i; j < listText.Length && j < i + MAX_WORDS_IN_SPEECH; j++)
                    {

                        string ea = "";
                        List<string> anno = new List<string>();
                        for (int k = i; k <= j; k++)
                        {
                            ea += listText[k] + " ";
                            if (!anno.Contains(annotations[k]))
                            {
                                anno.Add(annotations[k]);
                            }
                        }
                        string annotate = "";
                        foreach (string ann in anno)
                        {
                            annotate += ann + ";";
                        }
                        ea = ea.TrimEnd();
                        SemanticResultValue temp = new SemanticResultValue(ea, i);
                        GrammarBuilder gb = new GrammarBuilder(new SemanticResultKey("startIndex", temp));
                        SemanticResultValue ano = new SemanticResultValue(annotate);
                        gb.Append(new SemanticResultKey("annotation", ano));
                        //gb.Append(temp);
                        Grammar storyGrammar = new Grammar(gb);
                        //storyGrammar.Name = i + ":" + j;
                        recEngine.LoadGrammarAsync(storyGrammar);
                        currentStoryGrammars.Add(storyGrammar);
                        //choose.Add(gb);
                        allDefaultStartIndexes.Add(i);
                    }

                }
            }

            String after = GetTimestamp(DateTime.Now);
            Debug.WriteLine("after load grammar time:" + after);
        }


        /*
         * This method is to construct the new grammar from the current endpoint, and load it into SR.
         */
        public void GenerateAndLoadOnGoingGrammars(string[] list, int beginIndex)
        {
            UnloadOnGoingGrammar();

            int maxchoice = MAX_WORDS_IN_SPEECH;
            if (list.Length - beginIndex < MAX_WORDS_IN_SPEECH)
            {
                maxchoice = list.Length-beginIndex;
            }

            for (int i = beginIndex; i < list.Length && i < beginIndex + MAX_WORDS_IN_SPEECH; i++)
            {
                
                
                string ea = "";

                for (int k = beginIndex; k <= i; k++)
                {
                    ea += list[k] + " ";
                }

                ea = ea.TrimEnd();
                SemanticResultValue temp = new SemanticResultValue(ea, beginIndex);
                GrammarBuilder gb = new GrammarBuilder(new SemanticResultKey("startIndex",temp));

                Grammar storyGrammar = new Grammar(gb);

                recEngine.LoadGrammarAsync(storyGrammar);
                onGoingGrammars.Add(storyGrammar);
            }

            enableSR(true);
        }
        public void LoadCommandGrammars(List<string> list)
        {
            foreach (string each in list)
            {
                LoadCommandGrammar(each);
            }
        }
        /*
         * Load the command grammar from a file
         */
        public void LoadCommandGrammar(string path)
        {
            Grammar grammar = new Grammar(@path);
            grammar.Name = "command";
            grammar.Weight = EBookConstant.DEFAULT_WEIGHT;
            grammar.Priority = EBookConstant.DEFAULT_PRIORITY;
            Debug.WriteLine("loading command grammar(weight:" + grammar.Weight + 
                " ,priority:" + grammar.Priority + ")...");
            recEngine.LoadGrammarAsync(grammar);
        }

        /*
         * Load the given story grammar.
         */
        public void LoadStoryGrammar(Grammar g)
        {
            Debug.WriteLine("loading story grammar("+g.Weight+","+g.Priority+")..."+
                g.RuleName + "\t" + EBookUtil.GetUnixTimeMillis());
            loadGrammarCompleted = false;
            recEngine.LoadGrammarAsync(g);
            currentStoryGrammars.Add(g);

        }

        /*
         * Unload all grammars from the SR
         */
        public void UnloadAllGrammars()
        {
            recEngine.UnloadAllGrammars();
            currentStoryGrammars.Clear();
            onGoingGrammars.Clear();
        }

        /*
         * Unload the active story grammar from SR
         */
        public void UnloadStoryGrammar()
        {
            //stopSR();
            if (currentStoryGrammars.Count > 0)
            {
                foreach(Grammar g in currentStoryGrammars){
                    recEngine.UnloadGrammar(g);
                    Debug.WriteLine("unloading story grammar(" + g.Weight + "," + g.Priority +
                        ")..." + g.RuleName + "\t" + EBookUtil.GetUnixTimeMillis());
                }
                currentStoryGrammars.Clear();
            }
           // startSR();
        }

        public void ReloadStoryGrammars(List<Grammar> gs)
        {
            UnloadStoryGrammar();
            foreach (Grammar g in gs)
            {
                LoadStoryGrammar(g);
            }
        }

        /*
         * increase the priority and weight of the grammar that start from the given index
         * and remove the priority and weight for the rest of the active grammars.
         */
        public void ReloadAndChangeGrammarPriority(int index)
        {
            
            
            foreach (Grammar g in currentStoryGrammars)
            {
                if (g.RuleName.CompareTo("index_" + index) == 0)
                {
                    loadGrammarCompleted = false;
                    loadGrammarCount++;
                    Debug.WriteLine("reloading grammar(index)..." + "\t" + EBookUtil.GetUnixTimeMillis() + "\t" + 
                        g.RuleName);
                    recEngine.UnloadGrammar(g);

                    g.Weight = EBookConstant.NEXT_WORD_WEIGHT;
                    g.Priority = EBookConstant.NEXT_WORD_PRIORITY;
                    recEngine.LoadGrammarAsync(g);
                }
                else if (g.Weight != EBookConstant.DEFAULT_WEIGHT || g.Priority != EBookConstant.DEFAULT_PRIORITY)
                {
                    loadGrammarCompleted = false;
                    loadGrammarCount++;
                    Debug.WriteLine("reloading grammar(weight=+"+g.Weight+",p="+g.Priority+")..." +
                        "\t" + EBookUtil.GetUnixTimeMillis() + "\t" + g.RuleName);
                    recEngine.UnloadGrammar(g);
                    g.Weight = EBookConstant.DEFAULT_WEIGHT;
                    g.Priority = EBookConstant.DEFAULT_PRIORITY;
                    recEngine.LoadGrammarAsync(g);
                }
                
            }
        }

        /*
         * Load the given grammar.
         */
        public void LoadOnGoingGrammar(Grammar g)
        {
            loadGrammarCompleted = false;
            loadGrammarCount++;
            Debug.WriteLine("loading onGoing grammar..." + "\t" + EBookUtil.GetUnixTimeMillis());
            recEngine.LoadGrammarAsync(g);
            onGoingGrammars.Add(g);
        }

        public void UnloadOnGoingGrammar()
        {
            if (onGoingGrammars.Count > 0)
            {
                foreach (Grammar g in onGoingGrammars)
                {
                    recEngine.UnloadGrammar(g);
                }
                onGoingGrammars.Clear();
            }
        }

        public void ReloadOnGoingGrammar(Grammar g)
        {
            UnloadOnGoingGrammar();
            LoadOnGoingGrammar(g);
        }

        public void confirmAndSaveAudio()
        {
            foreach (string f in unconfirmSaveAudioList)
            {
                confirmSaveAudioList.Add(f);
            }
            unconfirmSaveAudioList.Clear();
        }
        public void cleanUnconfirmAudio()
        {
            foreach (string f in unconfirmSaveAudioList)
            {
                File.Delete(f);
            }
            unconfirmSaveAudioList.Clear();
        }

        void synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {

            //Clients.All.addNewMessageToPage("", e.CharacterPosition);
        }

        void recEngine_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            string text = "\nAudio State: " + e.AudioState;
            String timeStamp = GetTimestamp(DateTime.Now);

            Trace.WriteLine(text+"\t"+timeStamp);
            SpeechState state = SpeechState.UNKNOWN;
            if (e.AudioState == AudioState.Silence)
            {
                state = SpeechState.SPEECH_END;

                //if (eachSpeechTimes != null)
                //{
                //    turnAroundTimes.Add(eachSpeechTimes);
                //}
            }
            else if (e.AudioState == AudioState.Speech)
            {
                state = SpeechState.SPEECH_START;
                //eachSpeechTimes = new List<double>();
                //speechTurnAroundTime = EBookUtil.GetUnixTimeMillis();
            }
            //AbstractEBookEvent.raise(new SpeechStateChangeEvent(state));
            ActivityExecutor.add(new InternalSpeechStateChangeActivity(state));
        }


        void recEngine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            string text = "\nAudioPosition: " + e.AudioPosition;
            Trace.WriteLine(text);
        }

        void recEngine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            float confidence = e.Result.Confidence;
            string textResult = e.Result.Text;

            string grammarName = e.Result.Grammar.Name;
            string ruleName = e.Result.Grammar.RuleName;

            double audioDuration = -1;
            
            if (e.Result.Audio != null)
            {
                //Looks like we can't get the audio duration for hypothesis result, BUG in Microsoft?
                audioDuration = e.Result.Audio.Duration.TotalMilliseconds;
            }


            KeyValuePair<string, SemanticValue>[] kvp = e.Result.Semantics.ToArray();

            //AbstractEBookEvent.raise(new RecognitionResultEvent(confidence,textResult,true,
            //    kvp,grammarName,ruleName, audioDuration,null));
            ActivityExecutor.add(new InternalSpeechRecognitionResultActivity(confidence, textResult, true,
                kvp, grammarName, ruleName, audioDuration, null));
            String timeStamp = GetTimestamp(DateTime.Now);

            string text = "\n" + e.Result.Confidence + "\t" + e.Result.Text + "(Hypothesis)\t" + 
                e.Result.Semantics.Value + "\tgrammarName=" + e.Result.Grammar.Name + "\truleName=" + 
                e.Result.Grammar.RuleName +"\t"+ timeStamp;
            Trace.WriteLine(text);
            

            //double timeNow = EBookUtil.GetUnixTimeMillis();
            //double mun = timeNow - speechTurnAroundTime;
            //speechTurnAroundTime = timeNow;
            //eachSpeechTimes.Add(mun);
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }

        //String lastCompleteResult = "";
        void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            float confidence = e.Result.Confidence;
            string textResult = e.Result.Text;
            //string semantics = e.Result.Semantics.Value + "";
            string grammarName = e.Result.Grammar.Name;
            string ruleName = e.Result.Grammar.RuleName;
            double audioDuration = -1;
            if (e.Result.Audio != null)
            {
                audioDuration = e.Result.Audio.Duration.TotalMilliseconds;
            }
            //string phonetic = "";
            //foreach (RecognizedWordUnit unit in e.Result.Words)
            //{
            //    phonetic += unit.Pronunciation + " ";
            //}
            //Debug.WriteLine(textResult + "[" + phonetic.TrimEnd() + "]");

            Debug.WriteLine("audio duration=" + audioDuration);
            KeyValuePair<string, SemanticValue>[] kvp = e.Result.Semantics.ToArray();


            string fileP = null;
            string relPath = null;
            //only write audio file when given path is not null
            if (saveAudioPath != null)
            {
                string indexerStr = waveFileNameIndexer + "";
                while (indexerStr.Length < 4)
                {
                    indexerStr = "0" + indexerStr;
                }
                
                fileP = saveAudioPath + "\\" + indexerStr + ".wav";

                relPath = EBookUtil.convertAudioToRelativePath(@fileP);
            }



            //AbstractEBookEvent.raise(new RecognitionResultEvent(confidence, textResult, false, 
            //    kvp,grammarName,ruleName,audioDuration,@fileP));
            ActivityExecutor.add(new InternalSpeechRecognitionResultActivity(confidence, textResult, false,
                kvp, grammarName, ruleName, audioDuration, relPath));

            //only write audio file when given path is not null
            if (fileP != null)
            {
                //write audio to file
                FileStream stream = new FileStream(fileP, FileMode.Create);

                e.Result.Audio.WriteToWaveStream(stream);
                stream.Flush();
                stream.Close();
                unconfirmSaveAudioList.Add(fileP);
                Trace.WriteLine("write to file " + fileP);
                waveFileNameIndexer++;
            }
            String timeStamp = GetTimestamp(DateTime.Now);

            string text = "\n" + e.Result.Confidence + "\t" + e.Result.Text + "(complete)\t\t" + 
                e.Result.Semantics.Value + "\t" + e.Result.Grammar.Name + "\t" + timeStamp;
            Trace.WriteLine(text);
                
        }

        /*
         * The function get called when a speech ended and the Speech Recognzier conclude
         * no match for the given grammars.
         */
        void recEngine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            float confidence = e.Result.Confidence;
            string textResult = e.Result.Text;
            //raise a RejectRecognitionEvent
            //AbstractEBookEvent.raise(new RejectRecognitionEvent());
            ActivityExecutor.add(new InternalRejectRecognitionActivity());
            String timeStamp = GetTimestamp(DateTime.Now);

            string text = "\n" + e.Result.Confidence + "\t" + e.Result.Text + "(rejected)\t\t" + 
                e.Result.Semantics.Value + "\t" + timeStamp;
            Trace.WriteLine(text);
        }


        public void updateStream(float[] n, int index)
        {
            //if (index - previousIndex != 1)
            //{
            //    Debug.WriteLine("***stream out of order " + index + " " + previousIndex);
            //}
            //previousIndex = index;
            if (active)
            {
                //using different thread to do the conversion work to ensure the data will not be corrupted
                ThreadPool.QueueUserWorkItem(new WaitCallback(pushDataSync), new Object[]{n,index});
                //Thread thread = new Thread(unused => pushDataSync(n, index));
                //thread.Start();
            }
        }
        private static int lookingIndex = 1;
        public void updateStreamIndex(int index)
        {
            lookingIndex = index;
        }
        
        private static Object locker = new Object();

        private void pushDataSync(Object objs)
        {
            if (!startEnqueueAudio) return;
            Object[] eachObj = (Object[])objs;

            //need to convert float (32-bit) to 2 bytes (2 x 8-bit)
            float[] n = (float[])eachObj[0];
            int currentIndex = (int)eachObj[1];

            byte[] result = new byte[n.Length * 2];
            for (int i = 0; i < n.Length; i++)
            {
                double tmp = n[i] * 32768;
                if (tmp > 32767) tmp = 32767;
                else if (tmp < -32768) tmp = -32768;
                short toInt = (short)tmp;
                result[i * 2 + 1] = (byte)(toInt >> 8);
                result[i * 2] = (byte)(toInt & 0xff);
            }

            //use lock and while loop to sync the data in order
            bool added = false;
            int maxCycle = 100;
            int cycle = 0;
            //use max cycle to prevent any deadlock.
            while (!added && cycle<maxCycle)
            {
                lock (locker)
                {
                    if (currentIndex == lookingIndex)
                    {
                        conQueue.Enqueue(result);
                        lookingIndex++;
                        added = true;
                    }
                }
                if (!added)
                {
                    Thread.Sleep(5);
                    cycle++;
                }
            }
            //Debug.WriteLine("size of queue "+conQueue.Count);
            
        }

        public void restart()
        {
            enableSR(false);
            UnloadAllGrammars();
        }

        public override string getDeviceName()
        {
            return "ASR device";
        }
    }
}