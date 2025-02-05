using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace UI.LoraSort
{
    public class IndexToPriorityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Expecting two values:
            // values[0] = the current item
            // values[1] = the collection (ItemsSource)
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return null;

            var currentItem = values[0];
            var collection = values[1] as IEnumerable;
            if (collection == null)
                return null;

            int index = 0;
            foreach (var item in collection)
            {
                if (item == currentItem)
                {
                    // Return a 1-based index if desired:
                    return (index + 1).ToString();
                    // Otherwise, just return index.ToString();
                }
                index++;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
