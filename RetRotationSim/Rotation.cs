//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Rotation.
    /// </summary>
    public class Rotation
    {
        public Rotation ()
        {
            InquisitionOverlap = 6;
            MinInquisitionHp = 3;
        }
        
        public double InquisitionOverlap { get; set; }
        
        public int MinInquisitionHp { get; set; }
        
        public void OnUpdate (Simulator sim)
        {
            // /startattack
            sim.MainHand.Start();
            
            // Cooldowns when ready
            sim.Cast("Bloodlust");
            sim.Cast("Avenging Wrath");
            sim.Cast("Zealotry");
            
            // Inquisition
            var inqRemaining = sim.Buff("Inquisition").Remaining;
            if (sim.HasMaxHolyPower && inqRemaining < TimeSpan.FromSeconds(InquisitionOverlap))
                sim.Cast("Inquisition");
            
            if (inqRemaining < TimeSpan.Zero && sim.EffectiveHolyPower >= MinInquisitionHp)
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
    }
}
