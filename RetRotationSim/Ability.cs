//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Ability.
    /// </summary>
    public class Ability
    {
        public Ability (Simulator sim, string name, 
                        Func<TimeSpan> cooldown = null,
                        Func<TimeSpan> gcd = null,
                        Func<bool> isUsable = null)
        {
            Contract.Requires(sim != null);
            Contract.Requires(name != null);
            
            Sim = sim;
            Name = name;
            
            _cooldown = cooldown ?? (() => TimeSpan.Zero);
            _gcd = gcd ?? (() => TimeSpan.FromSeconds(1.5));
            _isUsable = isUsable ?? (() => true);
        }
        
        private Simulator Sim { get; set; }
        
        public string Name { get; private set; }
        
        public TimeSpan Ready { get; private set; }
        
        private readonly Func<TimeSpan> _cooldown;
        public TimeSpan Cooldown { get { return _cooldown(); } }
        
        private readonly Func<TimeSpan> _gcd;
        public TimeSpan Gcd { get { return _gcd(); } }
        
        private readonly Func<bool> _isUsable;
        public bool IsUsable { get { return _isUsable(); } }
        
        public event Action<Ability> OnCast = delegate { };
        
        public bool Cast ()
        {
            if (Sim.IsGcd || Ready > Sim.Time || !IsUsable)
                return false;
            
            Ready = Sim.Time + Cooldown;
            OnCast(this);
            
            // Schedule a dummy event for when the cooldown finishes
            if (Ready > Sim.Time)
                Sim.AddEvent(Ready, AbilityReady);
            
            return true;
        }
        
        private void AbilityReady ()
        {
            // 
        }
    }
}
