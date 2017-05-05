using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using System.Web;

namespace eDocumentReader.Hubs.devices.speech
{
    public class EBookSpeechRecognitionDevice : AbstractDevice,SpeechRecognitionListener
    {

        private int MAX_WORDS_IN_SPEECH;

        private static List<Grammar> currentStoryGrammars = new List<Grammar>();
        private static List<Grammar> onGoingGrammars = new List<Grammar>();
        private List<int> allDefaultStartIndexes = new List<int>();

        private string saveAudioPath;
        private bool loadGrammarCompleted = false; //used for debug purpose
        private int loadGrammarCount = 0; //used for debug purpose
        private int waveFileNameIndexer;

        private List<string> confirmSaveAudioList = new List<string>();
        private List<string> unconfirmSaveAudioList = new List<string>();

        private int preAudioLevel;

        private Boolean recOn = false;

        private RecognizerInterface sr;

        public EBookSpeechRecognitionDevice()
        {
            MAX_WORDS_IN_SPEECH = EBookInteractiveSystem.MAX_WORDS_IN_SPEECH;
            sr = new EBookSpeechRecognizer(this);
        }

        public void handleAudioLevelUpdate(int audioLevel)
        {
            String timeStamp = EBookUtil.GetTimestamp();
            Trace.WriteLine("audio level:" + audioLevel + "\t" + timeStamp);
            if (audioLevel > EBookInteractiveSystem.initialNoiseSensitivity)
            {
                int start = 0;
                double unixTime = EBookUtil.GetUnixTimeMillis();
                if (preAudioLevel == 0)
                {
                    //audio energy level jump
                    start = 1;
                }
                ActivityExecutor.add(new InternalAudioLevelChangeActivity(audioLevel, start, unixTime));
            }
            else if (audioLevel == 0 && preAudioLevel > 0 && audioLevel == 0)
            {
                //audio energy level drop
                ActivityExecutor.add(new InternalAudioLevelChangeActivity(audioLevel, -1, 0));
            }
            preAudioLevel = audioLevel;
        }

        public void handleAudioStateChanged(AudioState audioState)
        {
            string text = "\nAudio State: " + audioState;
            String timeStamp = EBookUtil.GetTimestamp();

            Trace.WriteLine(text + "\t" + timeStamp);
            SpeechState state = SpeechState.UNKNOWN;
            if (audioState == AudioState.Silence)
            {
                state = SpeechState.SPEECH_END;


            }
            else if (audioState == AudioState.Speech)
            {
                state = SpeechState.SPEECH_START;
            }
            ActivityExecutor.add(new InternalSpeechStateChangeActivity(state));
        }

        public void handleSpeechHypothesizedResult(float confidence, string textResult,
            string grammarName, string ruleName, KeyValuePair<string, SemanticValue>[] kvp,
            double audioDuration)
        {

            ActivityExecutor.add(new InternalSpeechRecognitionResultActivity(confidence, textResult, true,
                kvp, grammarName, ruleName, audioDuration, null));
            String timeStamp = EBookUtil.GetTimestamp();

            string text = "\n" + confidence + "\t" + textResult + "(Hypothesis)\t" +
                kvp.ToArray() + "\tgrammarName=" + grammarName + "\truleName=" +
                ruleName + "\t" + timeStamp;
            Trace.WriteLine(text);
        }

        public void handleSpeechRecognizedResult(float confidence, string textResult,
            string grammarName, string ruleName, KeyValuePair<string, SemanticValue>[] kvp,
            double audioDuration,RecognizedAudio audio)
        {
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

            ActivityExecutor.add(new InternalSpeechRecognitionResultActivity(confidence, textResult, false,
                kvp, grammarName, ruleName, audioDuration, relPath));

            //only write audio file when given path is not null
            if (fileP != null)
            {
                //write audio to file
                FileStream stream = new FileStream(fileP, FileMode.Create);

                audio.WriteToWaveStream(stream);
                stream.Flush();
                stream.Close();
                unconfirmSaveAudioList.Add(fileP);
                Trace.WriteLine("write to file " + fileP);
                waveFileNameIndexer++;
            }
            String timeStamp = EBookUtil.GetTimestamp();

            string text = "\n" + confidence + "\t" + textResult + "(complete)\t\t" +
                kvp.ToArray() + "\t" + grammarName + "\t" + timeStamp;
            Trace.WriteLine(text);
        }

