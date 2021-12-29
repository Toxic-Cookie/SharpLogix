using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;

namespace SharpLogix.Core
{
    /// <summary>
    /// A Neos representation of a class.
    /// </summary>
    class SharpLogixScript
    {
        public readonly Slot SpawnSlot;
        public readonly Slot Root;
        public readonly Slot Assembly;
        public readonly Slot Members;
        public readonly Slot Methods;
        public readonly Slot Debug;

        public SharpLogixScript(Slot spawnRoot, string Name = "Unnamed Script")
        {
            SpawnSlot = spawnRoot;
            Root = SpawnSlot.AddSlot(Name);
            Assembly = Root.AddSlot("Assembly");
            Members = Assembly.AddSlot("Members");
            Methods = Assembly.AddSlot("Methods");
            Debug = Assembly.AddSlot("Debug");
        }
    }
}
