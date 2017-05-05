using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using System.Web;

namespace eDocumentReader.Hubs
{
    public class EBookUtil
    {
        /// <summary>
        /// Find how many syllables in the given array
        /// </summary>
        /// <param name="str"></param>
        /// <returns>array contains number corresponding to the given input array</returns>
        public static int[] CountSyllables(string[] str){
            int[] ret = new int[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                int syll = GetSyllables(str[i]);
                ret[i] = syll;
            }
            return ret;
        }
        /// <summary>
        /// Find how many syllables in in the given string
        /// http://www.howmanysyllables.com/howtocountsyllables
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetSyllables(string str)
        {
            string vowels = "aeiouy";
            str = str.ToLower();
            int ret = 0;
            int len = str.Length;

            if(len >0){
                //count number of A E I O U, and only count once if two or more vowels are together
                char[] arr = str.ToCharArray();
                bool previousIsVowel = false;
                for (int i = 0; i < len; i++)
                {
                    if (vowels.IndexOf(arr[i]) >= 0)
                    {
                        if (!previousIsVowel) //if two vowels together, only count one. For instance, "cried" has two vowels, but only count one
                        {
                            ret++;
                        }
                        previousIsVowel = true;
                    }
                    else
                    {
                        previousIsVowel = false;
                    }
                }

                //add one if the word end with "le"
                if (str.EndsWith("le"))
                {
                    //do nothing
                }
                else if (str.EndsWith("e")) //substract one if word end with "e", but not "le"
                {
                    ret--;
                }

            }

            if (ret <= 0)
            {
                ret = 1;
            }
            if (ret == 1)
            {
                ret++; //hack. if a word only contains one syllable, we assume it will take as much time as two syllables word
            }
            return ret;
        }

        /// <summary>
        /// Total milliseconds since 1970, 1, 1
        /// </summary>
        /// <returns></returns>
        public static double GetUnixTimeMillis()
        {
            return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }

        /// <summary>
        /// Timestamp in yyyyMMddHHmmssffff format.
        /// </summary>
        /// <returns></returns>
        public static String GetTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// Generate a list of grammars with the annotations attach to the 
        /// grammars' semantic tag.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="annotations"></param>
        /// <returns></returns>
        public static List<SrgsDocument> GenerateGrammars(string[] list, string[] annotations)
        {
            String timeStamp = GetTimestamp();
            Debug.WriteLine("Before generate grammar time:" + timeStamp);

            List<SrgsDocument> srgsDocs = new List<SrgsDocument>();

            for (int i = 0; i < list.Length; i++)
            {
                SrgsRule rule = new SrgsRule("index_" + i);
                SrgsOneOf so = new SrgsOneOf();
                for (int j = i; j < list.Length && j < i + EBookInteractiveSystem.MAX_WORDS_IN_SPEECH; j++)
                {

                    string ea = "";
                    List<string> anno = new List<string>();
                    for (int k = i; k <= j; k++)
                    {
                        ea += list[k] + " ";
                        if (!anno.Contains(annotations[k]))
                        {
                            anno.Add(annotations[k]);
                        }
                    }
                    string annotate = "";
                    foreach (string ann in anno)
                    {
                        if (ann.Trim().Length > 0)
                        {
                            annotate += ann;
                        }
                    }
                    ea = ea.TrimEnd();
                    SrgsItem si = new SrgsItem(ea);
                    if (annotate.Length > 0)
                    {
                        annotate = "out.annotation=\"" + annotate + "\";";
                    }
                    si.Add(new SrgsSemanticInterpretationTag(annotate + "out.startIndex=\"" + i + "\";"));
                    so.Add(si);
                }
                rule.Add(so);
                SrgsDocument srgsDoc = new SrgsDocument();
                srgsDoc.Rules.Add(rule);
                srgsDoc.Root = rule;
                srgsDocs.Add(srgsDoc);


            }
            String after = GetTimestamp();
            Debug.WriteLine("after generate grammar time:" + after);
            return srgsDocs;
        }

        /// <summary>
        /// Check if given string contains a pause punctuation.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool containPausePunctuation(string str){
            foreach(string p in EBookConstant.PAUSE_PUNCTUATION){
                if(str.Contains(p)){
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Check if given string contains an end of sentence punctuation.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool containEndOfSentencePunctuation(string str)
        {
            foreach (string p in EBookConstant.EOS_PUNCTUATION)
            {
                if (str.Contains(p))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convert the full path to relative path
        /// </summary>
        /// <param name="path">The full path of a file</param>
        /// <returns></returns>
        public static string convertAudioToRelativePath(string path)
        {
            //need to find a way not to hard code "Content" here
            string[] stringSeparator = { "Content" };
            string[] result = path.Split(stringSeparator, StringSplitOptions.None);
            if (result.Length > 1)
            {
                path = "../Content/" + result[1];
                Debug.WriteLine("changed full path "+path+"to relative Path = " + path);
            }
            return path;
        }

        /// <summary>
        /// Compile the given grammar to a file
        /// </summary>
        /// <param name="srgDoc">The document to be complied into cfg grammar file</param>
        /// <param name="cfgPath">The full path of the cfg file that will be save to</param>
        public static void CompileGrammarToFile(SrgsDocument srgDoc, string cfgPath)
        {
            FileStream fs = new FileStream(cfgPath, FileMode.Create);
            SrgsGrammarCompiler.Compile(srgDoc, (Stream)fs);
            fs.Close();
        }

  
    }
}