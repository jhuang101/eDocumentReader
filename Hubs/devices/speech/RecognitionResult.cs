using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Speech.Recognition;

namespace eDocumentReader.Hubs
{
    public class RecognitionResult
    {
        public float confidence;
        public string textResult;
        public bool isHypothesis;
        public KeyValuePair<string, SemanticValue>[] semanticResult;
        public string grammarName;
        public string ruleName;
        public double audioDuration; //milliseconds
        public string wavPath;

        public double startTime; //this parameter use in replay mode.
        public bool endPage = false;

        public RecognitionResult(float confidence, string textResult, bool isHyp, KeyValuePair<string, SemanticValue>[] semantic, string grammarName,string ruleName, double duration,string wavPath)
        {
            this.confidence = confidence;
            this.textResult = textResult;
            this.isHypothesis = isHyp;
            this.semanticResult = semantic;
            this.grammarName = grammarName;
            this.ruleName = ruleName;
            this.audioDuration = duration;
            this.wavPath = wavPath;
        }

        public RecognitionResult()
        {
        }
    }
}