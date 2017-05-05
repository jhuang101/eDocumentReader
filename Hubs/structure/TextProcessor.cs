using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Speech.Recognition.SrgsGrammar;
using System.IO;
using System.Xml;
using eDocumentReader.Hubs.activities.system.lightweight;
using eDocumentReader.Hubs.structure;

namespace eDocumentReader.Hubs
{
    /*
     * Generate text for the given page. Produce highlighting for the recognized text.
     */
    public class TextProcessor
    {
        private System.Object lockThis = new System.Object();
        private readonly string GRAMMAR_TMP_DIR = "tmp"; //a directory store the temporary grammar files
        private readonly float PLAY_ANIMATION_CONFIDENT_THRESHOLD = 0.2f; //play the animation only when the intermidiate confidence is greater than this
        private readonly float HYP_THRESHOLD = 0.2f; //display the intermidiate result only when the conficence is greater than this
        private readonly double maxLag = 750; //milliseconds maximum possible lag time in a speech
        private readonly bool enableCache = true; //cache page's text and annotation in memory, fast loading time for the same page
        private readonly int INITIAL_LOOK_AHEAD = 4; //the minimal syllables to look ahead

        //indicate how many words a speech can skip, and the prototype considers it is a continuation
        //for example, consider this sentence "I am working on the protoype, and I am writting some comments."
        //if a reader say "I am working on the", and pause for a second, and say "and I am writing some comments"
        //The prototype will treat it as the reader say "I am working on the prototype, and I am writting some comments."
        //value in the SPEECH_JUMP_GAP_THRESHOLD is 2 in above example
        private readonly int SPEECH_JUMP_GAP_THRESHOLD = 2; 

        private Dictionary<int,string[]> pageTextCached = new Dictionary<int,string[]>();
        private Dictionary<int, string[]> pageTextHTMLCached = new Dictionary<int, string[]>();
        private Dictionary<int, string[]> annotationArrayCached = new Dictionary<int, string[]>();
        private Dictionary<int, int[]> syllableArrayCached = new Dictionary<int, int[]>();

        private List<int> defaultStartIndexes = new List<int>();
        
        private Page currentPage;
        private string[] allSpeechText;
        private string[] allSpeechTextHTML;
        private string[] annotationArray;
        private int[] syllableArray;

        private int confirmedStartIndex;
        private int confirmedEndIndex;
        private int confirmedLastSegPosition;

        private int hypothesisEndIndex;

        private int startIndex;
        private int endIndex;
        private int lastSegPosition;

        //for test only
        //private string intermediateSpeechText;
        //private string allConfirmedSpeechText;
        private bool guessAhead = true;
        private int numOfHypothsis;
        private string storyDirectory;


        private int lastConfirmIndexRM = -1;//to keep track the position where the last confirmed speech in record mode

        private int maxSyllableAhead;

        private Mode mode;

        private List<string> playedAnimationList = new List<string>();//keep track which animation already played in the current speech

        private int speechState; //value=1 means in speech, value=0 means silent

        private double timePerSyllable = 150; //continuously estimate the time to spoke one syllable, in ms. default value = 250ms

        public TextProcessor()
        {
        }

        /*
         * set current story's direction
         */
        public void SetStoryDirectory(string storyDirectory){
            this.storyDirectory = storyDirectory;
        }

        /*
         * Withdraw the last uncomfirmed hightlighting for the speech.
         * This method is called when reader click on reject button to remove the previous recorded speed
         * in record my voice mode.
         */
        public void rollBackHighlight()
        {
            confirmedLastSegPosition = lastConfirmIndexRM+1;
            lastSegPosition = confirmedLastSegPosition;

            if (lastConfirmIndexRM >= 0)
            {
                endIndex = lastConfirmIndexRM;
            }
            else
            {
                endIndex = 0;
            }

            confirmedEndIndex = endIndex;
            if (lastConfirmIndexRM < 0)
            {
                string pageTextStr = constructPageText();
                ActivityExecutor.add(new InternalUpdatePageTextActivity(pageTextStr, currentPage.GetPageNumber()));

            }
            else
            {
                constructTextAndDisplay();
            }


            string cfgPath = storyDirectory + "\\" + GRAMMAR_TMP_DIR + "\\" + currentPage.GetPageNumber() + "_" + 
                (endIndex + 1) + ".cfg";
            if (!defaultStartIndexes.Contains(endIndex + 1))
            {
                Grammar g = new Grammar(cfgPath);
                g.Weight = EBookConstant.NEXT_WORD_WEIGHT;
                g.Priority = EBookConstant.NEXT_WORD_PRIORITY;

                ActivityExecutor.add(new InternalReloadOnGoingGrammarActivity(g));
                Debug.WriteLine("Load onGoing grammar " + cfgPath);

            }
            else
            {
                ActivityExecutor.add(new InternalChangeGrammarPriorityActivity(endIndex+1));
            }


        }
        /*
         * Save the unconfirm speech.
         * This method is alled when user click on the accept button in record my voice mode
         */
        public void confirmHighlight()
        {
            lastConfirmIndexRM = confirmedEndIndex;
            if (confirmedEndIndex == allSpeechText.Length - 1)
            {
                ActivityExecutor.add(new InternalFinishPageActivity());
            }
        }

