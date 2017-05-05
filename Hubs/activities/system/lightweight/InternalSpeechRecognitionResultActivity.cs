using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.activities.system.lightweight
{
    public class InternalSpeechRecognitionResultActivity : Activity
    {
        float confidence;
        string textResult;
        bool isHypothesis;
        KeyValuePair<string, SemanticValue>[] kvp;
        string grammarName;
        string ruleName;
        double audioDuration;
        string wavPath;

        public InternalSpeechRecognitionResultActivity(float confidence, string textResult, 
            bool isHypothesis, KeyValuePair<string,SemanticValue>[] kvp, string grammarName, 
            string ruleName, double audioDuration, string wavPath)
        {
            this.confidence = confidence;
            this.textResult = textResult;
            this.isHypothesis = isHypothesis;
            this.kvp = kvp;
            this.grammarName = grammarName;
            this.ruleName = ruleName;
            this.audioDuration = audioDuration;
            this.wavPath = wavPath;
        }

        public float getConfidenceScore() { return confidence; }
        public string getTextResult() { return textResult; }
        public bool isHypothesisResult() { return isHypothesis; }
        public KeyValuePair<string, SemanticValue>[] getSemanticResult() { return kvp; }
        public string getGrammarName() { return grammarName; }
        public string getRuleName() { return ruleName; }
        public double getAudioDuration() { return audioDuration; }
        public string getWavPath() { return wavPath; }


        public override ActivityType getActivityType()
        {
            return ActivityType.LIGHTWEIGHT_ACTIVITY;
        }

        public override string getPropertyId()
        {
            return EBookConstant.INTERNAL_SPEECH_RECOGNITION_RESULT;
        }
    }
}