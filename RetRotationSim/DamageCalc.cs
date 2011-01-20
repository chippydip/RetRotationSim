//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of DamageCalc.
    /// </summary>
    public abstract class DamageCalc
    {
        public DamageCalc (Simulator sim)
        {
            Contract.Requires(sim != null);
            
            Sim = sim;
        }
        
        protected Simulator Sim { get; private set; }
        
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intellect { get; set; }
        
        public int AttackPower { get; set; }
        public int SpellPower { get; set; }
        
        public double MeleeCritChance { get; set; }
        public double SpellCritChance { get; set; }
        public double AttackSpeed { get; set; }
        public double MeleeMissChance { get; set; }
        public double MeleeDodgeChance { get; set; }
        public double SpellMissChance { get; set; }
        
        public double Mastery { get; set; }
        
        public double PhysicalDamageBoost { get; set; }
        public double MagicalDamageBoost { get; set; }
        
        public double ArmorReduction { get; set; }
        
        public event Action<CombatLogEvent> OnCombatLogEvent = delegate { };
    }
}
