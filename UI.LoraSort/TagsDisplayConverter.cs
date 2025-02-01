using System.Globalization;
using System.Windows.Data;

namespace UI.LoraSort
{
    /// <summary>
    /// Converts a List of tags into a friendly display string.
    /// It takes the first three tags and, for each tag longer than 10 characters,
    /// trims it to the first 10 characters followed by "..." (if needed).
    /// If there are more than three tags, appends ", ..." to the display.
    /// </summary>
    public class TagsDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> tags)
            {
                var tagList = tags.ToList();
                // Process the first three tags:
                var displayTags = tagList
                    .Take(3)
                    .Select(t => t.Length > 10 ? t.Substring(0, 10) + "..." : t)
                    .ToList();

                string result = string.Join(", ", displayTags);

                if (tagList.Count > 3)
                {
                    result += ", ...";
                }
                return result;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for one–way binding.
            throw new NotImplementedException();
        }
    }
}
