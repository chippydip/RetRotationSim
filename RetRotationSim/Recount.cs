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
                /*
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
                */
            }
            
            foreach (var buff in sim.Buffs)
            {
                buff.OnActivate += OnActivate;
                buff.OnRefresh += OnRefresh;
                buff.OnExpire += OnExpire;
                buff.OnCancel += OnCancel;
                buff.OnTick += OnTick;
            }
        }
        
        public void Reset ()
        {
            _ability.Clear();
            _buff.Clear();
        }
        
        private readonly Dictionary<string, Dictionary<string, int>> _ability = new Dictionary<string, Dictionary<string, int>>();
        private readonly Dictionary<string, Dictionary<string, int>> _buff = new Dictionary<string, Dictionary<string, int>>();
        
        public void PrintAbilities ()
        {
            Console.WriteLine(" Count |Ability");
            Console.WriteLine("-------+-------------------------");
            PrintDict(_ability);
            Console.WriteLine("-------+-------------------------");
        }
        
        public void PrintBuffs ()
        {
            Console.WriteLine(" Count |Buff Stats");
            Console.WriteLine("-------+-------------------------");
            PrintDict(_buff);
            Console.WriteLine("-------+-------------------------");
        }
        
        private static void PrintDict (Dictionary<string, Dictionary<string, int>> dict)
        {
            foreach (var kvp in dict.OrderBy(v => -v.Value[""]))
            {
                int count = kvp.Value[""];
                Console.WriteLine("{0,6} | {1}", count, kvp.Key);
                foreach (var detail in kvp.Value.OrderBy(v => -v.Value).Where(v => v.Key != ""))
                    if (detail.Value != count)
                        Console.WriteLine("       | {0,6} ({1:00.0%}) {2}", detail.Value, detail.Value / (float)count, detail.Key);
            }
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
            Record(_buff, buff.Name, "Gained");
        }
        
        private void OnRefresh (Buff buff, TimeSpan remaining)
        {
            Record(_buff, buff.Name);
            Record(_buff, buff.Name, "Refreshed");
        }
        
        private void OnExpire (Buff buff)
        {
            Record(_buff, buff.Name, "Expired");
        }
        
        private void OnCancel (Buff buff, TimeSpan remaingin)
        {
            Record(_buff, buff.Name, "Canceled");
        }
        
        private void OnTick (Buff buff)
        {
            Record(_buff, buff.Name, "Ticked");
        }
        
        private static void Record (Dictionary<string, Dictionary<string, int>> dict, string name, string type = "")
        {
            Dictionary<string, int> map;
            if (!dict.TryGetValue(name, out map))
            {
                map = new Dictionary<string, int>();
                map[""] = 0;
                dict[name] = map;
            }
            
            int count;
            map.TryGetValue(type, out count);
            map[type] = ++count;
        }
    }
}
