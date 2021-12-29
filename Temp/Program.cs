using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpLogix.Core
{
    class NodeDefinition
    {
        protected string typename;
        protected string[] inputs;
        protected string[] outputs;
        protected string default_output_name;
    }

    class BinaryOperationNode : NodeDefinition
    {
        public BinaryOperationNode(string name)
        {
            typename = name;
            inputs = new string[] { "A", "B" };
            outputs = new string[] { "*" };
            default_output_name = "*";
        }
    }

    struct NodeInfo
    {
        public string className;
        public string defaultOutput;
        public string[] inputs;
        public string[] outputs;

        public static NodeInfo Define(
            string cName,
            string output,
            string[] nodeInputs,
            string[] nodeOutputs)
        {
            return new NodeInfo
            {
                className = cName,
                defaultOutput = output,
                inputs = nodeInputs,
                outputs = nodeOutputs
            };
        }

        public static NodeInfo Define(string cName)
        {
            return Define(cName, "*", new string[0], new string[] { "*" });
        }
    }

    class NodeDB : Dictionary<string, NodeInfo>
    {
        public NodeInfo AddInputDefinition(string name)
        {
            string[] inputs = { };
            string[] outputs = { "*" };
            return AddDefinition(name, "*", inputs, outputs);
        }

        public NodeInfo AddBinaryOperationDefinition(string name)
        {
            string[] inputs = { "A", "B" };
            string[] outputs = { "*" };
            return AddDefinition(name, "*", inputs, outputs);
        }

        public NodeInfo AddDefinition(
            string name,
            string defaultOutputName,
            string[] inputs,
            string[] outputs)
        {
            string completeName = "FrooxEngine.LogiX." + name;
            NodeInfo nodeInfo = NodeInfo.Define(
                completeName, defaultOutputName, inputs, outputs);
            this.Add(completeName, nodeInfo);
            return nodeInfo;
        }
        public NodeInfo GetNodeInformation(string name)
        {
            return this[name];
        }

        public string DefaultOutputFor(string name)
        {
            return this[name].defaultOutput;
        }
    }

    struct Node
    {
        public string typename;
        public string GenericTypeName()
        {
            return typename.Split(new char[] { '<' })[0];
        }
    }
    struct NodeRef
    {
        public int nodeId;
        public NodeRef(int id)
        {
            nodeId = id;
        }
    }
    struct NodeRefGroup
    {
        List<NodeRef> nodes;
    }
    class ActiveElement
    {
        NodeRef node;
        string defaultOutputName;
        string selectedOutput;
    };

    // NumericLiteral - Int -> IntInput. 0 Inputs. 1 Output. DefaultOutputName = *
    // StringLiteral  - String -> StringInput. 0 Inputs. 1 Output. DefaultOutputName = *
    // Operator + -> Add. 2 Inputs. 1 Output. DefaultOutputName = *
    //
    // AssignmentOperation - NodeGroup. 0 Inputs. 1 Output (NodeGroup ID). DefaultOutputName = ??

    class OperationNodes : List<int> { }

    class LogixMethodParameter
    {
        public string name;
        public string type; // FIXME : Infer this from the representing node ?
        public NodeRef node;

        public LogixMethodParameter(string paramName, string paramType, int nodeID)
        {
            name = paramName;
            type = paramType;
            node = new NodeRef(nodeID);
        }
    }

    class LogixMethod
    {
        public string name;
        public List<LogixMethodParameter> parameters;
        public string returnType;
        public int slotID;

        public bool ReturnValue()
        {
            return returnType != "void";
        }

        public void SetReturnType(string typeName)
        {
            returnType = typeName;
        }

        static readonly LogixMethodParameter invalidParam = new LogixMethodParameter("", "", -1);

        public LogixMethod(string methodName, int newSlotID)
        {
            name = methodName;
            slotID = newSlotID;
            parameters = new List<LogixMethodParameter>(4);
        }

        public void AddParameter(string name, string type, int nodeID)
        {
            parameters.Add(new LogixMethodParameter(name, type, nodeID));
        }

        public LogixMethodParameter GetParameter(string name)
        {
            foreach (LogixMethodParameter methodParam in parameters)
            {
                if (methodParam.name == name) return methodParam;
            }
            return invalidParam;
        }
    }

    class Nodes : List<Node>
    {
        public readonly static Node invalidNode = new Node();

        public Node GetNode(int nodeID)
        {
           if (nodeID >= this.Count)
            {
                return invalidNode;
            }
            return this[nodeID];
        }
    }

    struct LogixSlot
    {
        public string name;

        public LogixSlot(string slotName)
        {
            name = slotName;
        }
    }

    class Slots : List<LogixSlot>
    {
        public int AddSlot(string name)
        {
            int slotID = Count;
            Add(new LogixSlot(name));
            return slotID;
        }

        public LogixSlot GetSlot(int slotID)
        {
            return this[slotID];
        }
    }
}