        /*
         * clean cached data
         */
        private void resetParameters()
        {
            defaultStartIndexes.Clear();

            confirmedStartIndex = 0;
            confirmedEndIndex = 0;
            confirmedLastSegPosition = 0;

            startIndex = -1;
            endIndex = -1;
            lastSegPosition = -1;

            numOfHypothsis = 0;

            lastConfirmIndexRM = -1;
            playedAnimationList.Clear();
        }

        /*
         * If Mode == REAL TIME, enable jump to different sentence. otherwise, disable jump to different sentence
         */
        public void process(Page page, Mode mode)
        {
            this.mode = mode;
            currentPage = page;

            //reset parameters when process a new page
            resetParameters();

            //retrieve data from cache
            if (enableCache && pageTextCached.ContainsKey(page.GetPageNumber()))
            {
                pageTextCached.TryGetValue(page.GetPageNumber(), out allSpeechText);
                pageTextHTMLCached.TryGetValue(page.GetPageNumber(), out allSpeechTextHTML);
                annotationArrayCached.TryGetValue(page.GetPageNumber(), out annotationArray);
                syllableArrayCached.TryGetValue(page.GetPageNumber(), out syllableArray);
            }
            else
            {
                List<string[]> pageText = page.GetListOfTextArray();
                List<string> annotation = page.GetListOfAnnotations();
                List<string> annArray = new List<string>();
                //cover list to array
                string whole = "";
                string wholeHTML = "";
                //obtain the text and the text in HTML format for the current page
                for(int i=0;i<pageText.Count;i++)
                {
                    if (pageText.ElementAt(i) == null)
                    {
                        wholeHTML = wholeHTML.TrimEnd() + "<br> ";
                    }
                    else
                    {
                        foreach (string str in pageText.ElementAt(i))
                        {
                            whole += str + " ";
                            if (str.Trim().Length == 0)
                            {
                                wholeHTML = wholeHTML.TrimEnd() + " ";
                            }
                            else
                            {
                                wholeHTML += str + " ";
                            }
                            annArray.Add(annotation.ElementAt(i));
                        }
                        //wholeHTML = wholeHTML.TrimEnd() + "<br> ";
                    }
                }
                whole = whole.Replace("\"", "");
                allSpeechText = whole.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                allSpeechTextHTML = wholeHTML.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                annotationArray = annArray.ToArray();
                syllableArray = EBookUtil.CountSyllables(allSpeechText);


                //save the data to hash map, we can simply retrieve the page data when 
                //the page get revisit again
                if (enableCache)
                {
                    pageTextCached.Add(page.GetPageNumber(), allSpeechText);
                    pageTextHTMLCached.Add(page.GetPageNumber(), allSpeechTextHTML);
                    annotationArrayCached.Add(page.GetPageNumber(), annotationArray);
                    syllableArrayCached.Add(page.GetPageNumber(), syllableArray);
                }
            }



                if (mode != Mode.REPLAY)
                {
                    defaultStartIndexes.Clear();
                    List<SrgsDocument> srgsDocs = EBookUtil.GenerateGrammars(allSpeechText, annotationArray);
                    Debug.WriteLine("generated " + srgsDocs.Count + " grammar files");

                    //load grammars
                    List<Grammar> gs = new List<Grammar>();
                    //loop from srgsDocs.Count to 0 will give the early sentence the priority.
                    for (int i = srgsDocs.Count; --i >= 0; )
                    {
                        string cfgPath = storyDirectory + "\\" + GRAMMAR_TMP_DIR + "\\" + page.GetPageNumber() + "_" + i + ".cfg";
                        Directory.CreateDirectory(storyDirectory + "\\" + GRAMMAR_TMP_DIR);
                        CompileGrammar(srgsDocs.ElementAt(i), cfgPath);
                        if (mode == Mode.REALTIME)
                        {
                            if (i == 0 || (i > 0 && (allSpeechText[i - 1].Contains("?") || allSpeechText[i - 1].Contains(".") || 
                                allSpeechText[i - 1].Contains("!"))))
                            {
                                defaultStartIndexes.Add(i);
                                Debug.WriteLine("loading grammar:" + cfgPath);
                                Grammar storyG = new Grammar(cfgPath);
                                storyG.Weight = EBookConstant.DEFAULT_WEIGHT;
                                storyG.Priority = EBookConstant.DEFAULT_PRIORITY;

                                gs.Add(storyG);

                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                defaultStartIndexes.Add(i);
                                Debug.WriteLine("loading grammar:" + cfgPath);
                                Grammar storyG = new Grammar(cfgPath);
                                storyG.Weight = EBookConstant.DEFAULT_WEIGHT;
                                storyG.Priority = EBookConstant.DEFAULT_PRIORITY;

                                ActivityExecutor.add(new InternalReloadOnGoingGrammarActivity(storyG));
                            }
                        }
                    }
                    if (gs.Count > 0)
                    {
                        ActivityExecutor.add(new InternalReloadStoryGrammarActivity(gs));
                    }
                }
            string pageTextStr = constructPageText();
            ActivityExecutor.add(new InternalUpdatePageTextActivity(pageTextStr, page.pageNumber));

        }

