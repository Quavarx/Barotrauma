﻿using System;
using System.Reflection;
using System.Xml.Linq;

namespace Barotrauma
{
    class EventPrefab
    {
        public readonly XElement ConfigElement;    
        public readonly Type EventType;
        public readonly float Probability;
        public readonly bool TriggerEventCooldown;
        public float Commonness;
        public string Identifier;
        public string BiomeIdentifier;
        public float SpawnDistance;

        public bool UnlockPathEvent;
        public string UnlockPathTooltip;
        public int UnlockPathReputation;
        public string UnlockPathFaction;

        public EventPrefab(XElement element)
        {
            ConfigElement = element;
         
            try
            {
                EventType = Type.GetType("Barotrauma." + ConfigElement.Name, true, true);
                if (EventType == null)
                {
                    DebugConsole.ThrowError("Could not find an event class of the type \"" + ConfigElement.Name + "\".");
                }
            }
            catch
            {
                DebugConsole.ThrowError("Could not find an event class of the type \"" + ConfigElement.Name + "\".");
            }

            Identifier = ConfigElement.GetAttributeString("identifier", string.Empty);
            BiomeIdentifier = ConfigElement.GetAttributeString("biome", string.Empty);
            Commonness = element.GetAttributeFloat("commonness", 1.0f);
            Probability = Math.Clamp(element.GetAttributeFloat(1.0f, "probability", "spawnprobability"), 0, 1);
            TriggerEventCooldown = element.GetAttributeBool("triggereventcooldown", EventType != typeof(ScriptedEvent));

            UnlockPathEvent = element.GetAttributeBool("unlockpathevent", false);
            UnlockPathTooltip = element.GetAttributeString("unlockpathtooltip", "lockedpathtooltip");
            UnlockPathReputation = element.GetAttributeInt("unlockpathreputation", 0);
            UnlockPathFaction = element.GetAttributeString("unlockpathfaction", "");

            SpawnDistance = element.GetAttributeFloat("spawndistance", 0);
        }

        public bool TryCreateInstance<T>(out T instance) where T : Event
        {
            instance = CreateInstance() as T;
            return instance is T;
        }

        public Event CreateInstance()
        {
            ConstructorInfo constructor = EventType.GetConstructor(new[] { typeof(EventPrefab) });
            Event instance = null;
            try
            {
                instance = constructor.Invoke(new object[] { this }) as Event;
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError(ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString());
            }
            if (instance != null && !instance.LevelMeetsRequirements()) { return null; }
            return instance;
        }

        public override string ToString()
        {
            return $"EventPrefab ({Identifier})";
        }
    }
}