using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using System.Reflection;

namespace SharpLogix.Core
{
    /// <summary>
    /// A hellscape of generics.
    /// </summary>
    class ExperimentalSyntaxWalker : CSharpSyntaxWalker
    {
        public static readonly List<Type> LogixNodeTypes = new List<Type>();

        int RecursionLevel;
        int CurrentNamespace = -1;
        int CurrentScript = -1;
        readonly Slot SpawnRoot;
        readonly List<SharpLogixScript> sharpLogixScripts = new List<SharpLogixScript>();

        readonly SemanticModel semanticModel;

        static ExperimentalSyntaxWalker()
        {
            foreach (Type AvailableType in AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(assemblies => assemblies.GetTypes()).
                Where(currentType => currentType.IsSubclassOf(typeof(LogixNode))))
            {
                LogixNodeTypes.Add(AvailableType);
            }
        }

        public ExperimentalSyntaxWalker(Slot spawnRoot, string Name, SemanticModel model)
        {
            semanticModel = model;
            SpawnRoot = spawnRoot.AddSlot(Name);
        }

        public override void Visit(SyntaxNode node)
        {
            //string indents = new string('\t', Tabs);
            //Core.Debug.Log(indents + node.Kind());
            RecursionLevel++;
            base.Visit(node);
            RecursionLevel--;
        }

        //Assume one namespace for now.
        /*
        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            CurrentNamespace++;
            node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }
        */

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            //Class information. Eg myClass.
            sharpLogixScripts.Add(new SharpLogixScript(SpawnRoot));
            CurrentScript++;

            string identifier = node.Identifier.ValueText;

            sharpLogixScripts[CurrentScript].Root.Name = identifier;
            sharpLogixScripts[CurrentScript].Root.AttachComponent<DynamicVariableSpace>().SpaceName.Value = identifier;

            base.VisitClassDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            base.VisitFieldDeclaration(node);
        }
        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            foreach (VariableDeclaratorSyntax variable in node.Variables)
            {
                Slot VariableSlot = sharpLogixScripts[CurrentScript].Members.AddSlot(string.Concat(node.Type.ToString(), " ", variable.Identifier.ValueText));
                Type VariableType = Type.GetType(GetFullMetadataName(semanticModel.GetSymbolInfo(node.Type).Symbol));

                object Initializer = null;

                if (variable.Initializer != null)
                {
                    Initializer = Convert.ChangeType(variable.Initializer.ChildNodes().First().ToString(), VariableType);
                }               

                Type[] dynVarType = new Type[1] { VariableType };
                if (VariableType.IsClass && typeof(IWorldElement).IsAssignableFrom(VariableType))
                {
                    MethodInfo method = GetType().GetMethod(nameof(AddDRV), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Slot), typeof(string), typeof(object) }, null);
                    MethodInfo generic = method.MakeGenericMethod(dynVarType);
                    object[] parameters = new object[3] { VariableSlot, variable.Identifier.ValueText, Initializer };
                    generic.Invoke(this, parameters);
                }
                else
                {
                    MethodInfo method = GetType().GetMethod(nameof(AddDVV), BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Slot), typeof(string), typeof(object) }, null);
                    MethodInfo generic = method.MakeGenericMethod(dynVarType);
                    object[] parameters = new object[3] { VariableSlot, variable.Identifier.ValueText, Initializer };
                    generic.Invoke(this, parameters);
                }
            }

            base.VisitVariableDeclaration(node);
        }
        void AddDRV<T>(Slot slot, string name, object value) where T : class, IWorldElement
        {
            var drv = slot.AttachComponent<DynamicReferenceVariable<T>>();
            drv.VariableName.Value = name;

            if (value != null)
            {
                drv.DynamicValue = (T)value;
            }
        }
        void AddDVV<T>(Slot slot, string name, object value)
        {
            var drv = slot.AttachComponent<DynamicValueVariable<T>>();
            drv.VariableName.Value = name;

            if (value != null)
            {
                drv.DynamicValue = (T)value;
            }
        }

        //Magic
        static string GetFullMetadataName(ISymbol s)
        {
            if (s == null || IsRootNamespace(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                //sb.Insert(0, s.MetadataName);
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }
        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol s = null;
            return ((s = symbol as INamespaceSymbol) != null) && s.IsGlobalNamespace;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            sharpLogixScripts[CurrentScript].Methods.AddSlot(string.Concat(node.ReturnType.ToString(), " ", node.Identifier.ValueText, "()"));

            base.VisitMethodDeclaration(node);
        }
    }
}
