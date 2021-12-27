using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Haley.Abstractions;
using System.Globalization;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using System.Reflection;
using System.Resources;
using Haley.Events;

namespace Haley.Models
{
    public delegate object TranslationOverrideCallBack(string key,string value,CultureInfo cultureInfo); //A delegate to be called (if subscribed) when the culture changes.
        
    public class ResourceProvider 
    {
        public string Id { get; }
        public Assembly Source { get;}
        public string Key { get; internal set; }
        public ResourceManager  Manager { get; }
        public TranslationOverrideCallBack TranslationOverride { get; internal set; }
        public override bool Equals(object obj)
        {
            return (Id == ((ResourceProvider)obj).Id);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public ResourceProvider(Assembly _source, ResourceManager _manager, TranslationOverrideCallBack _overrideCallback) 
        {
            Source = _source;
            Manager = _manager;
            TranslationOverride = _overrideCallback;
            Id = Guid.NewGuid().ToString(); 
        }
    }
   
}
