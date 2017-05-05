using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Collections;
using System.Diagnostics;

namespace eDocumentReader.Hubs
{
    /// <summary>
    /// A parser to read and parse the story xml file
    /// </summary>
    public class StoryXMLParser
    {
        private string storyName;
        //private Dictionary<string, string> grammars;
        private Dictionary<string, string> images; //image resource
        private Dictionary<string, string> sounds;
        private Dictionary<string, StoryAction> actions;
        private List<Page> pages;

        ArrayList list = new ArrayList();

        /// <summary>
        /// Parse the given story xml file
        /// </summary>
        /// <param name="filePath">The path of the xml file</param>
        public StoryXMLParser(string filePath)
        {
            images = new Dictionary<string, string>();
            sounds = new Dictionary<string, string>();
            actions = new Dictionary<string, StoryAction>();
            pages = new List<Page>();

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            if (doc.DocumentElement.Name.CompareTo("story") == 0)
            {
                if (doc.DocumentElement.Attributes != null)
                {
                    for (int i = 0; i < doc.DocumentElement.Attributes.Count; i++)
                    {
                        //find the story name
                        if (doc.DocumentElement.Attributes.Item(i).Name.CompareTo("name") == 0)
                        {
                            storyName = doc.DocumentElement.Attributes.Item(i).Value;
                        }
                    }
                }
            }
            //access each node
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                accessNodeTree(node);
            }
        }

        /// <summary>
        /// Get information from the image definition node
        /// </summary>
        /// <param name="node"></param>
        public void AccessImageDefinitionNode(XmlNode node)
        {
            if (node.Attributes != null){
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes.Item(i).Name.CompareTo("src") == 0)
                        {
                            images.Add(node.InnerText, node.Attributes.Item(i).Value);
                        }
                 }
            }
        }

        /*
         * Get information from the sound definition node
         */
        public void AccessSoundDefinitionNode(XmlNode node){
            if (node.Attributes != null){
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes.Item(i).Name.CompareTo("src") == 0)
                        {
                            sounds.Add(node.InnerText, node.Attributes.Item(i).Value);
                        }
                 }
            }
        }

        /*
         * Get information form the action definition node
         */
        public void AccessActionDefinitionNode(XmlNode node){
            if (node.Attributes != null){
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes.Item(i).Name.CompareTo("name") == 0)
                        {
                            StoryAction sa = new StoryAction(node.Attributes.Item(i).Value);

                            if (node.HasChildNodes)
                            {
                                foreach (XmlNode cNode in node.ChildNodes)
                                {
                                    if (cNode.Name.CompareTo("play_animation") == 0)
                                    {
                                        sa.addAnimationAction(cNode.InnerText);
                                    }
                                    else if (cNode.Name.CompareTo("change_bg") == 0)
                                    {
                                        sa.addBackgroundAction(cNode.InnerText);
                                    }
                                }
                            }
                            actions.Add(sa.getName(), sa);
                        }
                 }
            }
        }

        /*
         * Get the information from the page definition node.
         * the page node contains story content and linkage to
         * actions,etc.
         */
        public void AccessPageDefinitionNode(XmlNode node){
            Page page = new Page();
            if (node.Attributes != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (node.Attributes.Item(i).Name.CompareTo("number") == 0)
                    {
                        page.SetPageNumber(Convert.ToInt32(node.Attributes.Item(i).Value));
                    }
                    if (node.Attributes.Item(i).Name.CompareTo("bg_sound") == 0)
                    {
                        page.SetBackgroundNoise(node.Attributes.Item(i).Value);
                    }
                }
            }
            //get the paragraph nodes
            if (node.HasChildNodes)
            {
                foreach (XmlNode cNode in node.ChildNodes)
                {
                    if (cNode.Name.CompareTo("paragraph") == 0)
                    {
                        Paragraph paragraph = new Paragraph();
                        if (cNode.HasChildNodes)
                        {
                            foreach (XmlNode aNode in cNode.ChildNodes)
                            {
                                if (aNode.Name.CompareTo("actionText") == 0)
                                {
                                    ActionText actionText = new ActionText();
                                    if (aNode.HasChildNodes)
                                    {
                                        foreach (XmlNode annNode in aNode.ChildNodes)
                                        {
                                            Group group = new Group(annNode.InnerText);
                                            if (annNode.Name.CompareTo("annotate") == 0)
                                            {
                                                if (annNode.Attributes != null)
                                                {
                                                    for (int j = 0; j < annNode.Attributes.Count; j++)
                                                    {
                                                        group.AddAnnotation(annNode.Attributes.Item(j).Name,
                                                            annNode.Attributes.Item(j).Value);
                                                    }
                                                }
                                            }
                                            actionText.AddGroup(group);
                                        }
                                    }
                                    else
                                    {
                                        Group group = new Group(aNode.InnerText);
                                        actionText.AddGroup(group);
                                    }
                                    paragraph.AddActionText(actionText);
                                }


                            }
                        }
                        page.AddParagraph(paragraph);
                    }
                }
            }
            pages.Add(page);
        }


        public void accessNodeTree(XmlNode node){
            if (node.NodeType == XmlNodeType.Comment)
            {
                //return null if just a comment line
                return;
            }
            else if (node.Name.CompareTo("def_image") == 0)
            {
                 AccessImageDefinitionNode(node);
            }
            else if (node.Name.CompareTo("def_sound") == 0)
            {
                 AccessSoundDefinitionNode(node);
            }
            else if (node.Name.CompareTo("def_action") == 0)
            {
                 AccessActionDefinitionNode(node);
            }
            else if (node.Name.CompareTo("page") == 0)
            {
                 AccessPageDefinitionNode(node);
            }
        }
        public string GetStoryName()
        {
            return storyName;
        }
        public Dictionary<string,string> GetImages()
        {
            return images;
        }
        public Dictionary<string, string> GetSounds()
        {
            return sounds;
        }
        public Dictionary<string, StoryAction> GetStoryAction()
        {
            return actions;
        }
        public List<Page> GetSortedPages()
        {
            return pages.OrderBy(o => o.pageNumber).ToList();
        }
    }
}