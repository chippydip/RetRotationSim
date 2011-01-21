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
            
            double haste = 1 + (1812 + 65) / 12805.7;
            sim.WeaponSpeed = 3.8 / 1.09 / 1.1 / haste;
            sim.SpellHaste = 1.09 * 1.05 * haste;
            sim.Mastery = 0.08;
            sim.Has4pT11 = false;
            sim.Has4pPvp = false;
            sim.HasConsecrationGlyph = false;
            
            sim.OnEvent = PickAbility6sRefresh;
            DamageCalc calc = new DamageCalc403a(sim);
            calc.Random = new Random(0);
            
            calc.MainHandSpeed = 3.8;
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
        
        private static void PickAbility6sRefresh (Simulator sim)
        {
            sim.MainHand.Start();
            
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            
            // Inquisition
            if (sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(6))
                sim.Cast("Inquisition");
            
            // Stacked Cooldowns
            if (sim.HasMaxHolyPower)
            {
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