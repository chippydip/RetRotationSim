//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;

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
            
            Random = new Random();
            
            Handlers = new Dictionary<string, Action<Spell>>();
            
            foreach (var buff in Sim.Buffs)
            {
                buff.OnCast += (b) => CallHandler("B:" + b.Name, b);
                buff.OnTick += (b) => CallHandler("T:" + b.Name, b);
            }
            
            foreach (var abil in Sim.Abilities)
                abil.OnCast += (a) => CallHandler(a.Name, a);
            
            Sim.MainHand.OnSwing += () => CallHandler("MH:Melee", null);
        }
        
        public Simulator Sim { get; private set; }
        
        public Random Random { get; set; }
        
        public double MainHandSpeed { get; set; }
        public int MainHandMin { get; set; }
        public int MainHandMax { get; set; }
        
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
        
        protected void RaiseCombatLogEvent (CombatLogEvent evt)
        {
            OnCombatLogEvent(evt);
        }
        
        protected Dictionary<string, Action<Spell>> Handlers { get; private set; }
        
        private void CallHandler (string key, Spell spell)
        {
            Action<Spell> handler;
            if (Handlers.TryGetValue(key, out handler))
                handler(spell);
            else
                OnCombatLogEvent(new CombatLogEvent { Time = Sim.Time, Spell = spell });
        }
        
        protected T SwitchOnHp<T> (T value3, T value2, T value1, T value0 = default(T))
        {
            if (Sim.EffectiveHolyPower != 3)
                Console.WriteLine("Used HP @ {0}", Sim.EffectiveHolyPower);
            
            switch (Sim.EffectiveHolyPower)
            {
                case 3:
                    return value3;
                case 2:
                    return value2;
                case 1:
                    return value1;
                default:
                    return value0;
            }
        }
        
        protected int AdjustPhysicalAttack (int value)
        {
            return (int)(value * (1 + PhysicalDamageBoost) * (1 - ArmorReduction));
        }
        
        protected int AdjustMagicalAttack (int value)
        {
            return (int)(value * (1 + MagicalDamageBoost)); // TODO resistances?
        }
        
        protected void CombatTableSingleRoll (CombatLogEvent evt,
                                              double critMultiplier = 2,
                                              double missBoost = 0,
                                              double dodgeBoost = 0,
                                              double critBoost = 0)
        {
            double roll = Random.NextDouble();
            
            if (roll < missBoost + MeleeMissChance)
            {
                // miss
                evt.DamageAmount = 0;
                evt.DefenseType = DefenseType.Miss;
                return;
            }
            
            roll -= Math.Max(0, missBoost + MeleeMissChance);
            if (roll < dodgeBoost + MeleeDodgeChance)
            {
                // dodge
                evt.DamageAmount = 0;
                evt.DefenseType = DefenseType.Dodge;
                return;
            }
            
            roll -= Math.Max(0, dodgeBoost + MeleeDodgeChance);
            // TODO Parry?
            if (roll < 0.2) // Glancing Blow chance on +3 mob
            {
                // glancing blow
                evt.DamageAmount = (int)(evt.DamageAmount * 0.75);
                evt.AttackType = AttackType.GlancingBlow;
                return;
            }
            
            roll -= 0.2; // Glancing Blow chance on +3 mob
            // TODO Block?
            if (roll < critBoost + MeleeCritChance)
            {
                // crit
                evt.DamageAmount = (int)(evt.DamageAmount * critMultiplier);
                evt.AttackType = AttackType.Crit;
                return;
            }
            
            roll -= Math.Max(0, critBoost + MeleeCritChance);
            // TODO CrushingBlow?
            
            // ordinary hit (do nothing)
        }
        
        protected void CombatTableDoubleRoll (CombatLogEvent evt,
                                              double critMultiplier = 2,
                                              double missBoost = 0,
                                              double dodgeBoost = 0,
                                              double critBoost = 0)
        {
            // bonus damage roll (crit/crush)
            CombatTableDoubleRoll1(evt, critMultiplier, critBoost);
            
            // target defense roll (miss/dodge/parry/block/glancing)
            CombatTableDoubleRoll2(evt, missBoost, dodgeBoost);
        }
        
        protected void CombatTableSpellRoll (CombatLogEvent evt,
                                             double critMultiplier = 1.5,
                                             double missBoost = 0,
                                             double critBoost = 0)
        {
            if (Random.NextDouble() < missBoost + SpellMissChance)
            {
                // miss
                evt.DamageAmount = 0;
                evt.DefenseType = DefenseType.Miss;
            }
            
            if (Random.NextDouble() < critBoost + MeleeCritChance)
            {
                // crit
                evt.DamageAmount = (int)(evt.DamageAmount * critMultiplier);
                evt.AttackType = AttackType.Crit;
            }
            
            // TODO resists?
        }
        
        private void CombatTableDoubleRoll1 (CombatLogEvent evt, double critMultiplier, double critBoost)
        {
            double roll = Random.NextDouble(); // bonus damage roll
            
            if (roll < critBoost + MeleeCritChance)
            {
                // crit
                evt.DamageAmount = (int)(evt.DamageAmount * critMultiplier);
                evt.AttackType = AttackType.Crit;
                return;
            }
            
            roll -= Math.Max(0, critBoost + MeleeCritChance);
            // TODO CrushingBlow?
            
            // ordinary hit (do nothing)
        }
        
        private void CombatTableDoubleRoll2 (CombatLogEvent evt, double missBoost, double dodgeBoost)
        {
            double roll = Random.NextDouble(); // target defense roll
            
            if (roll < missBoost + MeleeMissChance)
            {
                // miss
                evt.DamageAmount = 0;
                evt.DefenseType = DefenseType.Miss;
                return;
            }
            
            roll -= Math.Max(0, missBoost + MeleeMissChance);
            if (roll < dodgeBoost + MeleeDodgeChance)
            {
                // dodge
                evt.DamageAmount = 0;
                evt.DefenseType = DefenseType.Dodge;
                return;
            }
            
            roll -= Math.Max(0, dodgeBoost + MeleeDodgeChance);
            // TODO Parry, GlancingBlow, Block?
            
            // ordinary hit (do nothing)
        }
    }
}
