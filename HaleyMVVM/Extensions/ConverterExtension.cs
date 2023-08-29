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
        private ConverterKind _kind = ConverterKind.Verification;
        public ConverterExtension(ConverterKind kind) {
            _kind = kind;
            _internalExtension = new StaticResourceExtension(GetConverterName());
        }
        public ConverterExtension() { }

        [ConstructorArgument("kind")]
        public ConverterKind Kind {
            get { return _kind; }
            set { _kind = value; }
        }
        public void EmptyConverterLists() {
            List<string> converterslist = new List<string>() {
                nameof(Conv.BoolToVisibilityConverter),
                nameof(Conv.ColorToBrushConverter),
                nameof(Conv.EnumToDescriptionConverter),
                nameof(Conv.EnumListToDescriptionListConverter),
                nameof(Conv.EnumTypeToDescriptionListConverter),
                nameof(Conv.EnumTypeToValuesConverter),
                nameof(Conv.EqualityCheckConverter),
                nameof(Conv.EqualityCheckToVisibilityConverter),
                nameof(Conv.EqualityCheckToVisibilityConverter),
                nameof(Conv.HalfValueConverter),
                nameof(Conv.InverseBooleanConverter),
                nameof(Conv.KeyToControlConverter),
                nameof(Conv.NegateValueConverter),
                nameof(Conv.ReducerConverter),
                nameof(Conv.VerificationConverter),
                nameof(Conv.MultiValueBinderConverter),
                nameof(Conv.MultiBindingEqualityCheckConverter),
                nameof(Conv.NullCheckerConverter),
                };
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            return getInternalResource()?.ProvideValue(serviceProvider);
        }

        private string GetConverterName() {
            return Kind.ToString();
        }

        private StaticResourceExtension getInternalResource() {
            if (_internalExtension == null) _internalExtension = new StaticResourceExtension(GetConverterName());
                return _internalExtension;
        }
    }
}