        /*
         * Compile the given grammar to a file
         */
        private void CompileGrammar(SrgsDocument srgDoc, string cfgPath)
        {
            FileStream fs = new FileStream(cfgPath, FileMode.Create);
            SrgsGrammarCompiler.Compile(srgDoc, (Stream)fs);
            fs.Close();
        }

        /*
         * return 1 if the result is a complete speech and it is the end of the page.
         */
        private int processSpeechResult(string result, int startInd, bool isH, float confidence, double duration)
        {
            //lock this method, in case the this method will take more time than the SR
            lock (lockThis)
            {
                int ret = 0;
                string[] arr = result.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);

                if (isH)
                {
                    int increment = 0;
                    if (guessAhead && EBookInteractiveSystem.lookAheadDivider > 0 && syllableArray.Length >= arr.Length+startInd)
                    {
                        //numOfHypothsis++;
                        int syCount = 0;
                        for (int i = startInd; i < arr.Length + startInd; i++)
                        {
                            syCount += syllableArray[i];
                        }
                        int syIn = syCount / EBookInteractiveSystem.lookAheadDivider; //one syllable forward for every lookAheadDivide

                        if (syIn < INITIAL_LOOK_AHEAD)
                        {
                            //The highlighting seems slow in the first second of a speech, let's highlight 2 syllables
                            //ahead in the beginning of a speech
                            syIn = INITIAL_LOOK_AHEAD;
                        }

                        int enInc = startInd + arr.Length;

                        if (maxSyllableAhead > 0)
                        {
                            //Debug.WriteLine("Time pier syllable=" + timePerSyllable);
                            if (syIn > maxSyllableAhead)
                            {
                                syIn = (int)maxSyllableAhead;
                            }
                        }

                        string currentEndWord = "";
                        if (enInc > 0 && enInc <= allSpeechText.Length)
                        {
                            currentEndWord = allSpeechText[enInc - 1];
                        }
                        Boolean cont = true;
                        while (syIn > 0 && cont)
                        {
                            if (enInc < syllableArray.Length)
                            {
                                string guessWord = allSpeechText[enInc];
                                //if the current end word has pause punctuation, stop look ahead
                                if (EBookUtil.containPausePunctuation(currentEndWord))
                                {
                                    
                                    Debug.WriteLine("currentEndWord \"" + currentEndWord + "\" contains pause, stop look ahead");
                                    break;
                                }
                                else if (EBookUtil.containPausePunctuation(guessWord))
                                {
                                    //reduce 4 syllables from enInc
                                    syIn -= 3;
                                    //no guess ahead if there is any possible pause in guess ahead word.
                                    //Debug.WriteLine("guessWord \"" + guessWord + "\" contains pause, stop look ahead");
                                    //cont = false;
                                }
                                if (syIn >= syllableArray[enInc])
                                {
                                    increment++;
                                }
                                syIn -= syllableArray[enInc];
                                enInc++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Debug.WriteLine("guess " + increment + " word(s) ahead");
                    }

                    //ONLY DISPLAY INTERMIDIATE RESULT WITH CONFIDENCE SCORE GREATER THAN HYP_THRESHOLD
                    if (confidence > HYP_THRESHOLD)
                    {
                        if (startInd == lastSegPosition) //continue with last segment
                        {
                            startIndex = confirmedStartIndex;
                            hypothesisEndIndex = arr.Length + lastSegPosition - 1;
                            endIndex = hypothesisEndIndex + increment;
                        }
                        else if (arr.Length > 1) //jumping to onther sentence when at least two word in the speech
                        {
                            int gap = startInd - confirmedEndIndex;
                            if (gap > 0 && gap <= SPEECH_JUMP_GAP_THRESHOLD)
                            {
                                //if reader skip maximum of SPEECH_JUMP_GAP_THRESHOLD, consider 
                                //the continue highlight the sentence
                            }
                            else
                            {
                                startIndex = startInd;
                            }
                            hypothesisEndIndex = arr.Length + startInd - 1;
                            endIndex = hypothesisEndIndex + increment;
                        }
                    }

                }
                else
                {
                    //duration != -1 only when !isH, so we need to estimate the time 
                    //per syllable for the next scentence using the previous speech
                    int syCount = 0;
                    for (int i = startInd; i < arr.Length + startInd; i++)
                    {
                        syCount += syllableArray[i];
                    }
                    timePerSyllable = duration / syCount;
                    Trace.WriteLine("timePerSyllable: " + timePerSyllable);
                    maxSyllableAhead = (int)(maxLag / timePerSyllable); //

                    numOfHypothsis = 0;
                    if (startInd == lastSegPosition)
                    {
                        //if number of confirmed word in the complete speech is one word less than imcomplete speech, 
                        //we can treat the last imcomplete speech as complete
                        int completeEndIndex = arr.Length + lastSegPosition - 1;
                        if (endIndex - completeEndIndex != 1)
                        {
                            endIndex = completeEndIndex;
                        }
                        lastSegPosition = endIndex + 1;
                    }
                    else
                    {
                        int gap = startInd - lastSegPosition;
                        if (gap > 0 && gap <= SPEECH_JUMP_GAP_THRESHOLD)
                        {
                            //if reader skip maximum of SPEECH_JUMP_GAP_THRESHOLD, consider 
                            //the continue highlight the sentence
                        }
                        else
                        {
                            startIndex = startInd;
                        }
                        endIndex = arr.Length + startInd - 1;
                        lastSegPosition = endIndex + 1;
                    }
                    confirmedStartIndex = startIndex;
                    confirmedEndIndex = endIndex;
                    confirmedLastSegPosition = lastSegPosition;
                    if (mode != Mode.REPLAY)
                    {
                        //EBookSRDevice rec = EBookSRDevice.GetInstance();

                        //reach the end of the page
                        if (confirmedEndIndex == allSpeechText.Length - 1)
                        {
                            //stop SR when transitioning to the next page
                            //rec.enableSR(false);
                            //ret = 1;
                            //AbstractEBookEvent.raise(new FinishPageEvent());
                        }
                        else
                        {
                            string cfgPath = storyDirectory + "\\" + GRAMMAR_TMP_DIR + "\\" + 
                                currentPage.GetPageNumber() + "_" + (endIndex + 1) + ".cfg";
                            if (!defaultStartIndexes.Contains(endIndex + 1))
                            {
                                //rec.enableSR(false);
                                //rec.UnloadOnGoingGrammar();

                                Grammar g = new Grammar(cfgPath);
                                g.Weight = EBookConstant.NEXT_WORD_WEIGHT;
                                g.Priority = EBookConstant.NEXT_WORD_PRIORITY;
                                //rec.LoadOnGoingGrammar(g);
                                ActivityExecutor.add(new InternalReloadOnGoingGrammarActivity(g));

                                Debug.WriteLine("Load onGoing grammar " + cfgPath);
                                //rec.GenerateAndLoadOnGoingGrammars(allSpeechText,endIndex+1);
                                //rec.enableSR(true);
                            }
                            else
                            {
                                //the next sentence has higher priority
                                //rec.ReloadAndChangeGrammarPriority(endIndex + 1);
                                ActivityExecutor.add(new InternalChangeGrammarPriorityActivity(endIndex + 1));

                            }
                        }
                    }

                    if (mode != Mode.RECORD && confirmedEndIndex == allSpeechText.Length - 1)
                    {
                        //the complete recognized result reaches the end of the page
                        ret = 1;
                    }

                    //Detect speech and complete
                    //AbstractEBookEvent.raise(new CompleteSpeechEvent());

                }
                Debug.WriteLine("startIndex=" + startIndex);
                Debug.WriteLine("endIndex=" + endIndex);
                Debug.WriteLine("lastSegPosition=" + lastSegPosition);

                Debug.WriteLine("confirmedStartIndex=" + confirmedStartIndex);
                Debug.WriteLine("confirmedEndIndex=" + confirmedEndIndex);
                Debug.WriteLine("confirmedLastSegPosition=" + confirmedLastSegPosition);
                return ret;
            }
        }
        /*
         * construct the highlight text and raise in a updateSpeechTextEvent
         */
        private void constructTextAndDisplay()
        {
            //if startIndex not equals to confirmedStartIndex, the hypothesis result 
            //starts from different position.

            
            string displayText = "<span class='storytext'>";
            for (int i = 0; i < allSpeechTextHTML.Length; i++)
            {

                //the on going speech is not yet confirmed
                if (startIndex == confirmedStartIndex && endIndex > confirmedEndIndex){
                    //if it is the beginning (no confirmed speech)
                    if (confirmedEndIndex == 0)
                    {
                        if (startIndex == i && endIndex == i)
                        {
                            if (endIndex > hypothesisEndIndex)
                            {
                                displayText += "<span class='hypothesis'>" + allSpeechTextHTML[i] + " </span> <span class='lookAhead'>" ;
                            }
                            else
                            {
                                displayText += "<span class='hypothesis'>" + allSpeechTextHTML[i] + " </span> ";
                            }
                        }
                        else if (startIndex == i)
                        {
                            displayText += " <span class='hypothesis'>" + allSpeechTextHTML[i] + " ";
                        }
                        else if (hypothesisEndIndex == i && hypothesisEndIndex < endIndex)
                        {
                            displayText += allSpeechTextHTML[i] + " </span> <span class='lookAhead'>";
                        }
                        else if (endIndex == i)
                        {
                            displayText += allSpeechTextHTML[i] + " </span> ";
                        }
                        else
                        {
                            displayText += allSpeechTextHTML[i] + " ";
                        }
                    }
                    else
                    {
                        if (startIndex == i && endIndex == i)
                        {
                            if (endIndex > hypothesisEndIndex)
                            {
                                displayText += "<span class='hypothesis'>" + allSpeechTextHTML[i] + " </span> <span class='lookAhead'>" ;
                            }
                            else
                            {
                                displayText += "<span class='hypothesis'>" + allSpeechTextHTML[i] + " </span> ";
                            }
                        }
                        else if (startIndex == i)
                        {
                            displayText += "<span class='highlight'>" + allSpeechTextHTML[i] + " ";
                        }
                        else if (confirmedEndIndex == i)
                        {
                            displayText += allSpeechTextHTML[i] + " </span> <span class='hypothesis'>";
                        }
                        else if (hypothesisEndIndex == i && hypothesisEndIndex < endIndex)
                        {
                            displayText += allSpeechTextHTML[i] + " </span> <span class='lookAhead'>";
                        }
                        else if (endIndex == i)
                        {
                            displayText += allSpeechTextHTML[i] + " </span> ";
                        }
                        else
                        {
                            displayText += allSpeechTextHTML[i] + " ";
                        }
                    }
                }
                //the on going speech is not yet confirmed and the hypothesis result starts from different position
                else if (startIndex != confirmedStartIndex && startIndex != -1)
                {
                    //display the confimred words
                    if (confirmedStartIndex == i && confirmedEndIndex == i)
                    {
                        displayText += "<span class='highlight'>" + allSpeechTextHTML[i] + " </span> ";
                    }
                    else if (confirmedStartIndex == i)
                    {
                        displayText += "<span class='highlight'>" + allSpeechTextHTML[i] + " ";
                    }
                    else if (confirmedEndIndex == i)
                    {
                        displayText += allSpeechTextHTML[i] + " </span> ";
                    }
                    else if (startIndex == i)
                    {
                        displayText += "<span class='hypothesis'>" + allSpeechTextHTML[i] + " ";
                    }
                    else if (endIndex == i)
                    {
                        displayText += allSpeechTextHTML[i] + " </span> ";
                    }
                    else
                    {
                        displayText += allSpeechTextHTML[i] + " ";
                    }
                }
                //all spoken speech are confirmed
                else
                {
                    if (startIndex == i && endIndex == i)
                    {
                        displayText += "<span class='highlight'>" + allSpeechTextHTML[i] + "</span> ";
                    }
                    else if (startIndex == i)
                    {
                        displayText += "<span class='highlight'>" + allSpeechTextHTML[i] + " ";
                    }
                    else if (endIndex == i)
                    {
                        displayText += allSpeechTextHTML[i] + "</span> ";
                    }
                    else
                    {
                        displayText += allSpeechTextHTML[i] + " ";
                    }
                }
            }
            displayText += "</span>";
            Debug.WriteLine(displayText);
            //AbstractEBookEvent.raise(new UpdateSpeechTextEvent(displayText));
            ActivityExecutor.add(new InternalUpdateTextHighLightActivity(displayText));
        }

        /*
         * Construct the html text for the current page with no highlight
         */
        private string constructPageText()
        {
            string displayText = "<span class='storytext'>";
            for (int i = 0; i < allSpeechTextHTML.Length; i++)
            {
                displayText += allSpeechTextHTML[i] + " ";
            }
            displayText += "</span>";
            return displayText;
        }

        /*
         * play the animation for a completed recognition, do nothing if the 
         * animation already been played in the hypothesis result.
         * 
         */
        private void processAnimation(int reverseIndex)
        {
            //preserve the last animation in the list
            string lastAnim = "";
            if (playedAnimationList.Count > 0)
            {
                lastAnim = playedAnimationList.ElementAt(playedAnimationList.Count-1);
                playedAnimationList.Clear();
            }

            int maxLen = annotationArray.Length;
            for (int i = reverseIndex; i >= 0 && i < maxLen; i--)
            {
                if (annotationArray[i].Length > 0)
                {
                    string[] annX = annotationArray[i].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string each in annX)
                    {
                        if (each.StartsWith("action="))
                        {
                            string action = each.Substring(7);
                            if (currentPage != null)
                            {
                                if (!lastAnim.Equals(action))
                                {
                                    Trace.WriteLine("Processing animation:" + action);
                                    currentPage.processAction(action);
                                    //playedAnimationList.Add(action);
                                    return;
                                }
                                else
                                {
                                    //the latest animation was just played in hypothesis result
                                    //don't need to do anything
                                    return;
                                }
                                
                            }

                        }
                    }
                }
            }
        }

