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
            double haste = 1;
            sim.WeaponSpeed = 3.6 / 1.09 / 1.1 / haste;
            sim.SpellHaste = 1.09 * 1.05 * haste;
            sim.Mastery = 0.08;
            sim.Has4pT11 = false;
            sim.Has4pPvp = false;
            sim.HasConsecrationGlyph = false;
            
            sim.OnEvent += PickAbility6sRefresh;
            
            sim.Random = new Random(0); // same sequence all the time
            
            var recount = new Recount(sim);
            
            sim.Run(TimeSpan.FromMinutes(600));
            
            recount.Print();
            
            //PrintResult(sim);
            
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
        
        private static void SpamCS (Simulator sim)
        {
            sim.Cast("Crusader Strike");
        }
        
        private static void PickAbility6sRefresh (Simulator sim)
        {
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            
            // Inquisition
            if (sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(6))
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
        
        private static void PickAbility3sRefresh (Simulator sim)
        {
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            
            // Inquisition
            if ((sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(3)) || inqRemaining <= TimeSpan.Zero)
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