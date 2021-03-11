using Haley.MVVM;
using System;
using System.Collections.Generic;
using System.Text;
using Haley.IOC;
using Haley.Models;

namespace HaleyMVVM.Test.Models
{
    /// <summary>
    /// SuperHero is an extension of a Person who has a secret identity
    /// </summary>
    public class SuperHero : Person 
    {
        [HaleyInject]
        public string power { get; set; }
        public string alter_ego { get; set; }
    }
}