        /*
         * play the animation within the hypothesis text.
         */
        private void processHypothesisAnimation(int textLength, int start)
        {
            //just play the last animation within the result
            int endIndex = textLength + start;
            endIndex = Math.Min(endIndex, annotationArray.Length - 1);
            for (int i = endIndex; i >= start; i--)
            {
                if (annotationArray[i].Length > 0)
                {
                    string[] annX = annotationArray[i].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string each in annX)
                    {
                        if (each.StartsWith("action="))
                        {
                            string action = each.Substring(7);
                            if (currentPage != null && !playedAnimationList.Contains(action))
                            {
                                Trace.WriteLine("Processing animation:" + action);
                                currentPage.processAction(action);
                                playedAnimationList.Add(action);
                                return;
                            }

                        }
                    }
                }
            }
        }

        /*
         * take out the hypothesis highlighting if SR return recognition result or 
         * if the audio contains no speech.
         */
        private void withdrawAudioEnergyHypothesizedHighlight()
        {
            Trace.WriteLine("withdraw highlight from " + endIndex + " to " + confirmedEndIndex);
            startIndex = confirmedStartIndex;
            endIndex = confirmedEndIndex;
            if (startIndex == endIndex && endIndex == 0)
            {
                //display no highlight
                string pageTextStr = constructPageText();
                //AbstractEBookEvent.raise(new UpdatePageTextEvent(pageTextStr, currentPage.GetPageNumber()));
                ActivityExecutor.add(new InternalUpdatePageTextActivity(pageTextStr, currentPage.GetPageNumber()));

            }
            else
            {
                //display the previous highlight
                constructTextAndDisplay();
            }
            
        }

