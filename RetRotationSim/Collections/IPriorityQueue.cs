//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Collections.Generic;

namespace RetRotationSim.Collections
{
    /// <summary>
    /// Description of IPriorityQueue.
    /// </summary>
    public interface IPriorityQueue<T> : ICollection<T>
    {
        void Push (T value);
        
        T Peek ();
        
        T Pop ();
        
        T[] ToArray ();
    }
}
