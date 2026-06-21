using System.Globalization;

namespace Ampere_Chat_Client.Converters
{
    public class UserIdToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            if (value is int userId && userId == currentUserId)
                return Colors.DodgerBlue; // твои сообщения
            return Colors.LightGray;      // чужие
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}