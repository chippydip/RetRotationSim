//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Linq;

namespace RetRotationSim
{
    class Program
    {
        public static void Main(string[] args)
        {
            Simulator sim = new Simulator403a();
            
            sim.OnEvent = PickAbility6sRefresh;
            DamageCalc calc = new DamageCalc403a(sim);
            calc.Random = new Random(0);
            
            SetRedcape513(calc);
            //SetExemplar107(calc);
            
            var recount = new Recount(calc);
            
            sim.Random = new Random(0); // same sequence all the time
            sim.Run(TimeSpan.FromMinutes(600));
            recount.Print();
            
            recount.Reset();
            
            // Run again to make sure we get the same results
            //sim.Random = new Random(0);
            //sim.Run(TimeSpan.FromMinutes(600));
            //recount.Print();
            
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
        
        private static void SetRedcape513 (DamageCalc calc)
        {
            double weaponSpeed = 3.8;
            int hasteRating = 1812 + 65;
            
            double haste = 1 + hasteRating / 12805.7;
            
            calc.Sim.WeaponSpeed = weaponSpeed / 1.09 / 1.1 / haste;
            calc.Sim.SpellHaste = 1.09 * 1.05 * haste;
            calc.Sim.Mastery = 0.1422;
            
            calc.Sim.Has4pT11 = true;
            calc.Sim.Has4pPvp = false;
            calc.Sim.HasConsecrationGlyph = false;
            
            calc.MainHandSpeed = weaponSpeed;
            calc.MainHandMin = 1894;
            calc.MainHandMax = 2843;
            
            // From Redcape
            calc.Strength = 6343;
            calc.Agility = 695;
            calc.Intellect = 131;
            
            calc.AttackPower = 14971;
            calc.SpellPower = 5074;
            
            calc.MeleeCritChance = 0.0818;
            calc.SpellCritChance = 0.1515;
            //Attack Speed
            calc.MeleeMissChance = 0;
            calc.MeleeDodgeChance = 0;
            calc.SpellMissChance = 0;
            //Mastery
            calc.PhysicalDamageBoost = 0.0918;
            calc.MagicalDamageBoost = 1.4674 / (1 + 0.3 * 0.98) - 1;
            calc.ArmorReduction = 0.2879;
        }
        
        private static void SetExemplar107 (DamageCalc calc)
        {
            double weaponSpeed = 3.6;
            
            double haste = 1.148;
            
            calc.Sim.WeaponSpeed = weaponSpeed / 1.09 / 1.1 / haste;
            calc.Sim.SpellHaste = 1.09 * 1.05 * haste;
            calc.Sim.Mastery = 0.08 + 0.027;
            
            calc.Sim.Has4pT11 = true;
            calc.Sim.Has4pPvp = false;
            calc.Sim.HasConsecrationGlyph = false;
            
            calc.MainHandSpeed = weaponSpeed;
            calc.MainHandMin = 1894;
            calc.MainHandMax = 2843;
            
            // From Redcape
            calc.Strength = 5981;
            calc.Agility = 689;
            calc.Intellect = 122;
            
            calc.AttackPower = 13627;
            calc.SpellPower = 4804;
            
            calc.MeleeCritChance = 0.147;
            calc.SpellCritChance = 0.143;
            //Attack Speed
            calc.MeleeMissChance = 0;
            calc.MeleeDodgeChance = 0;
            calc.SpellMissChance = 0;
            //Mastery
            calc.PhysicalDamageBoost = 0.0918;
            calc.MagicalDamageBoost = 1.4674 / (1 + 0.3 * 0.98) - 1;
            calc.ArmorReduction = 0.2879;
        }
        
        private static void PickAbility6sRefresh (Simulator sim)
        {
            // /startattack
            sim.MainHand.Start();
            
            // Cooldowns when ready
            sim.Cast("Bloodlust");
            sim.Cast("Avenging Wrath");
            sim.Cast("Zealotry");
            
            // Inquisition
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            if (sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(6))
                sim.Cast("Inquisition");
            
            // Stacked Cooldowns
            if (sim.Ability("Zealotry").IsUsable)
            {
                sim.Cast("Bloodlust");
                sim.Cast("Avenging Wrath");
                sim.Cast("Zealotry");
            }
            
            // HoW
            sim.Cast("Hammer of Wrath");
            
            // Exorcism
            if (sim.Buff("The Art of War").IsActive)
                sim.Cast("Exorcism");
            
            // Templar's Verdict
            if (sim.HasMaxHolyPower)
                sim.Cast("Templar's Verdict");
            
            // No special conditionals for the rest of these
            sim.Cast("Crusader Strike");
            sim.Cast("Judgement");
            sim.Cast("Holy Wrath");
            sim.Cast("Consecration");
        }
        
        private static void PickAbility3sRefresh (Simulator sim)
        {
            if (!sim.MainHand.IsAttacking)
                sim.MainHand.Start();
            
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            
            // Inquisition
            if ((sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(0)))// || inqRemaining <= TimeSpan.Zero)
                sim.Cast("Inquisition");
            
            // TODO HoW
            
            // Exorcism
            if (sim.Buff("The Art of War").IsActive)
                sim.Cast("Exorcism");
            
            // Templar's Verdict
            if (sim.HasMaxHolyPower)
                sim.Cast("Templar's Verdict");
            
            // No special conditionals for the rest of these
            sim.Cast("Crusader Strike");
            sim.Cast("Judgement");
            sim.Cast("Holy Wrath");
            sim.Cast("Consecration");
        }
        
    }
}