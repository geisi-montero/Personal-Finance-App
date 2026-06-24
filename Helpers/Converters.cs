using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FinanzasApp.Models;

namespace FinanzasApp.Helpers
{
    public class IndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string p)
            {
                // Handle int index comparison
                if (value is int intVal && int.TryParse(p, out var idx))
                    return intVal == idx;
                // Handle string comparison
                if (value is string strVal)
                    return strVal == p;
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter is string p)
            {
                if (int.TryParse(p, out var idx)) return idx;
                return p;
            }
            return Binding.DoNothing;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? Visibility.Visible : Visibility.Collapsed;
            if (value is string s) return !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
            if (value is int i) return i > 0 ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v == Visibility.Visible;
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b ? Visibility.Visible : Visibility.Collapsed;
            if (value is int i) return i == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (value is string s) return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType t)
                return t == TransactionType.Income
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class TransactionTypeToSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is TransactionType t && t == TransactionType.Income ? "+" : "-";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class AmountFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d) return d.ToString("N2", new CultureInfo("es-DO"));
            if (value is double dbl) return dbl.ToString("N2", new CultureInfo("es-DO"));
            return "0.00";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class HexColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string hex && !string.IsNullOrEmpty(hex))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch { }
            return new SolidColorBrush(Color.FromRgb(99, 102, 241));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class BalanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return d >= 0
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double pct = 0;
                if (value is double d) pct = d;
                else if (value is decimal dec) pct = (double)dec;
                else if (value is int i) pct = i;
                else if (value != null && double.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) pct = parsed;

                double max = 200;
                if (parameter is string s && double.TryParse(s, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var m))
                    max = m;

                double result = Math.Max(0, Math.Min(pct / 100.0 * max, max));
                return double.IsNaN(result) || double.IsInfinity(result) ? 0.0 : result;
            }
            catch { return 0.0; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
