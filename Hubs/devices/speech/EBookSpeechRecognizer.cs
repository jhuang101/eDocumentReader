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
using eDocumentReader.Hubs.devices.speech;


namespace eDocumentReader.Hubs
{
    public enum INPUT_STREAM_TYPE
    {
        DEFAULT_AUDIO_DEVICE,
        WEB_RTC
    }
    
    /// <summary>
    /// This class uses the speech recognition engine in Microsoft to
    /// do the speech recognition work.
    /// </summary>
    public class EBookSpeechRecognizer : RecognizerInterface
    {
        public SpeechRecognitionListener listener;

        public static readonly int UNAMBIGUOUS_SILENCE_TIMEOUT = 50;
        public static readonly int AMBIGUOUS_SILENCE_TIMEOUT = 500;

        private static Object locker = new Object();

        private static SpeechRecognitionEngine recEngine;

        private static ConcurrentQueue<byte[]> conQueue = new ConcurrentQueue<byte[]>();

        private static INPUT_STREAM_TYPE audioStreamType;

        private EbookStream ebookStream;

        private bool startEnqueueAudio = true; //a flag to indicate whether to put received audio in the queue.

        public EBookSpeechRecognizer(SpeechRecognitionListener listener)
        {
            this.listener = listener;

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
            listener.handleAudioLevelUpdate(e.AudioLevel);
        }

        /*
         * recognize the audio file from the given path.
         * TODO: need to implement this
         */
        public void recognizeAudioFile(string dir)
        {
            FileStream fs = new FileStream(dir,FileMode.Open);
            recEngine.SetInputToWaveStream(fs);
        }

        void recEngine_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            listener.handleRecognizeCompleted();
            
        }

        void recEngine_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            listener.handleFinishLoadingGrammar(e.Grammar.Name);
        }

        /// <summary>
        /// Set the audio input type.
        /// DEFAULT_AUDIO_DEVICE: use the Mic that connected to this computer
        /// WEB_RTC: capture audio from the browser and send thru SingalR 
        /// </summary>
        /// <param name="type"></param>
        public void setAudioStreamType(INPUT_STREAM_TYPE type)
        {
            audioStreamType = type;
        }

        /// <summary>
        /// Initialize the audio stream.
        /// </summary>
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

        /// <summary>
        /// use the default audio device for speech recognizer.
        /// </summary>
        public void UseDefaultAudioDevice()
        {
            Debug.WriteLine("SR is using default audio device");
            recEngine.SetInputToDefaultAudioDevice();
        }

        /// <summary>
        /// Construct a customized audio stream and attach to recognition engine.
        /// </summary>
        public void UseAudioQueue()
        {
            Debug.WriteLine("SR is using queued stream");
            ebookStream = new EbookStream(ref conQueue);
            SpeechAudioFormatInfo info = new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
            recEngine.SetInputToAudioStream(ebookStream, info);
        }

        void synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            //Clients.All.addNewMessageToPage("", e.CharacterPosition);
        }

        void recEngine_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            listener.handleAudioStateChanged(e.AudioState);
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
            KeyValuePair<string, SemanticValue>[] kvp = e.Result.Semantics.ToArray();

            double audioDuration = -1;

            if (e.Result.Audio != null)
            {
                //Looks like we can't get the audio duration for hypothesis result, BUG in Microsoft?
                audioDuration = e.Result.Audio.Duration.TotalMilliseconds;
            }

            listener.handleSpeechHypothesizedResult(confidence, textResult,grammarName,ruleName,
                kvp, audioDuration);

        }

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
            Debug.WriteLine("audio duration=" + audioDuration);
            KeyValuePair<string, SemanticValue>[] kvp = e.Result.Semantics.ToArray();

            listener.handleSpeechRecognizedResult(confidence, textResult, grammarName, ruleName,
                kvp, audioDuration,e.Result.Audio);
                
        }

        /*
         * The function get called when a speech ended and the Speech Recognzier conclude
         * no match for the given grammars.
         */
        void recEngine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            float confidence = e.Result.Confidence;
            string textResult = e.Result.Text;
            listener.handleRejectRecognition(confidence, textResult, e.Result.Semantics.ToArray());
        }


        public void updateStream(float[] n, int index)
        {
            //using different thread to do the conversion work to ensure the data will not be corrupted
            ThreadPool.QueueUserWorkItem(new WaitCallback(pushDataSync), new Object[]{n,index});
        }
        private static int lastStreamIndex = -1;
        /// <summary>
        /// A way to synchronized the receiving stream
        /// </summary>
        /// <param name="index">the next expected stream index</param>
        public void updateStreamIndex(int index)
        {
            lastStreamIndex = index;
        }

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
            //ignore it if the receiving buffer is older than the last added buffer
            if (currentIndex >= lastStreamIndex)
            {
                conQueue.Enqueue(result);
                lastStreamIndex = currentIndex;
            }
            /*
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
             */
            //Debug.WriteLine("size of queue "+conQueue.Count);
            
        }

        public void start()
        {
            ebookStream.enable(true);
            recEngine.RecognizeAsync(RecognizeMode.Multiple);

        }

        public void stop()
        {
            ebookStream.enable(false);
            recEngine.RecognizeAsyncCancel();//.RecognizeAsyncStop();

        }
        public void LoadGrammar(Grammar g)
        {
            recEngine.LoadGrammar(g);
        }
        public void LoadGrammarAsync(Grammar g)
        {
            recEngine.LoadGrammarAsync(g);
        }

        public void UnloadGrammar(Grammar g)
        {
            recEngine.UnloadGrammar(g);
        }

        public void UnloadAllGrammars()
        {
            recEngine.UnloadAllGrammars();
        }

    }
}