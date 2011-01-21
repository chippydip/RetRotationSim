//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of Buff.
    /// </summary>
    public sealed class Buff : SimulatorSpell
    {
        public Buff (Simulator sim, object secret, string name,
                     Func<TimeSpan> duration = null,
                     int maxStack = 1,
                     Func<TimeSpan> tickPeriod = null)
            : base(sim, name)
        {
            Contract.Requires(secret != null);
            Contract.Requires(maxStack >= 1);
            
            _secret = secret;
            
            _duration = duration ?? (() => TimeSpan.Zero);
            
            MaxStacks = maxStack;
            
            _tickPeriod = tickPeriod;
        }
        
        private readonly object _secret;
        
        public TimeSpan Expires { get; private set; }
        
        public TimeSpan Remaining { get { return Expires - Sim.Time; } }
        
        private readonly Func<TimeSpan> _duration;
        public TimeSpan Duration { get { return _duration(); } }
        
        public bool IsActive { get { return Expires > Sim.Time; } }
        
        public int MaxStacks { get; private set; }
        public int Stacks { get; private set; }
        
        private readonly Func<TimeSpan> _tickPeriod;
        public TimeSpan TickPeriod { get { return _tickPeriod(); } }
        
        public bool IsPeriodic { get { return _tickPeriod != null; } }
        public TimeSpan NextTick { get; private set; }
        
        public event Action<Buff> OnActivate = delegate { };
        public event Action<Buff, TimeSpan> OnRefresh = delegate { };
        public event Action<Buff, TimeSpan> OnCancel = delegate { };
        public event Action<Buff> OnExpire = delegate { };
        
        public void Reset ()
        {
            if (Sim.IsRunning)
                return;
            
            Expires = Sim.Time;
            Expire();
            
            Expires = TimeSpan.Zero;
            NextTick = TimeSpan.Zero;
        }
        
        public void Activate (object secret)
        {
            if (_secret != secret)
                return;
            
            var remaining = Remaining;
            
            // Expire the buff if there's no overlap but it hasn't yet been expired
            if (Expires == Sim.Time && Stacks > 0)
                Expire();
            
            Expires = Sim.Time + Duration;
            
            if (remaining > TimeSpan.Zero)
            {
                Stacks = Math.Min(Stacks + 1, MaxStacks);
                OnRefresh(this, remaining);
                RaiseCast();
                
                // TODO reset tick timer if it doesn't roll?
            }
            else
            {
                Stacks = 1;
                OnActivate(this);
                RaiseCast();
                
                if (IsPeriodic)
                {
                    NextTick = Sim.Time;
                    // TODO handle things that tick immediatly?
                    ScheduleTick();
                }
            }
            
            if (Expires > Sim.Time)
                Sim.AddEvent(Expires, Expire);
            else
                Stacks = 0; // expires immediatly
        }
        
        private void Expire ()
        {
            if (Expires != Sim.Time)
                return; // was refreshed
            
            OnExpire(this);
            Stacks = 0;
        }
        
        private void ScheduleTick ()
        {
            var next = NextTick + TickPeriod;
            
            if (next <= Expires)
            {
                NextTick = next;
                Sim.AddEvent(NextTick, Tick);
            }
        }
        
        private void Tick ()
        {
            RaiseTick();
            ScheduleTick();
        }
        
        public bool Cancel ()
        {
            var remaining = Remaining;
            
            if (remaining <= TimeSpan.Zero)
                return false;
            
            Expires = Sim.Time;
            OnCancel(this, remaining);
            Stacks = 0;
            return true;
        }
    }
}
