using System.Windows.Markup;
using System;
using System.Windows;

//[assembly: Guid("FECEB4DE-F5CA-4CB0-A18A-C48FAF964F38")]
//[assembly: NeutralResourcesLanguage("en-GB")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsPrefix("http://schemas.hpod9.com/haley/mvvm", "hly")]
//FOR XAML NAMESPACES - MVVM
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.Enums")]
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.MVVM.Converters")]
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.Abstractions")]
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.Models")]
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.MVVM")]
[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/mvvm", "Haley.Utils")]
//FOR XAML NAMESPACES - WPF
//[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/wpf", "Haley.WPF.ViewModels")]
//[assembly: XmlnsDefinition("http://schemas.hpod9.com/haley/wpf", "Haley.WPF.Views")]