        /*
         * The SR detects the audio energy jumps to a significant level, but hasn't yet declear 
         * it is a speech. The audio can be the beginning
         * of a speech, or any other noise. In all cases, we want to start highlighting
         * the next word to reduce the delay.
         */
        private void processAudioEnergyHypothesizedHighlight(double audioStartTime)
        {
            if (EBookInteractiveSystem.initialLookAhead > 0 )
            {
                double elapsedTime = GetUnixTime() - audioStartTime;

                int syIn = (int)(elapsedTime / timePerSyllable);
                int MAX_INITIAL_HIGHLIGHT = EBookInteractiveSystem.initialLookAhead;
                int forLoopMax = confirmedEndIndex + 1 + MAX_INITIAL_HIGHLIGHT;
                int syCount = 0;
                for (int i = confirmedEndIndex+1; i < forLoopMax && i < syllableArray.Length; i++)
                {
                    syCount += syllableArray[i];
                }
                //int syIn = syCount / EBookDialogueSystem.lookAheadDivider; //one syllable forward for every lookAheadDivide

                //make sure we can highlight at least one word
                if (confirmedEndIndex +1 < syllableArray.Length &&  syIn < syllableArray[confirmedEndIndex + 1])
                {
                    syIn = syllableArray[confirmedEndIndex + 1];
                }
                //the upperbound
                if (syIn > syCount)
                {
                    syIn = syCount;
                }

                int enInc = confirmedEndIndex+1;

                Boolean cont = true;
                int increment = 0;
                while (syIn > 0 && cont)
                {
                    if (enInc < syllableArray.Length)
                    {
                        if (syIn >= syllableArray[enInc])
                        {
                            increment++;
                        }
                        syIn -= syllableArray[enInc];
                        enInc++;
                    }
                    else
                    {
                        break;
                    }
                }
                Trace.WriteLine("(audio level)guess " + increment + " word(s) ahead");
                startIndex = confirmedStartIndex;
                endIndex = confirmedEndIndex + increment;
            }
            constructTextAndDisplay();
        }

