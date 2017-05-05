using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    public class Page
    {
        private static readonly string SCENE_ACTION_PREFIX = "scene_";
        private Dictionary<string, string> def_images;
        private Dictionary<string, string> def_sounds;
        private Dictionary<string, StoryAction> def_actions;

        public int pageNumber;
        private string background_noise;
        private List<Paragraph> paragraphs;

        private string lastAction;

        public Page()
        {
            paragraphs = new List<Paragraph>();
        }

        public void SetPageNumber(int n)
        {
            pageNumber = n;
        }
        public int GetPageNumber()
        {
            return pageNumber;
        }
        public List<string> GetText()
        {
            List<string> ret = new List<string>();
            foreach(Paragraph p in paragraphs){
                ret.AddRange(p.GetText());
                ret.Add("");
            }
            return ret;
        }
        public List<string[]> GetListOfTextArray()
        {
            List<string[]> ret = new List<string[]>();
            foreach (Paragraph at in paragraphs)
            {
                ret.AddRange(at.GetListOfTextArray());
                ret.Add(null);//null value indicate a newline
            }
            return ret;
        }
        public List<string> GetListOfAnnotations()
        {
            List<string> groupAnnotation = new List<string>();
            foreach (Paragraph at in paragraphs)
            {
                groupAnnotation.AddRange(at.GetListOfAnnotations());
                groupAnnotation.Add(null);//null value indicate a newline
            }
            return groupAnnotation;
        }
        /*
         * This will return grammar string with semantic tag.
         * For example,
         * <item repeat="0-1">This is a test </item>
         * <item repeat="0-1">say frog <tag> out='action=[scene_x]'</tag></item>
         * ...
         */
        public List<string> getXMLGrammarString()
        {
            List<string> ret = new List<string>();
            foreach (Paragraph p in paragraphs)
            {
                //ret.AddRange(p.getXMLGrammarString());
                //ret.Add("");
            }
            return ret;
        }

        public void SetBackgroundNoise(string name)
        {
            background_noise = name;
        }
        public void AddParagraph(Paragraph p)
        {
            paragraphs.Add(p);
        }


        public void processAction(string ani)
        {
            if (ani.Equals(lastAction))
            {
                //do nothing if the given animation is the same as the last animation 
                return;
            }
            StoryAction sa;
            def_actions.TryGetValue(ani, out sa);
            if (sa != null)
            {
                sa.start(def_images, def_sounds);//.playAnimationOnly();
            }

            //cache the last action
            lastAction = ani;
        }
        

        public override string ToString()
        {
            string ret = "<page pagenumber="+pageNumber+">";
            foreach (Paragraph p in paragraphs)
            {
                ret += p + "\n";
            }
            ret += "</page>";
            return ret;
        }

        internal void AddActions(Dictionary<string, StoryAction> actions)
        {
            def_actions = actions;
        }
        internal void AddImages(Dictionary<string, string> images)
        {
            def_images = images;
        }
        internal void AddSounds(Dictionary<string, string> sounds)
        {
            def_sounds = sounds;
        }
    }
}