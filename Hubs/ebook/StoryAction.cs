using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Microsoft.AspNet.SignalR;
using eDocumentReader.Hubs.activities.system.lightweight;

namespace eDocumentReader.Hubs
{

    public class StoryAction : Hub
    {
        private string name;
        private List<string> soundAction;
        private List<string> animationAction;
        private List<string> backgroundAction;

        public StoryAction(string name)
        {
            this.name = name;
            soundAction = new List<string>();
            animationAction = new List<string>();
            backgroundAction = new List<string>();
        }
        public string getName()
        {
            return name;
        }
        public void addAnimationAction(string action)
        {
            animationAction.Add(action);
        }
        public void addSoundAction(string sound)
        {
            soundAction.Add(sound);
        }
        public void addBackgroundAction(string bg)
        {
            backgroundAction.Add(bg);
        }
        public void start(Dictionary<string, string> images, Dictionary<string, string> sounds)
        {
            playSound(sounds);
            displayBackgroundImage(images);
            playAnimationOnly();
        }
        public void playSound(Dictionary<string, string> sounds)
        {
            foreach (Object sound in soundAction)
            {
                //TODO: 
            }
        }
        public void displayBackgroundImage(Dictionary<string, string> images)
        {
            foreach (Object bg in backgroundAction)
            {
                string path;
                images.TryGetValue(Convert.ToString(bg),out path);
                ActivityExecutor.add(new InternalChangeBackgroundActivity(path));
            }
        }
        public void playAnimationOnly()
        {
            foreach (Object animation in animationAction)
            {
                ActivityExecutor.add(new InternalPlayAnimationActivity(Convert.ToInt32(animation)));
            }
        }

        public override string ToString()
        {
            string ret = name + "->";
            foreach (string ea in soundAction)
            {
                ret += "soundAction="+ea + ";";
            }
            foreach (string ea in animationAction)
            {
                ret += "animationAction="+ea + ";";
            }
            foreach (string ea in backgroundAction)
            {
                ret += "bgroundAction="+ea + ";";
            }
            return ret;
        }
    }
}