using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

namespace Assets.Editor
{
    internal sealed class CsprojFix : AssetPostprocessor
    {
        // ReSharper disable once InconsistentNaming
        private static string OnGeneratedCSProject(string path, string content)
        {
            var document = XDocument.Parse(content);
            var root = document.Root;
            Debug.Assert(root != null, nameof(root) + " != null");
            root.Descendants()
                .Where(x => x.Name.LocalName == "Reference")
                .Where(x =>
                {
                    var attribute = (string)x.Attribute("Include");
                    return attribute == "Boo.Lang" || attribute == "UnityScript.Lang" || attribute == "UnityScript";
                })
                .Remove();
            return document.Declaration + Environment.NewLine + document.Root;
        }
    }
}