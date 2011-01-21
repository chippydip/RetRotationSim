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
    public sealed class Ability : SimulatorSpell
    {
        public Ability (Simulator sim, string name, 
                        Func<TimeSpan> cooldown = null,
                        Func<TimeSpan> gcd = null,
                        Func<bool> isUsable = null)
            : base(sim, name)
        {
            _cooldown = cooldown ?? (() => TimeSpan.Zero);
            _gcd = gcd ?? (() => TimeSpan.FromSeconds(1.5));
            _isUsable = isUsable ?? (() => true);
        }
        
        public TimeSpan Ready { get; private set; }
        
        private readonly Func<TimeSpan> _cooldown;
        public TimeSpan Cooldown { get { return _cooldown(); } }
        
        private readonly Func<TimeSpan> _gcd;
        public TimeSpan Gcd { get { return _gcd(); } }
        
        private readonly Func<bool> _isUsable;
        public bool IsUsable { get { return _isUsable(); } }
        
        public void Reset ()
        {
            if (Sim.IsRunning)
                return;
            
            Ready = TimeSpan.Zero;
        }
        
        public bool Cast ()
        {
            if (Sim.IsGcd || Ready > Sim.Time || !IsUsable)
                return false;
            
            Ready = Sim.Time + Cooldown;
            RaiseCast();
            
            // Schedule a dummy event for when the cooldown finishes
            if (Ready > Sim.Time)
                Sim.AddEvent(Ready, AbilityReady);
            
            RaiseAfterCast();
            
            return true;
        }
        
        private void AbilityReady ()
        {
            // empty
        }
    }
}
