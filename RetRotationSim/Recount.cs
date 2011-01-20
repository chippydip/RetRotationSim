//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Recount.
    /// </summary>
    public class Recount
    {
        public Recount (Simulator sim)
        {
            sim.MainHand.OnSwing += OnMainHandSwing;
            
            foreach (var abil in sim.Abilities)
            {
                abil.OnCast += OnCast;
                
                switch (abil.Name)
                {
                    case "Inquisition":
                        break;
                    case "Exorcism":
                        break;
                    case "Hammer of Wrath":
                        break;
                    case "Templar's Verdict":
                        break;
                    case "Crusader Strike":
                        break;
                    case "Judgement":
                        break;
                    case "Holy Wrath":
                        break;
                    case "Consecration":
                        break;
                    default:
                        Console.WriteLine("Unexpected Ability: {0}", abil.Name);
                        break;
                }
            }
            
            foreach (var buff in sim.Buffs)
            {
                buff.OnActivate += OnActivate;
                buff.OnRefresh += OnRefresh;
                buff.OnCancel += OnCancel;
            }
        }
        
        private readonly Dictionary<string, int> _ability = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _buff = new Dictionary<string, int>();
        
        public void PrintAbilities ()
        {
            Console.WriteLine(" Count |Ability");
            Console.WriteLine("-------+-------------------------");
            foreach (var kvp in _ability.OrderBy(kvp => -kvp.Value))
                Console.WriteLine("{0,6} | {1}", kvp.Value, kvp.Key);
            Console.WriteLine("-------+-------------------------");
        }
        
        public void PrintBuffs ()
        {
            Console.WriteLine(" Count |Buff Stats");
            Console.WriteLine("-------+-------------------------");
            foreach (var kvp in _buff.OrderBy(kvp => -kvp.Value))
                Console.WriteLine("{0,6} | {1}", kvp.Value, kvp.Key);
            Console.WriteLine("-------+-------------------------");
        }
        
        public void Print ()
        {
            PrintAbilities();
            PrintBuffs();
        }
        
        private void OnMainHandSwing ()
        {
            Record(_ability, "Melee");
        }
        
        private void OnCast (Ability abil)
        {
            Record(_ability, abil.Name);
        }
        
        private void OnActivate (Buff buff)
        {
            Record(_buff, buff.Name);
            Record(_buff, buff.Name + " Gained");
        }
        
        private void OnRefresh (Buff buff, TimeSpan remaining)
        {
            Record(_buff, buff.Name);
            Record(_buff, buff.Name + " Refreshed");
        }
        
        private void OnCancel (Buff buff, TimeSpan remaingin)
        {
            Record(_buff, buff.Name + " Canceled");
        }
        
        private static void Record (Dictionary<string, int> dict, string name)
        {
            int count = 0;
            dict.TryGetValue(name, out count);
            ++count;
            dict[name] = count;
        }
    }
}
