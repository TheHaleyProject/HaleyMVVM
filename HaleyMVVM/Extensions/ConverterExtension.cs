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