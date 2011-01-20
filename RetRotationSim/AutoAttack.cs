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
            
            _sim = sim;
            _swingTimer = swingTimer;
            
            NextSwing = TimeSpan.Zero;
        }
        
        private readonly Simulator _sim;
        
        private readonly Func<TimeSpan> _swingTimer;
        public TimeSpan SwingTimer { get { return _swingTimer(); } }
        
        public TimeSpan NextSwing { get; private set; }
        public bool IsAttacking { get; private set; }
        
        public event Action OnSwing = delegate { };
        
        public void Start ()
        {
            IsAttacking = true;
            
            if (NextSwing < _sim.Time)
                NextSwing = _sim.Time;
        }
        
        public void Stop ()
        {
            Update();
            IsAttacking = false;
        }
        
        public void Toggle ()
        {
            if (IsAttacking)
                Stop();
            else
                Start();
        }
        
        public void Update ()
        {
            if (!IsAttacking)
                return;
            
            while (NextSwing <= _sim.Time)
            {
                OnSwing();
                NextSwing += SwingTimer;
            }
        }
    }
}
