﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
    class TalentPrefab : IPrefab, IDisposable, IHasUintIdentifier
    {
        public string Identifier { get; private set; }
        public string OriginalName => Identifier;
        public ContentPackage ContentPackage { get; private set; }
        public string FilePath { get; private set; }

        public string DisplayName { get; private set; }

        public string Description { get; private set; }

        public readonly Sprite Icon;

        public static readonly PrefabCollection<TalentPrefab> TalentPrefabs = new PrefabCollection<TalentPrefab>();

        public XElement ConfigElement
        {
            get;
            private set;
        }

        public TalentPrefab(XElement element, string filePath)
        {
            FilePath = filePath;
            ConfigElement = element;
            Identifier = element.GetAttributeString("identifier", "noidentifier");
            DisplayName = TextManager.Get("talentname." + Identifier, returnNull: true) ?? Identifier;

            foreach (XElement subElement in element.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "icon":
                        Icon = new Sprite(subElement);
                        break;
                    case "description":
                        string tempDescription = Description;
                        TextManager.ConstructDescription(ref tempDescription, subElement);
                        Description = tempDescription;
                        break;
                }
            }

            if (string.IsNullOrEmpty(Description))
            {
                if (element.Attribute("description") != null)
                {
                    string description = element.GetAttributeString("description", string.Empty);
                    Description = TextManager.Get(description, returnNull: true) ?? description;
                }
                else
                {
                    Description = TextManager.Get("talentdescription." + Identifier, returnNull: true) ?? string.Empty;
                }
            }

#if DEBUG
            if (!TextManager.ContainsTag("talentname." + Identifier))
            {
                DebugConsole.AddWarning($"Name for the talent \"{Identifier}\" not found in the text files.");
            }
            if (string.IsNullOrEmpty(Description))
            {
                DebugConsole.AddWarning($"Description for the talent \"{Identifier}\" not configured");
            }
            if (Description.Contains('['))
            {
                DebugConsole.ThrowError($"Description for the talent \"{Identifier}\" contains brackets - was some variable not replaced correctly? ({Description})");
            }
#endif
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            TalentPrefabs.Remove(this);
        }

        /// <summary>
        /// Unique identifier that's generated by hashing the prefab's string identifier. 
        /// Used to reduce the amount of bytes needed to write talent data into network messages in multiplayer.
        /// </summary>
        public uint UIntIdentifier { get; set; }

        public static void RemoveByFile(string filePath) => TalentPrefabs.RemoveByFile(filePath);

        public static void LoadFromFile(ContentFile file)
        {
            DebugConsole.Log("Loading talent prefab: " + file.Path);
            RemoveByFile(file.Path);

            XDocument doc = XMLExtensions.TryLoadXml(file.Path);
            if (doc == null) { return; }

            void loadSinglePrefab(XElement element, bool isOverride)
            {
                var newPrefab = new TalentPrefab(element, file.Path) { ContentPackage = file.ContentPackage };
                TalentPrefabs.Add(newPrefab, isOverride);
                newPrefab.CalculatePrefabUIntIdentifier(TalentPrefabs);
            }

            void loadMultiplePrefabs(XElement element, bool isOverride)
            {
                foreach (var subElement in element.Elements())
                {
                    interpretElement(subElement, isOverride);
                }
            }

            void interpretElement(XElement subElement, bool isOverride)
            {
                if (subElement.IsOverride())
                {
                    loadMultiplePrefabs(subElement, true);
                }
                else if (subElement.Name.LocalName.Equals("talents", StringComparison.OrdinalIgnoreCase))
                {
                    loadMultiplePrefabs(subElement, isOverride);
                }
                else if (subElement.Name.LocalName.Equals("talent", StringComparison.OrdinalIgnoreCase))
                {
                    loadSinglePrefab(subElement, isOverride);
                }
                else
                {
                    DebugConsole.ThrowError($"Invalid XML element for the {nameof(TalentPrefab)} prefab type: '{subElement.Name}' in {file.Path}");
                }
            }

            interpretElement(doc.Root, false);
        }

        public static void LoadAll(IEnumerable<ContentFile> files)
        {
            DebugConsole.Log("Loading talent prefabs: ");

            foreach (ContentFile file in files)
            {
                LoadFromFile(file);
            }
        }
    }
}