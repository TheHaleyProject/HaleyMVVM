using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HaleyLR
{
    public class Entry
    {
        public static void Initiate()
        {
            LangUtils.Register("HaleyLR.Properties.Resources");
        }
    }
}
