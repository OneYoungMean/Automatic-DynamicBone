using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    [CustomEditor(typeof(ADBRuntimePoint))]
    public class ADBRunrimePointEditor : Editor
    {
        public ADBChainProcessorEditor rootEditor;
        public ADBRuntimePoint controller;
        public void OnEnable()
        {
            controller = target as ADBRuntimePoint;
            var root = controller;
            while (root.Parent != null)
            {
                root = root.Parent;
            }
            rootEditor = Editor.CreateEditor(root as ADBChainProcessor) as ADBChainProcessorEditor;
        }

        public override void OnInspectorGUI() { }
    }


}
