using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /*
     * Each paragraph denote by <p></p> tag in the story xml file
     */
    public class Paragraph
    {
        private List<ActionText> actionTexts;
        public Paragraph()
        {
            actionTexts = new List<ActionText>();
        }
        public void AddActionText(ActionText at)
        {
            actionTexts.Add(at);
        }
        public List<string> GetText()
        {
            List<string> ret = new List<string>();
            foreach (ActionText at in actionTexts)
            {
                ret.Add(at.GetText());
            }
            return ret;
        }
        public List<string[]> GetListOfTextArray()
        {
            List<string[]> ret = new List<string[]>();
            foreach (ActionText at in actionTexts)
            {
                ret.AddRange(at.GetListOfTextArray());
                ret.Add(null);//null value indicate a newline
            }
            return ret;
        }
        public List<string> GetListOfAnnotations()
        {
            List<string> groupAnnotation = new List<string>();
            foreach (ActionText at in actionTexts)
            {
                groupAnnotation.AddRange(at.GetListOfAnnotations());
                groupAnnotation.Add(null);//null value indicate a newline
            }
            return groupAnnotation;
        }
        public override string ToString()
        {
            string ret = "<p>";
            foreach (ActionText at in actionTexts)
            {
                ret += at + "\n";
            }
            ret += "</p>";
            return ret;
        }
    }
}