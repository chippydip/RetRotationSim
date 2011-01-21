//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Spell.
    /// </summary>
    public class Spell
    {
        public Spell (string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            
            Name = name;
        }
        
        public Spell (Spell toCopy)
        {
            Name = toCopy.Name;
        }
        
        public string Name { get; private set; }
        //public string Rank { get; private set; }
        //public int Cost { get; private set; }
        //public bool IsChanneled { get; private set; }
        //public PowerType PowerType { get; private set; }
        //public TimeSpan CastTime { get; private set; }
        //public int MinRange { get; private set; }
        //public int MaxRange { get; private set; }
        
        public event Action<Spell> OnCast = delegate { };
        public event Action<Spell> AfterCast = delegate { };
        
        public event Action<Spell> OnTick = delegate { };
        
        protected void RaiseCast ()
        {
            OnCast(this);
        }
        
        protected void RaiseAfterCast ()
        {
            AfterCast(this);
        }
        
        protected void RaiseTick ()
        {
            OnTick(this);
        }
    }
    
    public enum PowerType
    {
        Health = -2,
        Mana = 0,
        Rage = 1,
        Focus = 2,
        Energy = 3,
        Happiness = 4,
        Rune = 5,
        RunicPower = 6,
        HolyPower = 7,
    }
    
    public abstract class SimulatorSpell : Spell
    {
        public SimulatorSpell (Simulator sim, string name)
            : base(name)
        {
            Contract.Requires(sim != null);
            Contract.Requires(!string.IsNullOrEmpty(name));
            
            Sim = sim;
        }
        
        public SimulatorSpell (SimulatorSpell toCopy)
            : base(toCopy)
        {
            Sim = toCopy.Sim;
        }
        
        protected Simulator Sim { get; private set; }
    }
}
