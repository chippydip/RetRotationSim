//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;

namespace RetRotationSim
{
    /// <summary>
    /// Description of CombatLogEvent.
    /// </summary>
    public class CombatLogEvent
    {
        public CombatLogEvent ()
        {
        }
        
        public TimeSpan Time { get; set; }
        //public CombatLogEventType Type { get; set; }
        //public Unit Source { get; set; }
        //public Unit Target { get; set; }
        
        public Ability Spell { get; set; }
        
        public int DamageAmount { get; set; }
        //public int OverkillAmount { get; set; }
        public DamageType DamageType { get; set; }
        public AttackType AttackType { get; set; }
        public DefenseType DefenseType { get; set; }
        //public int DamageAvoided { get; set; }
    }
    
    [Flags]
    public enum DamageType
    {
        Physical = 1,
        Holy = 2,
        Fire = 4,
        Nature = 8,
        Frost = 16,
        Shadow = 32,
        Arcane = 64,
    }
    
    public enum AttackType
    {
        Hit,
        Crit,
        GlancingBlow,
        CrushingBlow,
    }
    
    public enum DefenseType
    {
        None,
        Dodge,
        Parry,
        Block,
        Miss,
        Resist,
        Absorb,
        Deflect,
        Evade,
        Immune,
        Reflect,
    }
}
