using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using beh = Microsoft.Xaml.Behaviors;

namespace Haley.Models
{
    public sealed class BehaviourCollection : List<beh.Behavior> {
        
        public BehaviourCollection() { }
    }
}