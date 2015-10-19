using Lamna.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Lamna.Misc
{
    public class RelativeTimeConverter : IValueConverter
    {
        private const int Minute = 60;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        private const int Year = Day * 365;

        private readonly Dictionary<long, Func<TimeSpan, string>> thresholds = new Dictionary<long, Func<TimeSpan, string>>
        {
            {2, t => "now"},
            {Minute,  t => String.Format("{0} seconds", (int)t.TotalSeconds)},
            {Minute * 2,  t => "a minute"},
            {Hour,  t => String.Format("{0} minutes", (int)t.TotalMinutes)},
            {Hour * 2,  t => "an hour"},
            {Day,  t => String.Format("{0} hours", (int)t.TotalHours)},
            {Day * 2,  t => "tomorrow"},
            {Day * 30,  t => String.Format("{0} days", (int)t.TotalDays)},
            {Day * 60,  t => "next month"},
            {Year,  t => String.Format("{0} months", (int)t.TotalDays / 30)},
            {Year * 2,  t => "this year"},
            {Int64.MaxValue,  t => String.Format("{0} years", (int)t.TotalDays / 365)}
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dateTime = (DateTime)value;
            var difference = dateTime - DataSource.Now;

            return thresholds.First(t => difference.TotalSeconds < t.Key).Value(difference);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }

}
