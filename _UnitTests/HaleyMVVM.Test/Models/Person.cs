using System;
using System.Collections.Generic;
using System.Text;
using HaleyMVVM.Test.Interfaces;

namespace HaleyMVVM.Test.Models
{
    public class Person : IPerson
    {
        public string id { get; set; }
        public string name { get; set; }
        public override bool Equals(object obj)
        {
            return (this.id == ((Person)obj).id);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public Person() { id = Guid.NewGuid().ToString(); }
    }
}
