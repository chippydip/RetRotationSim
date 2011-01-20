//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Diagnostics.Contracts;

namespace RetRotationSim
{
    /// <summary>
    /// Description of AutoAttack.
    /// </summary>
    public class AutoAttack
    {
        public AutoAttack (Simulator sim, Func<TimeSpan> swingTimer)
        {
            Contract.Requires(sim != null);
            Contract.Requires(swingTimer != null);
            
            Sim = sim;
            _swingTimer = swingTimer;
            
            NextSwing = TimeSpan.Zero;
        }
        
        private Simulator Sim { get; set; }
        
        private readonly Func<TimeSpan> _swingTimer;
        public TimeSpan SwingTimer { get { return _swingTimer(); } }
        
        public TimeSpan NextSwing { get; private set; }
        public bool IsAttacking { get; private set; }
        
        public event Action OnSwing = delegate { };
        
        public void Reset ()
        {
            if (Sim.IsRunning)
                return;
            
            IsAttacking = false;
            NextSwing = TimeSpan.Zero;
        }
        
        public void Start ()
        {
            if (IsAttacking)
                return;
            
            IsAttacking = true;
            
            // If NextSwing == Sim.Time we can't tell if the event has fired yet
            // so schedule another event anyway and let Update() skip the 2nd
            if (NextSwing <= Sim.Time)
            {
                NextSwing = Sim.Time;
                Sim.AddEvent(NextSwing, Update);
            }
        }
        
        public void Stop ()
        {
            IsAttacking = false;
        }
        
        public void Toggle ()
        {
            if (IsAttacking)
                Stop();
            else
                Start();
        }
        
        private void Update ()
        {
            if (!IsAttacking)
                return;
            
            // Make sure a swing hasn't already happened during this timestep
            if (NextSwing == Sim.Time)
            {
                OnSwing();
                
                NextSwing += SwingTimer;
                Sim.AddEvent(NextSwing, Update);
            }
        }
    }
}
