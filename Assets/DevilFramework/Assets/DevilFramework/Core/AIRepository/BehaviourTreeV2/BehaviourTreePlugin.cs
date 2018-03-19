using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTPluginNode
    {

        string Name { get; }

        BTNodeBase CreateNodeInstance();
    }

    public class BTRandomNode : IBTPluginNode
    {
        public string Name { get { return "RANDOM"; } }

        public BTNodeBase CreateNodeInstance()
        {
            return null;
        }
    }

    public class BehaviourTreePlugin : Singleton<BehaviourTreePlugin>
    {
        Dictionary<string, IBTPluginNode> mPlugins;

        protected override void OnInit()
        {
            mPlugins = new Dictionary<string, IBTPluginNode>();
            IBTPluginNode plugin;
            plugin = new BTRandomNode();
            mPlugins[plugin.Name] = plugin;
        }

        public IBTPluginNode GetPluginNode(string name)
        {
            IBTPluginNode node;
            if (mPlugins.TryGetValue(name, out node))
                return node;
            else
                return null;
        }

        public void GetPluginNames(ICollection<string> names)
        {
            foreach(string na in mPlugins.Keys)
            {
                names.Add(na);
            }
        }
    }
}
