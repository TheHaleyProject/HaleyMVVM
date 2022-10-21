using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Haley.Models;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Conv=Haley.MVVM.Converters;
using System.Windows.Markup;

namespace Haley.Utils
{
    public class ConverterExtension : MarkupExtension {

        private StaticResourceExtension _internalExtension;
        private ConverterKind _resourceKey = ConverterKind.Verification;
        public ConverterExtension(ConverterKind kind) {
            _resourceKey = kind;
            _internalExtension = new StaticResourceExtension(GetConverterName());
        }
        public ConverterExtension() { }

        [ConstructorArgument("kind")]
        public ConverterKind ResourceKey {
            get { return _resourceKey; }
            set { _resourceKey = value; }
        }
        public string GetConverterClassName() {
            switch (ResourceKey) {
                case ConverterKind.BooltoVisibility:
                    return nameof(Conv.BoolToVisibilityConverter);
                case ConverterKind.ColorToBrush:
                    return nameof(Conv.ColorToBrushConverter);
                case ConverterKind.EnumListoStringList:
                    return nameof(Conv.EnumListToDescriptionListConverter);
                case ConverterKind.EnumtoDescription:
                    return nameof(Conv.EnumToDescriptionConverter);
                case ConverterKind.EnumTypeToDescriptionList:
                    return nameof(Conv.EnumTypeToDescriptionListConverter);
                case ConverterKind.EnumTypeToValues:
                    return nameof(Conv.EnumTypeToValuesConverter);
                case ConverterKind.EqualityCheck:
                    return nameof(Conv.EqualityCheckConverter);
                case ConverterKind.EqualityToVisibility:
                    return nameof(Conv.EqualityCheckToVisibilityConverter);
                case ConverterKind.HalfValue:
                    return nameof(Conv.HalfValueConverter);
                case ConverterKind.InverseBoolean:
                    return nameof(Conv.InverseBooleanConverter);
                case ConverterKind.KeytoControl:
                    return nameof(Conv.KeyToControlConverter);
                case ConverterKind.NegateValue:
                    return nameof(Conv.NegateValueConverter);
                case ConverterKind.LengthReducer:
                    return nameof(Conv.ReducerConverter);
                case ConverterKind.Verification:
                    return nameof(Conv.VerificationConverter);
                case ConverterKind.MultiBinder:
                    return nameof(Conv.MultiValueBinderConverter);
                case ConverterKind.MultiBindingEqualityCheck:
                    return nameof(Conv.MultiBindingEqualityCheckConverter);
            }
            return string.Empty;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            return getInternalResource()?.ProvideValue(serviceProvider);
        }

        private string GetConverterName() {
            return ResourceKey.ToString();
        }

        private StaticResourceExtension getInternalResource() {
            if (_internalExtension == null) _internalExtension = new StaticResourceExtension(GetConverterName());
                return _internalExtension;
        }
    }
}