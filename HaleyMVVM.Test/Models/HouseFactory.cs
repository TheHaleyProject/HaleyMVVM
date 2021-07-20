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
    public class HouseFactory 
    {
        public IContainerFactory factory { get; set; }
        public House house { get; set; }
        public HouseFactory(IContainerFactory _factory,House _house) { factory = _factory;house = _house; }
    }
}
