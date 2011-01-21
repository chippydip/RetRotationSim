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
        public Recount (DamageCalc calc)
        {
            calc.OnCombatLogEvent += OnCombatLogEvent;
            
            var sim = calc.Sim;
            
            sim.EnteringCombat += EnteringCombat;
            sim.LeavingCombat += LeavingCombat;
            
            sim.MainHand.OnSwing += OnMainHandSwing;
            
            foreach (var abil in sim.Abilities)
                abil.OnCast += OnCast;
            
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
            
            _damage.Clear();
            _totalTime = TimeSpan.Zero;
        }
        
        private readonly Dictionary<string, Dictionary<string, int>> _ability = new Dictionary<string, Dictionary<string, int>>();
        private readonly Dictionary<string, Dictionary<string, int>> _buff = new Dictionary<string, Dictionary<string, int>>();
        
        private readonly Dictionary<string, long> _damage = new Dictionary<string, long>();
        
        private TimeSpan _totalTime;
        
        public long TotalDamage { get { return _damage.Values.Sum(); } }
        public double Dps { get { return TotalDamage / _totalTime.TotalSeconds; } }
        
        public void PrintAbilities ()
        {
            Console.WriteLine(" Count  | Ability");
            Console.WriteLine("--------+-------------------------");
            PrintDict(_ability);
            Console.WriteLine("--------+-------------------------");
        }
        
        public void PrintBuffs ()
        {
            Console.WriteLine(" Count  | Buff Stats");
            Console.WriteLine("--------+-------------------------");
            PrintDict(_buff);
            Console.WriteLine("--------+-------------------------");
        }
        
        private static void PrintDict (Dictionary<string, Dictionary<string, int>> dict)
        {
            foreach (var kvp in dict.OrderBy(v => -v.Value[""]))
            {
                int count = kvp.Value[""];
                Console.WriteLine("{0,7} | {1}", count, kvp.Key);
                foreach (var detail in kvp.Value.OrderBy(v => -v.Value).Where(v => v.Key != ""))
                {
                    bool show = true;
                    switch (detail.Key)
                    {
                        case "Gained":
                            if (detail.Value == count || detail.Value == 1)
                                show = false;
                            break;
                        case "Refreshed":
                            if (detail.Value == count - 1)
                                show = false;
                            break;
                        case "Used":
                            if (detail.Value == count)
                                show = false;
                            break;
                        case "Expired":
                            show = false; // always the same as Gained
                            break;
                        default:
                            break;
                    }
                    if (show)
                        Console.WriteLine("        | {0,6} ({1:00.0%}) {2}", detail.Value, detail.Value / (float)count, detail.Key);
                }
            }
        }
        
        public void PrintDamage ()
        {
            Console.WriteLine(" Damage | Source");
            Console.WriteLine("--------+-------------------------");
            long total = TotalDamage;
            foreach (var kvp in _damage.OrderBy(v => -v.Value))
            {
                Console.WriteLine("{0,7} | {1,-18} {2:00.0%} (avg {3,5})", ShortForm(kvp.Value), kvp.Key, kvp.Value / (double)TotalDamage, kvp.Value / GetCount(kvp.Key));
            }
            Console.WriteLine("--------+-------------------------");
            
            Console.WriteLine("Total: {0} ({1:0.0} dps)", ShortForm(total), ShortForm(total / _totalTime.TotalSeconds, 3));
        }
        
        private static string ShortForm (double value, int places = 1)
        {
            string format = "N" + places;
            if (value > 1000000000)
                return (value * 0.000000001).ToString(format) + 'b';
            if (value > 1000000)
                return (value * 0.000001).ToString(format) + 'm';
            if (value > 1000)
                return (value * 0.001).ToString(format) + 'k';
            return value.ToString(format);
        }
        
        private int GetCount (string name)
        {
            Dictionary<string, int> map;
            if (!_ability.TryGetValue(name, out map))
                if (!_buff.TryGetValue(name, out map))
                    return 1;
            
            int count;
            if (!map.TryGetValue("Ticked", out count))
                if (!map.TryGetValue("", out count))
                    return 1;
            
            return count;
        }
        
        public void Print ()
        {
            PrintBuffs();
            PrintAbilities();
            PrintDamage();
        }
        
        private void EnteringCombat (Simulator sim)
        {
        }
        
        private void LeavingCombat (Simulator sim)
        {
            _totalTime += sim.Time;
        }
        
        private void OnCombatLogEvent (CombatLogEvent evt)
        {
            string name = evt.Spell != null ? evt.Spell.Name : "Melee";
            
            long amount;
            _damage.TryGetValue(name, out amount);
            
            amount += evt.DamageAmount;
            
            if (amount != 0)
                _damage[name] = amount;
        }
        
        private void OnMainHandSwing ()
        {
            Record(_ability, "Melee");
        }
        
        private void OnCast (Spell abil)
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
            Record(_buff, buff.Name, "Used");
        }
        
        private void OnTick (Spell buff)
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