        /*
         * Interpret the meaning for a command speech.
         * Caution: the string comparison in this function for the commands are
         * rely on the script in command.grxml. If you can something in the command.grxml file
         * you may need to edit command string in this function, vice versa.
         */
        private void processCommandSemantics(KeyValuePair<string, SemanticValue>[] semantics)
        {
            foreach (KeyValuePair<string, SemanticValue> each in semantics)
            {
                if (each.Key.CompareTo("NavigatePage") == 0)
                {
                    if (each.Value.Value.ToString().CompareTo("[next]") == 0)
                    {
                        //AbstractEBookEvent.raise(new ChangePageEvent(PageAction.NEXT));
                        ActivityExecutor.add(new InternalSpeechNavigatePageActivity(PageAction.NEXT));

                    }
                    else if (each.Value.Value.ToString().CompareTo("[previous]") == 0)
                    {
                        //AbstractEBookEvent.raise(new ChangePageEvent(PageAction.PREVIOUS));
                        ActivityExecutor.add(new InternalSpeechNavigatePageActivity(PageAction.PREVIOUS));

                    }
                    else
                    {
                        int pageN = Convert.ToInt32(each.Value.Value);
                        //AbstractEBookEvent.raise(new ChangePageEvent(PageAction.GO_PAGE_X, pageN));
                        ActivityExecutor.add(new InternalSpeechNavigatePageActivity(PageAction.GO_PAGE_X, pageN));
                    }
                }
            }
        }

