using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.IO;

namespace eDocumentReader.Hubs
{
    public class Story
    {
        private string storyName;
        private string storyPath;
        //private Dictionary<string, string> grammars;
        private Dictionary<string, string> images;
        private Dictionary<string, string> sounds;
        private Dictionary<string, StoryAction> actions;
        private List<Page> pages;
        private int currentPage;

        public Story(string xmlFileName)
        {
            StoryXMLParser storyParser = new StoryXMLParser(xmlFileName);
            storyName = storyParser.GetStoryName();
            images = storyParser.GetImages();
            sounds = storyParser.GetSounds();
            actions = storyParser.GetStoryAction();
            pages = storyParser.GetSortedPages();
            foreach (Page p in pages)
            {
                p.AddActions(actions);
                p.AddImages(images);
                p.AddSounds(sounds);
            }

            storyPath = Directory.GetParent(xmlFileName).FullName;
  
        }

        public string GetStoryName()
        {
            return storyName;
        }
        public string getFullPath()
        {
            return storyPath;
        }
        public List<string> GetFirstPageText()
        {
            Page p = pages.First();
            return p.GetText();
        }
        public Page GetFirstPage()
        {
            currentPage = 1;
            return pages.First();
        }
        public Page GetPage(int i)
        {
            //page 1 is element 0
            i--;

            if (i >= 0 && i < pages.Count)
            {
                currentPage = i+1;
                return pages.ElementAt(i);
            }
            return null;
        }
        public Page GetNextPage()
        {
            return GetPage(currentPage + 1); //
        }
        public Page GetPreviousPage()
        {
            return GetPage(currentPage - 1);
        }
        public override string ToString()
        {
            string ret = storyName + ": ";
            foreach (KeyValuePair<string, string> pair in images)
            {
                ret += pair.Key + "->" + pair.Value + "; ";
            }
            foreach (KeyValuePair<string, string> pair in sounds)
            {
                ret += pair.Key + "->" + pair.Value + "; ";
            }
            foreach (KeyValuePair<string, StoryAction> pair in actions)
            {
                ret += pair.Key + "->" + pair.Value + "; ";
            }
            foreach (Page pair in pages)
            {
                ret += pair + "; ";
            }
            return ret;
        }


    }
}