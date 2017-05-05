using eDocumentReader.Hubs.structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// This class logs the speech recognition result into a log file in
    /// recording mode. It will not write to a log file in real time mode.
    /// The log file will be used in replay mode to simulate recognition 
    /// results.
    /// </summary>
    public class StoryLoggingDevice : AbstractDevice
    {
        private System.Object lockThis = new System.Object();

        public static readonly string logFileName = "result.txt";
        public static readonly string time_tag = "time";
        public static readonly string confidence_tag = "confidence";
        public static readonly string textResult_tag = "textResult";
        public static readonly string isHyp_tag = "isHypothesis";
        public static readonly string grammarName_tag = "grammarName";
        public static readonly string ruleName_tag = "ruleName";
        public static readonly string duration_tag = "duration";
        public static readonly string wavePath_tag = "wavePath";
        public static readonly string key_tag = "key";
        public static readonly string value_tag = "value";

        private string loggingPath;
        private List<string> oneSpeech = new List<string>();
        private int lastCompleteSpeechIndex = -1;

        //private Mode mode = Mode.UNKNOWN;

        public StoryLoggingDevice()
        {
        }
        /// <summary>
        /// Set where the logs will be saved in.
        /// </summary>
        /// <param name="path">The path to the folder that will contain the log file</param>
        public void setLogPath(string path)
        {
            loggingPath = path;
        }

        /// <summary>
        /// The previous hypothesis result is rejected by the SR. 
        /// Remove the hypothesis result from the log.
        /// </summary>
        public void logRejectLastRecognition()
        {
            if (lastCompleteSpeechIndex > 0)
            {
                while (oneSpeech.Count > lastCompleteSpeechIndex)
                {
                    oneSpeech.RemoveAt(lastCompleteSpeechIndex);
                }
            }
        }
        /// <summary>
        /// A new speech recognition received from SR. Save the result to a list.
        /// </summary>
        /// <param name="confidence">confident score</param>
        /// <param name="textResult">the text string of the speech</param>
        /// <param name="isHypothesis">is a hypothesis result?</param>
        /// <param name="semanticResult">the semantic that define in grammar </param>
        /// <param name="grammarName">the grammar that used for this speech</param>
        /// <param name="ruleName">The rule name that define in the grammar</param>
        /// <param name="audioDuration">The duration of the speech</param>
        /// <param name="wavPath">the path to the audio file</param>
        public void logRecognitionResult(float confidence, string textResult,
            bool isHypothesis, KeyValuePair<string, SemanticValue>[] semanticResult, string grammarName,
            string ruleName, double audioDuration, string wavPath)
        {
            double unixTime = EBookUtil.GetUnixTimeMillis();
            string semantics = "";
            foreach (KeyValuePair<string, SemanticValue> each in semanticResult)
            {
                semantics += "key::" + each.Key.ToString() + "|value::" + each.Value.Value.ToString() + "|";
            }

            oneSpeech.Add("time::" + unixTime + "|confidence::" + confidence + "|textResult::" + textResult +
                "|isHypothesis::" + isHypothesis + "|" + semantics + "grammarName::" + grammarName + "|ruleName::" +
                ruleName + "|duration::" + audioDuration + "|wavePath::" + wavPath + "\n");
            if (!isHypothesis)
            {
                lastCompleteSpeechIndex = oneSpeech.Count;
            }
        }

        /// <summary>
        /// Log the "start of speech"
        /// </summary>
        /// <param name="state">the speech state</param>
        public void logSpeechState(SpeechState state)
        {
            if (state == SpeechState.SPEECH_START)
            {
                oneSpeech.Add("start::" + EBookUtil.GetUnixTimeMillis() + "\n");
            }
        }
        /// <summary>
        /// Write an "end of page" indicator to the log
        /// </summary>
        public void finishPage()
        {
            writeToFile("finishPage\n");
        }

        /// <summary>
        /// The user click the reject button in recording mode to remove the last unconfirmed speech.
        /// Rejecting the last recognition result will remove the last recognition result
        /// from the log.
        /// </summary>
        public void rejectSpeech()
        {
            oneSpeech.Clear();
        }

        /// <summary>
        /// The user click the accept button in recording mode cause the system to save the result in file.
        /// </summary>
        public void acceptSpeech()
        {
            if (loggingPath != null)
            {
                string oneSpeechStr = "";
                foreach (string ea in oneSpeech)
                {
                    oneSpeechStr += ea;
                }

                writeToFile(oneSpeechStr);
                oneSpeech.Clear();
            }
        }

        /// <summary>
        /// Write the given content to a log file.
        /// If the log file already exist, append the content, 
        /// otherwise create a new log file, and write to the 
        /// beginning of the file
        /// </summary>
        /// <param name="content"></param>
        private void writeToFile(string content)
        {
            if (loggingPath != null && active)
            {
                string logFilePath = loggingPath + "//" + logFileName;
                lock (lockThis)
                {
                    if (!File.Exists(logFilePath))
                    {
                        File.WriteAllText(logFilePath, content);
                    }
                    else
                    {
                        File.AppendAllText(logFilePath, content);
                    }
                }
            }
        }
        /// <summary>
        /// Reset
        /// </summary>
        public void reset()
        {
            oneSpeech.Clear();
            lastCompleteSpeechIndex = -1;

        }
        public override string getDeviceName()
        {
            return "Story logger";
        }
    }
}