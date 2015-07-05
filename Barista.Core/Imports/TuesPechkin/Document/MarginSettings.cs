namespace Barista.TuesPechkin
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MarginSettings
    {
        public MarginSettings()
        {
            Unit = TuesPechkin.Unit.Millimeters;
        }

        public MarginSettings(double top, double right, double bottom, double left) : this()
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        public double? Bottom { get; set; }

        public double? Left { get; set; }

        public double? Right { get; set; }

        public double? Top { get; set; }

        [Browsable(false)]
        public double All
        {
            set
            {
                Top = Right = Bottom = Left = value;
            }
        }

        /// <summary>
        /// Defaults to Inches.
        /// </summary>
        public Unit Unit { get; set; }
    }
}