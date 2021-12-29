using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BaseX;
using FrooxEngine;

namespace SharpLogix.Core
{
    class SharpLogixProgram
    {
        public SharpLogixProgram(string Program, string Name = "Unnamed Program", Slot spawnRoot = null)
        {
            if (spawnRoot == null)
            {
                spawnRoot = Engine.Current.WorldManager.FocusedWorld.RootSlot;
            }
            //slx = SharpLogix
            if (!Name.EndsWith(".slx"))
            {
                Name = string.Concat(Name, ".slx");
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(Program);

            //Might require end user to reference their libs. A given but oof.
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("slxCompilation", syntaxTrees: new[] { tree }, references: new[] { mscorlib });

            ExperimentalSyntaxWalker walker = new ExperimentalSyntaxWalker(spawnRoot, Name, compilation.GetSemanticModel(tree));
            walker.Visit(tree.GetRoot());
        }
    }
}
