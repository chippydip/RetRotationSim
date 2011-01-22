//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;

namespace RetRotationSim
{
    /// <summary>
    /// Description of DamageCalc406.
    /// </summary>
    public sealed class DamageCalc406 : DamageCalc
    {
        public DamageCalc406 (Simulator sim)
            : base(sim)
        {
            Handlers["MH:Melee"] = (_) =>
            {
                var evt = WeaponDamageAttack(MainHandSpeed);
                ScaleDamage(evt, 1.2); // 2H spec
                CombatTableSingleRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["Crusader Strike"] = (spell) =>
            {
                var evt = WeaponDamageAttack(3.3, 1.35);
                evt.Spell = spell;
                ScaleDamage(evt, 1.2); // 2H spec
                ScaleDamage(evt, 1.3); // Crusade
                CombatTableDoubleRoll(evt, critBoost:0.15);
                RaiseCombatLogEvent(evt);
                HandOfLightDamage(evt);
            };
            
            Handlers["Templar's Verdict"] = (spell) =>
            {
                var evt = WeaponDamageAttack(MainHandSpeed, SwitchOnHp(2.35, 0.9, 0.3)); // TODO not normalized?
                evt.Spell = spell;
                ScaleDamage(evt, 1.2); // 2H spec
                ScaleDamage(evt, 1 + 0.3 + 0.15 + 0.1); // Crusade + Glyph + 2pT11
                CombatTableDoubleRoll(evt, critBoost:0.12);
                RaiseCombatLogEvent(evt);
                HandOfLightDamage(evt);
            };
            
            Handlers["Hammer of Wrath"] = (spell) =>
            {
                var evt = SpellDamageAttack(3815, 4215, DamageType.Holy, 0.39, 0.117); // TODO are these coefficients correct?
                evt.Spell = spell;
                AdjustForInq(evt);
                CombatTableDoubleRoll(evt, critBoost:0.6);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["Exorcism"] = (spell) =>
            {
                double scale = 1.2;
                if (Sim.Buff("The Art of War").IsActive)
                    scale += 1;
                
                var evt = SpellDamageAttack(2591, 2891, DamageType.Holy, 0.34); // TODO should this be 0.344?
                evt.Spell = spell;
                int glyph = (int)(evt.DamageAmount * 0.2 / (1 + MagicalDamageBoost)); // TODO don't calc this here
                ScaleDamage(evt, scale); // Blazing Light + Art of War
                AdjustForInq(evt);
                evt.DamageAmount += glyph;
                CombatTableSpellRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["Judgement"] = (spell) =>
            {
                var evt = SpellDamageAttack(1, DamageType.Holy, 0.1421, 0.2229);
                evt.Spell = spell;
                ScaleDamage(evt, 1.2); // 2H spec
                ScaleDamage(evt, (1 + 0.1 * Sim.Buff("Censure").Stacks));
                AdjustForInq(evt);
                CombatTableDoubleRoll(evt, critBoost:0.12);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["Holy Wrath"] = (spell) =>
            {
                var evt = SpellDamageAttack(2402, DamageType.Holy, 0, 0.61); // TODO is base 2435?
                evt.Spell = spell;
                AdjustForInq(evt);
                CombatTableSpellRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["T:Consecration"] = (spell) =>
            {
                var evt = SpellDamageAttack(81, DamageType.Holy, 0.026, 0.026); // TODO are these correct?
                evt.Spell = spell;
                AdjustForInq(evt);
                CombatTableSpellRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["T:Censure"] = (spell) =>
            {
                var evt = SpellDamageAttack(0, DamageType.Holy, 0.0192, 0.01); // TODO should AP be 0.0193? Exemplar: 0.033, 0.017
                evt.Spell = spell;
                ScaleDamage(evt, (1 + 0.3 + 0.12)); // Inquiry of Faith + Seals of the Pure
                ScaleDamage(evt, Sim.Buff("Censure").Stacks);
                AdjustForInq(evt);
                CombatTableSpellRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["B:Seal of Truth"] = (spell) =>
            {
                var evt = WeaponDamageAttack(MainHandSpeed, 0.03 * Sim.Buff("Censure").Stacks, DamageType.Holy); // TODO not normalized? 3% per stack?
                evt.Spell = spell;
                ScaleDamage(evt, 1.2); // 2H spec
                ScaleDamage(evt, 1.12); // Seals of the Pure
                AdjustForInq(evt);
                CombatTableDoubleRoll(evt);
                RaiseCombatLogEvent(evt);
            };
            
            Handlers["B:Seals of Command"] = (spell) =>
            {
                var evt = WeaponDamageAttack(MainHandSpeed, 0.07, DamageType.Holy); // TODO not normalized?
                evt.Spell = spell;
                ScaleDamage(evt, 1.2); // 2H spec
                AdjustForInq(evt);
                CombatTableDoubleRoll(evt);
                RaiseCombatLogEvent(evt);
            };
        }
        
        private CombatLogEvent WeaponDamageAttack (double speed, double multiplier = 1, DamageType damageType = DamageType.Physical)
        {
            int baseDmg = Random.Next(MainHandMin, MainHandMax + 1);
            
            int apDmg = (int)(ActualAttackPower * speed / 14);
            int value = (int)((baseDmg + apDmg) * multiplier);
            
            value = AdjustForAW(value);
            
            // TODO: handle Physical + Magic attack type ?
            if ((damageType & DamageType.Physical) != 0)
                value = AdjustPhysicalAttack(value);
            else
                value = AdjustMagicalAttack(value);
            
            var evt = new CombatLogEvent();
            evt.DamageAmount = value;
            evt.DamageType = damageType;
            
            return evt;
        }
        
        private CombatLogEvent SpellDamageAttack (int baseMin, int baseMax, DamageType damageType, double apScale = 0, double spScale = 0)
        {
            int baseDmg = Random.Next(baseMin, baseMax + 1);
            
            int apDmg = (int)(ActualAttackPower * apScale);
            int spDmg = (int)(ActualSpellPower * spScale);
            int value = baseDmg + apDmg + spDmg;
            
            value = AdjustForAW(value);
            
            // TODO: handle Physical + Magic attack type ?
            if ((damageType & DamageType.Physical) != 0)
                value = AdjustPhysicalAttack(value);
            else
                value = AdjustMagicalAttack(value);
            
            var evt = new CombatLogEvent();
            evt.DamageAmount = value;
            evt.DamageType = damageType;
            
            return evt;
        }
        
        private CombatLogEvent SpellDamageAttack (int baseValue, DamageType damageType, double apScale = 0, double spScale = 0)
        {
            return SpellDamageAttack(baseValue, baseValue, damageType, apScale, spScale);
        }
        
        private void ScaleDamage (CombatLogEvent evt, double scale)
        {
            evt.DamageAmount = (int)(evt.DamageAmount * scale);
        }
        
        private int ActualAttackPower
        {
            get
            {
                return AttackPower; // TODO consider temp buffs
            }
        }
        
        private int ActualSpellPower
        {
            get
            {
                return SpellPower; // TODO consider temp buffs
            }
        }
        
        private int AdjustForAW (int value)
        {
            if (Sim.Buff("Avenging Wrath").IsActive)
                return (int)(value * 1.2);
            
            return value;
        }
        
        private void AdjustForInq (CombatLogEvent evt)
        {
            if (Sim.Buff("Inquisition").IsActive)
                evt.DamageAmount = (int)(evt.DamageAmount * 1.3);
        }
        
        private static readonly Spell HoLSpell = new Spell("Hand of Light");
        
        private void HandOfLightDamage (CombatLogEvent evt)
        {
            CombatLogEvent hol = new CombatLogEvent();
            hol.Spell = HoLSpell;
            hol.Time = evt.Time;
            
            hol.DamageAmount = (int)(evt.DamageAmount * Mastery * 2.1);
            hol.DamageType = DamageType.Holy;
            
            hol.AttackType = AttackType.Hit; // TODO can this miss/etc?
            hol.DefenseType = DefenseType.None; // TODO ^ ?
            
            AdjustForInq(hol);
            RaiseCombatLogEvent(hol);
        }
    }
}
