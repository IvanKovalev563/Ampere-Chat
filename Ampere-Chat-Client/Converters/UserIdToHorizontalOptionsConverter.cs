using System.Globalization;

namespace Ampere_Chat_Client.Converters
{
    public class UserIdToHorizontalOptionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            if (value is int userId && userId == currentUserId)
                return LayoutOptions.End;
            return LayoutOptions.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}