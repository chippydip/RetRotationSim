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
    public class Buff
    {
        public Buff (BuffImpl impl)
        {
            Contract.Requires(impl != null);
            
            Impl = impl;
        }
        
        private BuffImpl Impl { get; set; }
        
        public string Name { get { return Impl.Name; } }
        
        public TimeSpan Expires { get { return Impl.Expires; } }
        
        public TimeSpan Remaining { get { return Impl.Remaining; } }
        
        //public TimeSpan NextTick { get { return _imp.NextTick; } }
        
        public TimeSpan Duration { get { return Impl.Duration; } }
        
        public bool IsActive { get { return Impl.IsActive; } }
        
        public int MaxStacks { get { return Impl.MaxStacks; } }
        public int Stacks { get { return Impl.Stacks; } }
        
        public event Action<Buff> OnActivate
        {
            add { Impl.OnActivate += value; }
            remove { Impl.OnActivate -= value; }
        }
        
        public event Action<Buff, TimeSpan> OnRefresh
        {
            add { Impl.OnRefresh += value; }
            remove { Impl.OnRefresh -= value; }
        }
        
        public event Action<Buff, TimeSpan> OnCancel
        {
            add { Impl.OnCancel += value; }
            remove { Impl.OnCancel -= value; }
        }
        
        public bool Cancel ()
        {
            return Impl.Cancel();
        }
    }
    
    public class BuffImpl
    {
        public BuffImpl (Simulator sim, string name, Func<TimeSpan> duration,
                         int maxStack = 1,
                         Func<TimeSpan> tickPeriod = null)
        {
            Contract.Requires(sim != null);
            Contract.Requires(name != null);
            Contract.Requires(duration != null);
            Contract.Requires(maxStack >= 1);
            
            Buff = new Buff(this);
            
            Sim = sim;
            Name = name;
            _duration = duration;
            
            MaxStacks = maxStack;
            
            _tickPeriod = tickPeriod;
        }
        
        public Buff Buff { get; private set; }
        
        private Simulator Sim { get; set; }
        
        public string Name { get; private set; }
        
        public TimeSpan Expires { get; private set; }
        
        public TimeSpan Remaining { get { return Expires - Sim.Time; } }
        
        //public TimeSpan NextTick { get; private set; }
        
        private readonly Func<TimeSpan> _duration;
        public TimeSpan Duration { get { return _duration(); } }
        
        public bool IsActive { get { return Expires > Sim.Time; } }
        
        public int MaxStacks { get; private set; }
        public int Stacks { get; private set; } // not set to zero once the buff expires or is canceled
        
        private readonly Func<TimeSpan> _tickPeriod;
        public TimeSpan TickPeriod { get { return _tickPeriod != null ? _tickPeriod() : TimeSpan.MaxValue; } }
        
        public bool IsPeriodic { get { return _tickPeriod != null; } }
        public TimeSpan NextTick { get; private set; }
        
        public event Action<Buff> OnActivate = delegate { };
        public event Action<Buff, TimeSpan> OnRefresh = delegate { };
        public event Action<Buff, TimeSpan> OnCancel = delegate { };
        
        public event Action<Buff> OnTick = delegate { };
        
        public void Activate ()
        {
            var remaining = Remaining;
            
            Expires = Sim.Time + Duration;
            
            if (remaining > TimeSpan.Zero)
            {
                Stacks = Math.Min(Stacks + 1, MaxStacks);
                // TODO reset tick timer if it doesn't roll?
                OnRefresh(this.Buff, remaining);
            }
            else
            {
                Stacks = 1;
                // TODO handle things that tick immediatly?
                NextTick = Sim.Time + TickPeriod;
                OnActivate(this.Buff);
            }
        }
        
        public void Update ()
        {
            if (!IsPeriodic)
                return;
            
            while (NextTick <= Sim.Time && NextTick <= Expires)
            {
                OnTick(this.Buff);
                NextTick += TickPeriod;
            }
        }
        
        public bool Cancel ()
        {
            var remaining = Remaining;
            
            if (remaining <= TimeSpan.Zero)
                return false;
            
            Expires = Sim.Time;
            OnCancel(this.Buff, remaining);
            return true;
        }
    }
}
