using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.devices.speech
{
    public interface SpeechRecognitionListener
    {
        void handleAudioLevelUpdate(int audioLevel);
        void handleRecognizeCompleted();
        void handleFinishLoadingGrammar(string grammarName);
        void handleAudioStateChanged(AudioState state);
        void handleSpeechHypothesizedResult(float confidence, string textResult,
            string grammarName, string ruleName, KeyValuePair<string, SemanticValue>[] kvp,
            double audioDuration);
        void handleSpeechRecognizedResult(float confidence, string textResult,
            string grammarName, string ruleName, KeyValuePair<string, SemanticValue>[] kvp,
            double audioDuration, RecognizedAudio audio);
        void handleRejectRecognition(float confidence, string textResult, KeyValuePair<string, SemanticValue>[] kvp);
    }
}