using System.Globalization;

namespace Ampere_Chat_Client.Converters
{
    public class UserIdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            var isOwn = value is int userId && userId == currentUserId;
            var showOwn = parameter?.ToString() == "own";
            return (isOwn == showOwn) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}