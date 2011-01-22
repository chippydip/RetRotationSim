//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Linq;

using RetRotationSim.Collections;

namespace RetRotationSim
{
    class Program
    {
        public static void Main(string[] args)
        {
            DamageCalc calc = new DamageCalc403a();
            //DamageCalc calc = new DamageCalc406();
            
            calc.Sim.OnEvent = PickAbility6sRefresh;
            //calc.Random = new Random(0);
            
            SetRedcape513(calc);
            //SetExemplar107(calc);
            
            //TestDps(calc, 600);
            //TestStatWeights(calc, "str"); // str, crit, haste, or mastery
            
            calc.Sim.Has4pT11 = false;
            TestRotationInqRefresh(calc);
            
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
        
        private static void TestDps (DamageCalc calc, double minutes)
        {
            var recount = new Recount(calc);
            calc.Sim.Run(TimeSpan.FromMinutes(minutes));
            recount.Print();
        }
        
        private static void TestStatWeights (DamageCalc calc, string stat)
        {
            var recount = new Recount(calc);
            
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
            
            double haste1 = calc.Sim.SpellHaste / 1.05 / 1.09;
            double haste2 = haste1 + 100 / 12805.701;
            
            double ws1 = calc.Sim.WeaponSpeed;
            double ws2 = calc.Sim.WeaponSpeed * haste1 / haste2;
            
            double spellHaste1 = calc.Sim.SpellHaste;
            double spellHaste2 = calc.Sim.SpellHaste / haste1 * haste2;
            
            double mastery1 = calc.Sim.Mastery;
            double mastery2 = calc.Sim.Mastery + 100 / 17928.0;
            
            if (stat != "str")
            {
                str2 = str1;
                ap2 = ap1;
                sp2 = sp1;
            }
            
            if (stat != "crit")
            {
                crit2 = crit1;
                spellCrit2 = spellCrit1;
            }
            
            if (stat != "haste")
            {
                ws2 = ws1;
                spellHaste2 = spellHaste1;
            }
            
            if (stat != "mastery")
                mastery2 = mastery1;
            
            int minutes = 600;
            
            for (int i = 0; i < 10000; ++i)
            {
                calc.Strength = str1;
                calc.AttackPower = ap1;
                calc.SpellPower = sp1;
                calc.MeleeCritChance = crit1;
                calc.SpellCritChance = spellCrit1;
                calc.Sim.WeaponSpeed = ws1;
                calc.Sim.SpellHaste = spellHaste1;
                calc.Sim.Mastery = mastery1;
                
                calc.Sim.Run(TimeSpan.FromMinutes(minutes));
                double dps1 = recount.Dps;
                recount.Reset();
                
                calc.Strength = str2;
                calc.AttackPower = ap2;
                calc.SpellPower = sp2;
                calc.MeleeCritChance = crit2;
                calc.SpellCritChance = spellCrit2;
                calc.Sim.WeaponSpeed = ws2;
                calc.Sim.SpellHaste = spellHaste2;
                calc.Sim.Mastery = mastery2;
                
                calc.Sim.Run(TimeSpan.FromMinutes(minutes));
                double dps2 = recount.Dps;
                recount.Reset();
                
                hist.Add(dps2 - dps1);
                
                if (i % 10 == 9)
                    Console.WriteLine("Average DPS: {0:0.0} StdDev: {1:0.0}", hist.Mean, hist.SampleStandardDeviation);
            }
        }
        
        private class RotationEntry : IComparable<RotationEntry>
        {
            public string Name;
            public Histogram Hist;
            
            public int CompareTo (RotationEntry other)
            {
                return Hist.Mean.CompareTo(other.Hist.Mean);
            }
        }
        
        private static void TestRotationInqRefresh (DamageCalc calc)
        {
            var rotation = new Rotation();
            calc.Sim.OnEvent = rotation.OnUpdate;
            
            var heap = new Heap<RotationEntry>();
            var recount = new Recount(calc);
            
            for (int s = 0; s <= 10; ++s)
            {
                rotation.InquisitionOverlap = s;
                
                for (int hp = 1; hp <= 3; ++hp)
                {
                    rotation.MinInquisitionHp = hp;
                    var name = string.Format("Refresh under {0}s with at least {1}hp after falling:", s, hp);
                    Console.WriteLine(name);
                    var hist = new Histogram(-1e6, 1e6, 1);
                    
                    for (int i = 1; i <= 30; ++i)
                    {
                        calc.Sim.Run(TimeSpan.FromMinutes(600));
                        hist.Add(recount.Dps);
                        recount.Reset();
                        
                        if (i % 10 == 0)
                            Console.WriteLine("    Average DPS: {0:0.0} StdDev: {1:0.0}", hist.Mean, hist.SampleStandardDeviation);
                    }
                    
                    heap.Push(new RotationEntry() { Name = name, Hist = hist });
                }
                Console.WriteLine();
            }
            
            // Top 10
            for (int i = 1; i <= 10; ++i)
            {
                var entry = heap.Pop();
                Console.WriteLine("#{0}: {1}: Average DPS: {2:0.0} StdDev: {1:0.0}", i, entry.Name, entry.Hist.Mean, entry.Hist.SampleStandardDeviation);
            }
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