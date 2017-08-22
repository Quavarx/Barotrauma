﻿using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
    class EventManager
    {
        const float CriticalPriority = 50.0f;

        private List<ScriptedEvent> events;

        public List<ScriptedEvent> Events
        {
            get { return events; }
        }
        
        public EventManager(GameSession session)
        {
            events = new List<ScriptedEvent>();        
        }
        
        public void StartShift(Level level)
        {
            CreateScriptedEvents(level);
            foreach (ScriptedEvent ev in events)
            {
                ev.Init();
            }
        }

        public void EndShift()
        {
            events.Clear();
        }

        private void CreateScriptedEvents(Level level)
        {
            MTRandom rand = new MTRandom(ToolBox.StringToInt(level.Seed));
            events.AddRange(ScriptedEvent.GenerateLevelEvents(rand, level));
        }
        
        public void Update(float deltaTime)
        {
            events.RemoveAll(t => t.IsFinished);
            foreach (ScriptedEvent ev in events)
            {
                if (!ev.IsFinished)
                {
                    ev.Update(deltaTime);
                }
            }
        }
    }
}