        private void processRecognitionResult(RecognitionResult result)
        {
            processRecognitionResult(result.confidence, result.textResult, result.isHypothesis, 
                result.semanticResult, result.grammarName,
                result.ruleName, result.audioDuration, result.wavPath);
        }

        /*
         * process the recognition result from SR, the recognition result can be the
         * hypothesis result or the complete result.
         */
        public void processRecognitionResult(float confidence, string textResult,
            bool isHypothesis, KeyValuePair<string, SemanticValue>[] semantics, string grammarName,
            string ruleName, double audioDuration, string wavPath)
        {
            
            //handle result if the recognized speech is a command
            if (grammarName.CompareTo("command") == 0)
            {
                if (!isHypothesis && confidence*100 > EBookInteractiveSystem.commandConfidenceThreshold)
                {
                    processCommandSemantics(semantics);
                }
            }
            //handle result if this is story text
            else
            {
                int start = -1; //the index of the first word of the recognized text in the current page
                //process the story annotations
                if (semantics != null && semantics.Length > 0)
                {
                    foreach (KeyValuePair<string, SemanticValue> each in semantics)
                    {
                        if (each.Key.CompareTo("startIndex") == 0)
                        {
                            start = Convert.ToInt32(each.Value.Value);
                        }
                    }

                }

                if (start == -1)
                {
                    string rule = ruleName;
                    if (rule.StartsWith("index_"))
                    {
                        string startIndex = rule.Substring(6);
                        if (startIndex.Length > 0)
                        {
                            start = Convert.ToInt32(startIndex);
                        }
                    }
                }

                //process the highlighting before animation (try to underline the text as fast as possible)
                Debug.WriteLine(textResult);
                Trace.WriteLine("start process Text time: " + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                int isEndOfPage = processSpeechResult(textResult, start, isHypothesis, 
                    confidence, audioDuration);
                constructTextAndDisplay();
                Trace.WriteLine("end process Text time: " + DateTime.Now.ToString("yyyyMMddHHmmssfff"));


                //for some reasons, the hypothesis results do not contain any semantic results, so 
                //we have to find it manually
                if (isHypothesis)
                {
                    string[] tmp = textResult.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    //it is offen misrecognize the first word in a speech.
                    //generate animation when hypothsis text is greater than 1
                    if (tmp.Length > 1)
                    {
                        processHypothesisAnimation(tmp.Length, start);
                    }
                }
                else
                {
                    string[] tmp = textResult.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    processAnimation(tmp.Length+start);

                    //keep the last action in the list, remove rest of the them
                    //string lastAction = playedAnimationList.Last();
                    //playedAnimationList.Clear();
                    //playedAnimationList.Add(lastAction);
                    if (isEndOfPage == 1)
                    {
                        //Debug.WriteLine("generating a finishpageactivity "+isHypothesis);
                        //AbstractEBookEvent.raise(new FinishPageEvent());
                        ActivityExecutor.add(new InternalFinishPageActivity());
                    }
                }


                

            }
        }

        public double GetUnixTime()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        public void setSpeechState(SpeechState state)
        {
            if (state == SpeechState.SPEECH_START)
            {
                speechState = 1;
            }
            else if (state == SpeechState.SPEECH_END)
            {
                speechState = 0;
            }
        }

        public void processAcousticHypothesisHighlight(int audioState, double startTime)
        {
            if (speechState == 0 && audioState >= 0)
            {
                processAudioEnergyHypothesizedHighlight(startTime);
            }
            else if (speechState == 0 && audioState < 0)
            {
                withdrawAudioEnergyHypothesizedHighlight();
            }
        }

        
        //SR rejects the recent hypothesis result, roll the highlighting to previous detect speech
        public void rollBackText()
        {
            numOfHypothsis = 0;

            lastSegPosition = confirmedLastSegPosition;
            startIndex = confirmedStartIndex;
            endIndex = confirmedEndIndex;
            processAnimation(endIndex);

            //construct the text without highlight if the rejected recognition is the first sentence/word of the page
            if (confirmedLastSegPosition == confirmedStartIndex && confirmedStartIndex == confirmedEndIndex &&
                confirmedEndIndex == 0)
            {
                string pageTextStr = constructPageText();
                //AbstractEBookEvent.raise(new UpdatePageTextEvent(pageTextStr, currentPage.GetPageNumber()));
                ActivityExecutor.add(new InternalUpdatePageTextActivity(pageTextStr, currentPage.GetPageNumber()));
            }
            else
            {
                //construct html text with highlight
                constructTextAndDisplay();
            }
        }
    }
}