//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Simulator403a.
    /// </summary>
    public sealed class Simulator403a : Simulator
    {
        public Simulator403a ()
        {
            InitBuffs();
            InitWeapon();
            InitAbilities();
        }
        
        public override int EffectiveHolyPower { get { return Buff("Hand of Light").IsActive ? 3 : base.EffectiveHolyPower; } }
        
        private void InitBuffs ()
        {
            // Buffs
            BuffImpl b;
            b = new BuffImpl(this, "The Art of War", () => TimeSpan.FromSeconds(15));
            AddBuff(b);
            
            b = new BuffImpl(this, "Hand of Light", () => TimeSpan.FromSeconds(8));
            AddBuff(b);
            
            b = new BuffImpl(this, "Inquisition", () =>
                             {
                                 var hp = EffectiveHolyPower;
                                 if (Has4pT11)
                                     ++hp;
                                 return TimeSpan.FromSeconds(hp * 10);
                             });
            AddBuff(b);
            
            b = new BuffImpl(this, "Divine Purpose", () => TimeSpan.Zero);
            AddBuff(b);
            
            b = new BuffImpl(this, "Censure", () => TimeSpan.FromSeconds(15),
                             maxStack:5,
                             tickPeriod:() => TimeSpan.FromSeconds(3 / SpellHaste));
            AddBuff(b);
            
            b = new BuffImpl(this, "Seal of Truth", () => TimeSpan.Zero);
            AddBuff(b);
            
            b = new BuffImpl(this, "Seals of Command", () => TimeSpan.Zero);
            AddBuff(b);
        }
        
        private void InitWeapon ()
        {
            // Auto Attacks
            MainHand = new AutoAttack(this, () => TimeSpan.FromSeconds(WeaponSpeed));
            MainHand.OnSwing += () =>
            {
                // Seals of Command
                BuffImpl("Seals of Command").Activate();
                
                ProcSoT(); // before Censure application
                
                // Censure
                BuffImpl("Censure").Activate();
                
                // Art of War
                if (Random.NextDouble() < 0.2)
                    BuffImpl("The Art of War").Activate();
                
                // Hand of Light
                if (Random.NextDouble() < Mastery)
                    BuffImpl("Hand of Light").Activate();
            };
        }
        
        private void InitAbilities ()
        {
            // Abilities
            Ability a;
            
            a = new Ability(this, "Inquisition",
                            isUsable:() => IsHpAbilityUsable);
            a.OnCast += (abil) =>
            {
                BuffImpl("Inquisition").Activate();
                UsedHolyPowerAbility(abil);
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Exorcism",
                            gcd:() => SpellGcd,
                            isUsable:() => Buff("The Art of War").IsActive); // TODO really active all the time
            a.OnCast += (_) =>
            {
                Buff("The Art of War").Cancel();
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Hammer of Wrath",
                            cooldown:() => TimeSpan.FromSeconds(6),
                            gcd:() => SpellGcd,
                            isUsable:() => false); // TODO fix this
            a.OnCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Templar's Verdict",
                            isUsable:() => IsHpAbilityUsable);
            a.OnCast += (abil) =>
            {
                UsedHolyPowerAbility(abil);
                ProcDivinePurpose();
                ProcSoT();
            };
            AddAbility(a);
            
            a = new Ability(this, "Crusader Strike",
                            cooldown:() => TimeSpan.FromSeconds(4.5 / SpellHaste));
            a.OnCast += (_) =>
            {
                HolyPower = Math.Min(HolyPower + 1, 3);
                ProcSoT();
            };
            AddAbility(a);
            
            a = new Ability(this, "Judgement",
                            cooldown:() => TimeSpan.FromSeconds(Has4pPvp ? 7 : 8));
            a.OnCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Holy Wrath",
                            cooldown:() => TimeSpan.FromSeconds(15));
            a.OnCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Consecration",
                            cooldown:() => TimeSpan.FromSeconds(HasConsecrationGlyph ? 36 : 30));
            AddAbility(a);
        }
        
        private void ProcSoT ()
        {
            if (Buff("Censure").IsActive)
            {
                // Seals of Command
                BuffImpl("Seals of Command").Activate();
                
                // Seal of Truth
                BuffImpl("Seal of Truth").Activate();
            }
        }
        
        private void ProcDivinePurpose ()
        {
            if (Random.NextDouble() < 0.4)
            {
                BuffImpl("Divine Purpose").Activate();
                if (HolyPower < 3)
                    ++HolyPower;
                // else
                    // TODO RecordAttack("Divine Purpose Wasted");
            }
        }
        
        private void UsedHolyPowerAbility (Ability abil)
        {
            if (!Buff("Hand of Light").Cancel())
                HolyPower = 0;
        }
    }
}
