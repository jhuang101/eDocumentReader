using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Speech.Synthesis;
using System.Speech.Synthesis.TtsEngine;
using System.Speech.Recognition;
using System.Diagnostics;
using System.IO;

namespace eDocumentReader.Hubs
{ 
    /// <summary>
    /// A speech synthesizer. The system will use this class to generate
    /// the synthesized speech and save it to a log (the log will in the same
    /// format as you do in recording mode). The synthesized speech can be
    /// play in replay mode.
    /// </summary>
    public class EBookSpeechSynthesizer
    {
        private static EBookSpeechSynthesizer instance;
        private static SpeechSynthesizer synthesizer;
        private static readonly int MAXLEN = 4; //max length of the audio file's numeric name
        private static readonly int synthesisSpeakRate = -2;

        private static bool saveData; //tell whether to save the current speaking utterance in files
        private string logResultFile;
        private double totalSentenceDuration;
        private int startIndex;//the start index of current sentence
        private double startTime;

        private static string prompt;
        private EBookSpeechSynthesizer(){
            if (synthesizer == null)
            {
                synthesizer = new SpeechSynthesizer();
                synthesizer.PhonemeReached += synthesizer_PhonemeReached;
                synthesizer.StateChanged += synthesizer_StateChanged;
                synthesizer.SpeakCompleted += synthesizer_SpeakCompleted;
                synthesizer.SpeakProgress += synthesizer_SpeakProgress;
                //synthesizer.SetOutputToDefaultAudioDevice();
                
            }
        }


        void synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            
            //phoneme = "";
            if(saveData){
                //
                double rate = 1+synthesisSpeakRate * 0.1;
                if (rate > 1)
                {
                    rate = 1;
                }
                totalSentenceDuration = e.AudioPosition.TotalMilliseconds*(rate);
              
                prompt += e.Text+" ";
                string data = "time::" + (startTime + totalSentenceDuration) + "|confidence::0.99|textResult::" + prompt.Trim() +
                    "|isHypothesis::True|grammarName::|ruleName::index_"+startIndex+"|duration::-1|wavePath::\n";
                writeToFile(data);
                
            }
        }

        void synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
        }

        void synthesizer_StateChanged(object sender, System.Speech.Synthesis.StateChangedEventArgs e)
        {
            Debug.WriteLine(e.State.ToString());
        }
        public void speak(string utts)
        {
            //force to terminate all queued prompts
            synthesizer.SpeakAsyncCancelAll();
            string[] separator = {"!","?","."};
            string[] sentences = utts.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            foreach (string ea in sentences)
            {
                string path = "C:\\temp\\audio"+index+".wav";
                synthesizer.SetOutputToWaveFile(@path);
                PromptBuilder pb = new PromptBuilder();
                synthesizer.Rate = -1;
                synthesizer.Speak(ea);
                index++;
            }
            
            //Debug.WriteLine("Phonetics: " + phoneme);
        }

        public void generateSynthesisData(Story story)
        {
            saveData = true;
            foreach (InstalledVoice iv in synthesizer.GetInstalledVoices())
            {
                string voiceName = iv.VoiceInfo.Name;
                Debug.WriteLine("installed voice :" + voiceName);                
                synthesizer.SelectVoice(voiceName);
                string path = story.getFullPath() + "\\" + EBookInteractiveSystem.voice_dir + "\\" + voiceName.Replace(" ", "_");
                Directory.CreateDirectory(path);
                int index = 0;
                Page p = story.GetFirstPage();

                logResultFile = path + "\\" + StoryLoggingDevice.logFileName;

                if (File.Exists(logResultFile))
                {
                    Debug.WriteLine("skip gnerating synthesis data, data found in " + path);
                    return;
                }
                startTime = (long)LogPlayer.GetUnixTimestamp();
                
                while(p != null){
                    //reset startIndex when start a new page
                    startIndex = 0;

                    List<string[]> textArray = p.GetListOfTextArray();
                    string whole = "";
                    foreach(string[] text in textArray){
                        if (text != null)
                        {
                            foreach (string ea in text)
                            {
                                whole += ea + " ";
                            }
                        }
                    }
                    string[] separator = { "!", "?", "." };
                    string[] sp = whole.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    List<string> sentences = new List<string>();
                    string tmpStr = "";
                    foreach (string ea in sp)
                    {
                        tmpStr += ea + " ";
                        foreach (string punc in separator)
                        {
                            if (ea.Contains(punc))
                            {
                                sentences.Add(tmpStr.TrimEnd());
                                tmpStr = "";
                                break;
                            }
                        }
                    }
                    foreach (string ea in sentences)
                    {
                        Debug.WriteLine("generating tts for:" + ea);
                        string audioPath = path + "\\" + getPrefix(index)+".wav";
                        synthesizer.SetOutputToWaveFile(@audioPath);

                        PromptBuilder pb = new PromptBuilder();
                        synthesizer.Rate = synthesisSpeakRate;
                        prompt = "";
                        writeToFile("start::" + startTime+"\n");
                        startTime += 10;
                        synthesizer.Speak(ea);
                        index++;
                        startTime += (totalSentenceDuration);

                        string data = "time::" + startTime + "|confidence::0.99|textResult::" + prompt.Trim() +
                    "|isHypothesis::False|key::startIndex|value::" + startIndex + "|grammarName::|ruleName::index_" + 
                    startIndex + "|duration::" + totalSentenceDuration + "|wavePath::" + EBookUtil.convertAudioToRelativePath(@audioPath) + "\n";
                        writeToFile(data);

                        string[] tmp = ea.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                        startIndex += tmp.Length;

                        startTime += 1000;//one second pause

                    }
                    writeToFile("finishPage\n");
                    p = story.GetNextPage();

                }

            }
            saveData = false;
        }

        private void writeToFile(string data)
        {
            if (!File.Exists(logResultFile))
            {
                File.WriteAllText(logResultFile, data);
            }
            else
            {
                File.AppendAllText(logResultFile, data);
            }
        }
        private string getPrefix(int n)
        {
            string x = n + "";
            while (x.Length < MAXLEN)
            {
                x = "0" + x;
            }
            return x;
        }

        void synthesizer_PhonemeReached(object sender, PhonemeReachedEventArgs e)
        {
            Debug.WriteLine(e.Prompt.ToString()+"\t"+e.Prompt.IsCompleted+"\t"+e.Phoneme);
  
        }
        public static EBookSpeechSynthesizer getInstance(){
            if(instance == null){
                instance = new EBookSpeechSynthesizer();
            }
            return instance;
        }

    }
    

}