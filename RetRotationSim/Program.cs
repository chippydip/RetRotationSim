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
            //calc.Random = new Random(0);
            
            SetRedcape513(calc);
            //SetExemplar107(calc);
            
            var recount = new Recount(calc);
            //sim.Random = new Random(0); // same sequence all the time
            
            Histogram hist = new Histogram(-1e6, 1e6, 1); // not really a histogram, but will keep track of Mean and StdDev
            
            int str1 = calc.Strength;
            int str2 = str1 + 100;
            
            int ap1 = calc.AttackPower;
            int ap2 = ap1 + (int)(100 * 1.05 *1.05 * 2 * 1.1);
            
            int sp1 = calc.SpellPower;
            int sp2 = sp1 + (int)((ap2 - ap1) * 0.3);
            
            double crit1 = calc.MeleeCritChance;
            double crit2 = crit1 + 100 / 17928.0;
            
            double spellCrit1 = calc.SpellCritChance;
            double spellCrit2 = spellCrit1 + 100 / 17928.0;
            
            double haste1 = sim.SpellHaste / 1.05 / 1.09;
            double haste2 = haste1 + 100 / 12805.701;
            
            double ws1 = sim.WeaponSpeed;
            double ws2 = sim.WeaponSpeed * haste1 / haste2;
            
            double spellHaste1 = sim.SpellHaste;
            double spellHaste2 = sim.SpellHaste / haste1 * haste2;
            
            double mastery1 = sim.Mastery;
            double mastery2 = sim.Mastery + 100 / 17928.0;
            
            var test = "mastery";
            
            if (test != "str")
            {
                str2 = str1;
                ap2 = ap1;
                sp2 = sp1;
            }
            
            if (test != "crit")
            {
                crit2 = crit1;
                spellCrit2 = spellCrit1;
            }
            
            if (test != "haste")
            {
                ws2 = ws1;
                spellHaste2 = spellHaste1;
            }
            
            if (test != "mastery")
                mastery2 = mastery1;
            
            int minutes = 600;
            
            for (int i = 0; i < 10000; ++i)
            {
                calc.Strength = str1;
                calc.AttackPower = ap1;
                calc.SpellPower = sp1;
                calc.MeleeCritChance = crit1;
                calc.SpellCritChance = spellCrit1;
                sim.WeaponSpeed = ws1;
                sim.SpellHaste = spellHaste1;
                sim.Mastery = mastery1;
                sim.Run(TimeSpan.FromMinutes(minutes));
                double dps1 = recount.Dps;
                recount.Reset();
                
                calc.Strength = str2;
                calc.AttackPower = ap2;
                calc.SpellPower = sp2;
                calc.MeleeCritChance = crit2;
                calc.SpellCritChance = spellCrit2;
                sim.WeaponSpeed = ws2;
                sim.SpellHaste = spellHaste2;
                sim.Mastery = mastery2;
                sim.Run(TimeSpan.FromMinutes(minutes));
                double dps2 = recount.Dps;
                recount.Reset();
                
                hist.Add(dps2 - dps1);
                
                if (i % 10 == 9)
                    Console.WriteLine("Average DPS: {0:0.0} StdDev: {1:0.0}", hist.Mean, hist.SampleStandardDeviation);
            }
            
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