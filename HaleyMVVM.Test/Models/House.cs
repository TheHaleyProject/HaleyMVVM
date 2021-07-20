using System;
using System.Collections.Generic;
using System.Text;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using HaleyMVVM.Test.Interfaces;


namespace HaleyMVVM.Test.Models
{
    public class House
    {
        public IBaseContainer Container { get; set; }
        public House(IBaseContainer _container) { Container = _container; }
    }
}
