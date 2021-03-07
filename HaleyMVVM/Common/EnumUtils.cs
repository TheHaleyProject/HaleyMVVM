using System;
using System.Linq;

namespace Haley.Enums
{
    public enum ResizeAffectMode
    {
        PixelOnly,
        PixelAndDPI,
        PixelAndImage,
    }

    public enum DialogMode
    {
        Notification,
        Confirmation,
        GetInput
    }

    public enum RGB
    {
        Blue,
        Green,
        Red
    }

    public enum InputConstraintType
    {
        All,
        TextOnly,
        Double,
        Integer,
    }

}
