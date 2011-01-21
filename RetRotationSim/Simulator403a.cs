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
            Buff b;
            b = new Buff(this, Secret, "The Art of War", () => TimeSpan.FromSeconds(15));
            AddBuff(b);
            
            b = new Buff(this, Secret, "Hand of Light", () => TimeSpan.FromSeconds(8));
            AddBuff(b);
            
            b = new Buff(this, Secret, "Inquisition", () =>
                             {
                                 var hp = EffectiveHolyPower;
                                 if (Has4pT11)
                                     ++hp;
                                 return TimeSpan.FromSeconds(hp * 10);
                             });
            AddBuff(b);
            
            b = new Buff(this, Secret, "Divine Purpose", () => TimeSpan.Zero);
            AddBuff(b);
            
            b = new Buff(this, Secret, "Censure", () => TimeSpan.FromSeconds(15),
                             maxStack:5,
                             tickPeriod:() => TimeSpan.FromSeconds(3 / SpellHaste));
            AddBuff(b);
            
            b = new Buff(this, Secret, "Seal of Truth", () => TimeSpan.Zero);
            AddBuff(b);
            
            b = new Buff(this, Secret, "Seals of Command", () => TimeSpan.Zero);
            AddBuff(b);
            
            b = new Buff(this, Secret, "Consecration", () => TimeSpan.FromSeconds(10),
                         tickPeriod:() => TimeSpan.FromSeconds(1 / SpellHaste));
            AddBuff(b);
        }
        
        private void InitWeapon ()
        {
            // Auto Attacks
            MainHand = new AutoAttack(this, () => TimeSpan.FromSeconds(WeaponSpeed));
            MainHand.OnSwing += () =>
            {
                // Seals of Command
                Buff("Seals of Command").Activate(Secret);
                
                ProcSoT(); // before Censure application
                
                // Censure
                Buff("Censure").Activate(Secret);
                
                // Art of War
                if (Random.NextDouble() < 0.2)
                    Buff("The Art of War").Activate(Secret);
                
                // Hand of Light
                if (Random.NextDouble() < Mastery)
                    Buff("Hand of Light").Activate(Secret);
            };
        }
        
        private void InitAbilities ()
        {
            // Abilities
            Ability a;
            
            a = new Ability(this, "Inquisition",
                            isUsable:() => IsHpAbilityUsable);
            a.AfterCast += (abil) =>
            {
                Buff("Inquisition").Activate(Secret);
                UsedHolyPowerAbility();
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Exorcism",
                            gcd:() => SpellGcd,
                            isUsable:() => Buff("The Art of War").IsActive); // TODO really active all the time
            a.AfterCast += (_) =>
            {
                Buff("The Art of War").Cancel();
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Hammer of Wrath",
                            cooldown:() => TimeSpan.FromSeconds(6),
                            gcd:() => SpellGcd,
                            isUsable:() => false); // TODO fix this
            a.AfterCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Templar's Verdict",
                            isUsable:() => IsHpAbilityUsable);
            a.AfterCast += (abil) =>
            {
                UsedHolyPowerAbility();
                ProcDivinePurpose();
                ProcSoT();
            };
            AddAbility(a);
            
            a = new Ability(this, "Crusader Strike",
                            cooldown:() => TimeSpan.FromSeconds(4.5 / SpellHaste));
            a.AfterCast += (_) =>
            {
                HolyPower = Math.Min(HolyPower + 1, 3);
                ProcSoT();
            };
            AddAbility(a);
            
            a = new Ability(this, "Judgement",
                            cooldown:() => TimeSpan.FromSeconds(Has4pPvp ? 7 : 8));
            a.AfterCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Holy Wrath",
                            cooldown:() => TimeSpan.FromSeconds(15));
            a.AfterCast += (_) =>
            {
                ProcDivinePurpose();
            };
            AddAbility(a);
            
            a = new Ability(this, "Consecration",
                            cooldown:() => TimeSpan.FromSeconds(HasConsecrationGlyph ? 36 : 30));
            a.AfterCast += (_) =>
            {
                Buff("Consecration").Activate(Secret);
            };
            AddAbility(a);
        }
        
        private void ProcSoT ()
        {
            if (Buff("Censure").IsActive)
            {
                // Seals of Command
                Buff("Seals of Command").Activate(Secret);
                
                // Seal of Truth
                Buff("Seal of Truth").Activate(Secret);
            }
        }
        
        private void ProcDivinePurpose ()
        {
            if (Random.NextDouble() < 0.4)
            {
                Buff("Divine Purpose").Activate(Secret);
                if (HolyPower < 3)
                    ++HolyPower;
                // else
                    // TODO RecordAttack("Divine Purpose Wasted");
            }
        }
        
        private void UsedHolyPowerAbility ()
        {
            if (!Buff("Hand of Light").Cancel())
                HolyPower = 0;
        }
    }
}
