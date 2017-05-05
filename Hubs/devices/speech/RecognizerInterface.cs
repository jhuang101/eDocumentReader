using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Web;

namespace eDocumentReader.Hubs.devices.speech
{
    /// <summary>
    /// A simple recognizer interface.
    /// </summary>
    public interface RecognizerInterface
    {
        void setAudioStreamType(INPUT_STREAM_TYPE type);
        void initializeAudioStream();
        void updateStream(float[] data, int index);
        void updateStreamIndex(int index);

        void LoadGrammarAsync(Grammar g);
        void LoadGrammar(Grammar g);
        void UnloadGrammar(Grammar g);
        void UnloadAllGrammars();

        void start();
        void stop();
    }
}