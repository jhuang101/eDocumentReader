using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /*
     * represent the <actionText> tag in the story xml file
     * Each line of text in a paragrah that contains one or more groups
     * 
     */
    public class ActionText
    {
        List<Group> groups;
        public ActionText()
        {
            groups = new List<Group>();
        }
        public void AddGroup(Group g)
        {
            groups.Add(g);
        }
        public string GetText()
        {
            string ret = "";
            foreach (Group g in groups)
            {
                ret+=g.GetText();
            }
            return ret;
        }
        public List<string[]> GetListOfTextArray()
        {
            List<string[]> groupArray = new List<string[]>();
            foreach (Group g in groups)
            {
                groupArray.Add(g.GetTextArray());
            }
            return groupArray;
        }
        public List<string> GetListOfAnnotations()
        {
            List<string> groupAnnotation = new List<string>();
            foreach (Group g in groups)
            {
                groupAnnotation.Add(g.getAllAnnotationString());
            }
            return groupAnnotation;
        }
        public override string ToString()
        {
            string ret = "<<";
            foreach (Group g in groups)
            {
                ret += g.ToString();
            }
            ret += ">>";
            return ret;
        }
    }
}