        public void handleRejectRecognition(float confidence, string textResult, KeyValuePair<string, SemanticValue>[] kvp)
        {
            ActivityExecutor.add(new InternalRejectRecognitionActivity());
            String timeStamp = EBookUtil.GetTimestamp();

            string text = "\n" + confidence + "\t" + textResult + "(rejected)\t\t" +
                kvp.ToArray() + "\t" + timeStamp;
            Trace.WriteLine(text);
        }

        public void handleRecognizeCompleted()
        {
            Debug.WriteLine("Rec completed");
        }
        public void handleFinishLoadingGrammar(string grammarName)
        {
            Debug.WriteLine("Load grammar completed" + "\t" + EBookUtil.GetUnixTimeMillis());
            loadGrammarCount--;
            loadGrammarCompleted = true;
        }
        /// <summary>
        /// Enable/disable SR
        /// Disable SR when you absolutely don't need to recognize a speech.
        /// You can keep the SR running when load a grammar. But
        /// it may need to disable the SR before unload a grammar.
        /// </summary>
        /// <param name="b"></param>
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
                    sr.start();
                    recOn = true;
                    Debug.WriteLine("Rec on");
                }
            }
            else
            {

                if (recOn)
                {
                    sr.stop();
                    recOn = false;
                    Debug.WriteLine("Rec off");
                }
            }

        }

        /// <summary>
        /// Set the audio path. 
        /// The audio path will be used to save the wave files in the 'record my voice' mode.
        /// </summary>
        /// <param name="path"></param>
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

        public void initializeAudioStream(INPUT_STREAM_TYPE type){
            sr.setAudioStreamType(type);
            sr.initializeAudioStream();
        }

        /// <summary>
        /// This method will create the grammar from the given list of text and annotation, and activiate it on the fly.
        /// This is happening in runtime, while the SR is running; it might downgrade the performance if the list increased.
        /// </summary>
        /// <param name="listText"></param>
        /// <param name="annotations"></param>
        public void GenerateAndLoadGrammars(string[] listText, string[] annotations)
        {

            String timeStamp = EBookUtil.GetTimestamp();
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
                        Grammar storyGrammar = new Grammar(gb);

                        sr.LoadGrammarAsync(storyGrammar);
                        currentStoryGrammars.Add(storyGrammar);
                        allDefaultStartIndexes.Add(i);
                    }

                }
            }

            String after = EBookUtil.GetTimestamp();
            Debug.WriteLine("after load grammar time:" + after);
        }


        /// <summary>
        /// This method is to construct the new grammar from the current endpoint, and load it into SR.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="beginIndex"></param>
        public void GenerateAndLoadOnGoingGrammars(string[] list, int beginIndex)
        {
            UnloadOnGoingGrammar();

            int maxchoice = MAX_WORDS_IN_SPEECH;
            if (list.Length - beginIndex < MAX_WORDS_IN_SPEECH)
            {
                maxchoice = list.Length - beginIndex;
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
                GrammarBuilder gb = new GrammarBuilder(new SemanticResultKey("startIndex", temp));

                Grammar storyGrammar = new Grammar(gb);

                sr.LoadGrammarAsync(storyGrammar);
                onGoingGrammars.Add(storyGrammar);
            }

            enableSR(true);
        }



        /// <summary>
        /// Increase the priority and weight of the grammar that start from the given index
        /// and remove the priority and weight for the rest of the active grammars.
        /// </summary>
        /// <param name="index"></param>
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
                    sr.UnloadGrammar(g);

                    g.Weight = EBookConstant.NEXT_WORD_WEIGHT;
                    g.Priority = EBookConstant.NEXT_WORD_PRIORITY;
                    sr.LoadGrammarAsync(g);
                }
                else if (g.Weight != EBookConstant.DEFAULT_WEIGHT || g.Priority != EBookConstant.DEFAULT_PRIORITY)
                {
                    loadGrammarCompleted = false;
                    loadGrammarCount++;
                    Debug.WriteLine("reloading grammar(weight=+" + g.Weight + ",p=" + g.Priority + ")..." +
                        "\t" + EBookUtil.GetUnixTimeMillis() + "\t" + g.RuleName);
                    sr.UnloadGrammar(g);
                    g.Weight = EBookConstant.DEFAULT_WEIGHT;
                    g.Priority = EBookConstant.DEFAULT_PRIORITY;
                    sr.LoadGrammarAsync(g);
                }

            }
        }

        /// <summary>
        /// Load a list of command grammars.
        /// </summary>
        /// <param name="list"></param>
        public void LoadCommandGrammars(List<string> list)
        {
            foreach (string each in list)
            {
                LoadCommandGrammar(each);
            }
        }

        /// <summary>
        /// Load the command grammar from a file
        /// </summary>
        /// <param name="path"></param>
        public void LoadCommandGrammar(string path)
        {
            Grammar grammar = new Grammar(@path);
            grammar.Name = "command";
            grammar.Weight = EBookConstant.DEFAULT_WEIGHT;
            grammar.Priority = EBookConstant.DEFAULT_PRIORITY;
            Debug.WriteLine("loading command grammar(weight:" + grammar.Weight +
                " ,priority:" + grammar.Priority + ")...");
            sr.LoadGrammarAsync(grammar);
        }

        /// <summary>
        /// Load the on going grammar.
        /// On going grammar are generated when the reader stop at the word that is not
        /// the last word of a sentence. for instance, in a sentence "I ate pizza for dinner.",
        /// if the reader say "I ate pizza" and stop, the system will expect the reader to "for dinner"
        /// next, so the system will generate the grammar for "for dinner" and loaded to SR.
        /// </summary>
        /// <param name="g">The grammar to be loaded</param>
        public void LoadOnGoingGrammar(Grammar g)
        {
            loadGrammarCompleted = false;
            loadGrammarCount++;
            Debug.WriteLine("loading onGoing grammar..." + "\t" + EBookUtil.GetUnixTimeMillis());
            sr.LoadGrammarAsync(g);
            onGoingGrammars.Add(g);
        }
        /// <summary>
        /// Unload the on going grammar.
        /// It needs to stop the SR before unload grammar, otherwise
        /// the recEngine might failed.
        /// </summary>
        public void UnloadOnGoingGrammar()
        {
            if (onGoingGrammars.Count > 0)
            {
                foreach (Grammar g in onGoingGrammars)
                {
                    sr.UnloadGrammar(g);
                }
                onGoingGrammars.Clear();
            }
        }

        /// <summary>
        /// The ReloadOnGoingGrammar will unload all the on going grammar
        /// from the SR and load the given grammar to the SR.
        /// </summary>
        /// <param name="g"> the grammar to be load into the SR</param>
        public void ReloadOnGoingGrammar(Grammar g)
        {
            UnloadOnGoingGrammar();
            LoadOnGoingGrammar(g);
        }

        /// <summary>
        /// Load the given story grammar.
        /// </summary>
        /// <param name="g"></param>
        public void LoadStoryGrammar(Grammar g)
        {
            Debug.WriteLine("loading story grammar(" + g.Weight + "," + g.Priority + ")..." +
                g.RuleName + "\t" + EBookUtil.GetUnixTimeMillis());
            loadGrammarCompleted = false;
            sr.LoadGrammarAsync(g);
            currentStoryGrammars.Add(g);

        }

        /// <summary>
        /// Unload the active story grammar from SR
        /// </summary>
        public void UnloadStoryGrammar()
        {
            //stopSR();
            if (currentStoryGrammars.Count > 0)
            {
                foreach (Grammar g in currentStoryGrammars)
                {
                    sr.UnloadGrammar(g);
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

        /// <summary>
        /// Unload all grammars from the SR
        /// </summary>
        public void UnloadAllGrammars()
        {
            sr.UnloadAllGrammars();
            currentStoryGrammars.Clear();
            onGoingGrammars.Clear();
        }

        public void restart()
        {
            enableSR(false);
            UnloadAllGrammars();
        }



        /// <summary>
        /// Add the unconfirmed audio file to confirm.
        /// The method is used in record mode where it need to
        /// save the audio files when the reader accept the speech.
        /// </summary>
        public void confirmAndSaveAudio()
        {
            foreach (string f in unconfirmSaveAudioList)
            {
                confirmSaveAudioList.Add(f);
            }
            unconfirmSaveAudioList.Clear();
        }
        /// <summary>
        /// Remove the unconfirm audio file list.
        /// The method is used in recording mode where the user
        /// click the "reject" button to clear the previous
        /// unconfirmed speech.
        /// </summary>
        public void cleanUnconfirmAudio()
        {
            foreach (string f in unconfirmSaveAudioList)
            {
                File.Delete(f);
            }
            unconfirmSaveAudioList.Clear();
        }

        public void updateStream(float[] data, int index)
        {
            if (active)
            {
                sr.updateStream(data, index);
            }
        }
        public void updateStreamIndex(int index)
        {
            sr.updateStreamIndex(index);
        }

        public override string getDeviceName()
        {
            return "Speech recognition device";
        }
    }
}