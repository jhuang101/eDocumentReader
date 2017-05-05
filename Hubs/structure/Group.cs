using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs
{
    /*
     * group of text or text with annotation. For example: "there is a <annotate object='book'>book</annotate> on the desk" 
     * contains three groups. group 1 = "there is a", group 2 = "<annotate object='book'>book</annotate>, and group 3 = "on the desk"
     */
    public class Group
    {
        private Dictionary<string, string> annotations; //anything other than action and microAction. For example: object='book'
        private Dictionary<string, string> actions; //action denote in the xml file
        private Dictionary<string, string> microActions; //microAction denote in the xml file
        private string text;
        private string[] textArray;
        private string annotationString;
        public Group()
        {
        }
        public Group(string text) : this()
        {
            this.text = text;
            this.textArray = text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            annotations = new Dictionary<string, string>();
            actions = new Dictionary<string, string>();
            microActions = new Dictionary<string, string>();
        }
        public void AddAnnotation(string key, string val)
        {
            if (key.CompareTo("action") == 0)
            {
                actions.Add(key, val);
            }
            else if (key.CompareTo("microAction") == 0)
            {
                microActions.Add(key, val);
            }
            else { 
                annotations.Add(key, val);
            }
        }
        public string GetText()
        {
            return text;
        }
        public string[] GetTextArray()
        {
            return textArray;
        }
        public Dictionary<string, string> getAnnotation()
        {
            return annotations;
        }
        public Dictionary<string, string> getActions()
        {
            return actions;
        }
        public Dictionary<string, string> getMicroActions()
        {
            return microActions;
        }
        public string getAllAnnotationString()
        {
            string ret = "";
            if (annotationString == null)
            {
                foreach (KeyValuePair<string, string> each in annotations)
                {
                    ret += each.Key + "=" + each.Value+";";
                }
                foreach (KeyValuePair<string, string> each in actions)
                {
                    ret += each.Key + "=" + each.Value+";";
                }
                foreach (KeyValuePair<string, string> each in microActions)
                {
                    ret += each.Key + "=" + each.Value+";";
                }
            }
            return ret;
        }
        public override string ToString()
        {
            string ret = text + ": ";
            foreach (KeyValuePair<string, string> pair in actions)
            {
                ret+=pair.Key + "->" + pair.Value + "; ";
            }
            foreach (KeyValuePair<string, string> pair in microActions)
            {
                ret += pair.Key + "->" + pair.Value + "; ";
            }
            foreach (KeyValuePair<string, string> pair in annotations)
            {
                ret += pair.Key + "->" + pair.Value + "; ";
            }
            return ret;
        }
    }
}