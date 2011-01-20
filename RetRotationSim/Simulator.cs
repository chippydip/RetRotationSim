//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;

using RetRotationSim.Collections;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Simulator.
    /// </summary>
    public abstract class Simulator
    {
        public sealed class Event : IComparable<Event>
        {
            public TimeSpan Time { get; private set; }
            public Action Action { get; private set; }
            
            public Event (TimeSpan time, Action action)
            {
                Contract.Requires(time >= TimeSpan.Zero);
                Contract.Requires(action != null);
                
                Time = time;
                Action = action;
            }
            
            public int CompareTo (Event other)
            {
                // Sort events from highest to lowest (heap returns last to first)
                return other.Time.CompareTo(Time);
            }
        }
        
        private readonly IPriorityQueue<Event> _events = new Heap<Event>();
        
        public Simulator ()
        {
            WeaponSpeed = 3.6 / 1.09 / 1.1;
            SpellHaste = 1.09 * 1.05;
            Mastery = 0.08;
            
            Random = new Random();
        }
        
        // User settings
        public double WeaponSpeed { get; set; }
        public double SpellHaste { get; set; }
        public double Mastery { get; set; }
        public bool Has4pT11 { get; set; }
        public bool Has4pPvp { get; set; }
        public bool HasConsecrationGlyph { get; set; }
        
        public event Action<Simulator> OnEvent = delegate { };
        
        public Random Random { get; set; }
        
        // Informational properties
        public int HolyPower { get; protected set; }
        
        public TimeSpan Time { get; protected set; }
        public TimeSpan GcdDone { get; protected set; }
        public bool IsGcd { get { return GcdDone > Time; } }
        
        public AutoAttack MainHand { get; protected set; }
        
        public TimeSpan TotalRuntime { get; protected set; }
        
        // Methods
        public void AddEvent (TimeSpan time, Action action)
        {
            Contract.Requires(time >= Time);
            Contract.Requires(action != null);
            
            _events.Push(new Event(time, action));
        }
        
        public void Run (TimeSpan fightDuration)
        {
            Time = TimeSpan.Zero;
            
            Init();
            
            while (_events.Count > 0)
            {
                Event next = _events.Pop();
                
                if (next.Time >= fightDuration)
                    break;
                
                Time = next.Time;
                
                next.Action();
                
                OnEvent(this);
            }
            
            Time = fightDuration;
            
            TotalRuntime += fightDuration;
        }
        
        protected virtual void Init ()
        {
            MainHand.Start();
        }
        
        public void Reset ()
        {
            TotalRuntime = TimeSpan.Zero;
            
            _buffImpl.Clear();
            _buff.Clear();
            _ability.Clear();
            
            _events.Clear();
            Time = TimeSpan.Zero;
        }
        
        private readonly Dictionary<string, BuffImpl> _buffImpl = new Dictionary<string, BuffImpl>();
        private readonly Dictionary<string, Buff> _buff = new Dictionary<string, Buff>();
        private readonly Dictionary<string, Ability> _ability = new Dictionary<string, Ability>();
        
        public IEnumerable<Buff> Buffs { get { return _buff.Values; } }
        public IEnumerable<Ability> Abilities { get { return _ability.Values; } }
        
        protected BuffImpl BuffImpl (string name)
        {
            Contract.Requires(name != null);
            
            return _buffImpl[name];
        }
        
        public Buff Buff (string name)
        {
            Contract.Requires(name != null);
            
            return _buff[name];
        }
        
        public Ability Ability (string name)
        {
            Contract.Requires(name != null);
            
            return _ability[name];
        }
        
        protected void AddBuff (BuffImpl buffImpl)
        {
            _buffImpl[buffImpl.Name] = buffImpl;
            _buff[buffImpl.Name] = buffImpl.Buff;
        }
        
        protected void AddAbility (Ability abil)
        {
            _ability[abil.Name] = abil;
            abil.OnCast += (ability) =>
            {
                GcdDone = Time + ability.Gcd;
                if (GcdDone > Time)
                    AddEvent(GcdDone, delegate { });
            };
        }
        
        public virtual int EffectiveHolyPower { get { return Math.Min(3, HolyPower); } }
        
        public bool IsHpAbilityUsable { get { return EffectiveHolyPower > 0; } }
        
        public bool HasMaxHolyPower { get { return EffectiveHolyPower >= 3; } }
        
        public bool Cast (string name)
        {
            Contract.Requires(name != null);
            Contract.Requires(Ability(name) != null);
            
            return Ability(name).Cast();
        }
        
        protected TimeSpan SpellGcd { get { return TimeSpan.FromSeconds(1.5 / SpellHaste); } }
    }
}
