using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Speech.Recognition;
using eDocumentReader.Hubs.structure;

namespace eDocumentReader.Hubs
{
    public enum Mode{
        RECORD,
        REPLAY,
        REALTIME,
        TTS,
        UNKNOWN
    }
    public class StoryManager
    {
        private readonly string navigationCommandFileName = "command.grxml";
        private string storyDirectory;
        private Story currentStory;
        private string tests;
        List<Story> stories = new List<Story>();

        private TextProcessor textProcessor;
        private LogPlayer audioProcessor; //used to replay recorded speech
        private StoryLoggingDevice storyLogger;
        private Mode storyMode = Mode.UNKNOWN;

        public StoryManager()
        {

            textProcessor = new TextProcessor();
            audioProcessor = new LogPlayer();
            storyLogger = new StoryLoggingDevice();
        }

        public void init(string storyDirectory)
        {
            //initialize story
            foreach (string d in Directory.GetDirectories(storyDirectory))
            {
                foreach (string xmlMainFileName in Directory.GetFiles(d, "*.xml"))
                {
                    Story story = new Story(xmlMainFileName);
                    Debug.WriteLine(story.ToString());
                    stories.Add(story);
                }
            }
            this.storyDirectory = storyDirectory;
            textProcessor.SetStoryDirectory(storyDirectory);
            
        }
        public void stop()
        {
            audioProcessor.stop();
            audioProcessor = new LogPlayer();
        }
        public void pause()
        {
            audioProcessor.pause();
        }
        public void resume()
        {
            audioProcessor.resume();
        }

        public string[] getStoryNames()
        {
            string[] ret = new string[stories.Count];
            for (int i = 0; i < stories.Count; i++)
            {
                ret[i] = stories.ElementAt(i).GetStoryName();
            }
            return ret;
        }

        public void SetStory(string storyName)
        {
            foreach (Story s in stories)
            {
                if (s.GetStoryName().CompareTo(storyName) == 0)
                {
                    currentStory = s;
                    break;
                }
            }
            generateSpeechSynthesisData();
        }
        public string getCurrentStoryPath()
        {
            if(currentStory != null){
                return currentStory.getFullPath();
            }
            return null;
        }

        private void generateSpeechSynthesisData()
        {
            EBookSpeechSynthesizer.getInstance().generateSynthesisData(currentStory);
        }

        public List<string> getCommandGrammars()
        {
            List<string> grammarList = new List<string>();
            foreach (string grxmlFile in Directory.GetFiles(storyDirectory, navigationCommandFileName))
            {
                grammarList.Add(grxmlFile);
            }
            return grammarList;
        }

        public void changeStoryMode(Mode storyMode)
        {
            this.storyMode = storyMode;
        }

        public void startReplay(string path)
        {
            audioProcessor.processAudioFiles(path);
            textProcessor.process(currentStory.GetFirstPage(), storyMode);
        }

        public void start()
        {
            textProcessor.process(currentStory.GetFirstPage(), storyMode);
        }

        public void finishReplayAudio(int audioIndex)
        {
            audioProcessor.finishReplayAudio(audioIndex);
        }

        public void confirmHighlight()
        {
            textProcessor.confirmHighlight();
        }
        /*
         * USE IN RECORD MODE
         * The user click reject to clear out the unconfirm recordings
         */
        public void rollBackHighlight()
        {
            textProcessor.rollBackHighlight();
        }

        /*
         * update the recognized text
         */
        public void updateRecognizedText(float confidence, string textResult,
            bool isHypothesis, KeyValuePair<string, SemanticValue>[] semantics, string grammarName,
            string ruleName, double audioDuration, string wavPath)
        {
            textProcessor.processRecognitionResult(confidence, textResult,
            isHypothesis, semantics, grammarName,
            ruleName, audioDuration, wavPath);
        }
        /*
         * update the speech state.
         */
        public void updateSpeechState(SpeechState state)
        {
            textProcessor.setSpeechState(state);
        }

        public void updateAcousticHypothesis(int audioState, double startTime)
        {
            textProcessor.processAcousticHypothesisHighlight(audioState, startTime);
        }

        /**
         * remove the last highlight if the SR reject the intermidiate result at the final stage
         */
        public void removeLastHighlight()
        {
            textProcessor.rollBackText();
        }

        public Mode getStoryMode() { return storyMode; }

        /*
         * Change page in the story. 
         */
        public void changePage(PageAction pa, int pageNum)
        {
            if (pa == PageAction.NEXT)
            {
                Page page = currentStory.GetNextPage();
                if (page != null)
                {
                    textProcessor.process(page, storyMode);
                }

            }
            else if (pa == PageAction.PREVIOUS)
            {
                Page page = currentStory.GetPreviousPage();
                if (page != null)
                {
                    textProcessor.process(page, storyMode);
                }
            }
            else if (pa == PageAction.GO_PAGE_X)
            {
                Page page = currentStory.GetPage(pageNum);
                if (page != null)
                {
                    textProcessor.process(page, storyMode);
                }
            }
        }
    }
}