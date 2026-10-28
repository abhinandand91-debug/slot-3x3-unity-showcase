using System.Globalization;

namespace Slot3x3
{
    /// <summary>Formats coin amounts with a space as the thousands separator, e.g. 1 250 000.</summary>
    public static class NumberFormat
    {
        private static readonly NumberFormatInfo Info = new NumberFormatInfo
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        };

        public static string Coins(int value)
        {
            return value.ToString("N0", Info);
        }
    }
}
