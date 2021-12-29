using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NeosModLoader;
using HarmonyLib;
using BaseX;
using FrooxEngine;

namespace SharpLogix.Core
{
    class SharpLogixCompiler : NeosMod
    {
        public override string Name => "SharpLogix";
        //Originally forked from: https://github.com/vr-voyage/SharpLogix and refactored into a nml mod.
        public override string Author => "Toxic_Cookie";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/Toxic-Cookie/SharpLogix";
        public override void OnEngineInit()
        {
            Core.Debug.Log("Hello World!");

            Harmony harmony = new Harmony("net.Toxic_Cookie.SharpLogix");
            harmony.PatchAll();
        }
        
        [HarmonyPatch(typeof(ImageImportDialog), "OnAttach")]
        class ImportScript
        {
            public static void Postfix()
            {
                string MyProgram = @"
                namespace MyProgram
                {
                    class Class1
	                {
		                int TestIntA;
		                int TestIntB = 3;
		                int TestIntC;

		                void Start()
		                {
			                TestIntA = 2;

			                TestIntC = TestIntA + TestIntB;
		                }
	                }
                }
                ";
                
                new SharpLogixProgram(MyProgram, "Hello World");
            }
        }
    }
}
