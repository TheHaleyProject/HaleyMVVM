﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Events;

namespace Haley.Abstractions
{
        public interface IHaleyVM 
        {
            event EventHandler<FrameClosingEventArgs> OnViewClosed; //this is the name... :) :) :) 
        }
}