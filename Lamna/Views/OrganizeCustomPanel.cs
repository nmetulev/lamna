using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Lamna.Views
{
    public class OrganizeCustomPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var resultSize = new Size(0, 0);
            var nextItemBuckets = new Dictionary<double, double>();
            var currentPointer = 0d;

            foreach (var child in Children)
            {
                if (!nextItemBuckets.ContainsKey(currentPointer))
                {
                    nextItemBuckets.Add(currentPointer, 0);
                }

                child.Measure(availableSize);

                nextItemBuckets[currentPointer] += child.DesiredSize.Height;

                resultSize.Width = Math.Max(resultSize.Width, currentPointer);
                resultSize.Height = Math.Max(resultSize.Height, nextItemBuckets[currentPointer]);

                currentPointer += child.DesiredSize.Width;

                if ((currentPointer + child.DesiredSize.Width) > availableSize.Width)
                {
                    currentPointer = 0;
                }
            }

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? resultSize.Width : availableSize.Width;
            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? resultSize.Height : availableSize.Height;

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!this.Children.Any())
            {
                return finalSize;
            }

            var nextItemBuckets = new Dictionary<double, double>();
            var currentPointer = 0d;

            foreach (var child in this.Children)
            {
                if (!nextItemBuckets.ContainsKey(currentPointer))
                {
                    // Adding an 12 pixel top margin so it looks equal
                    nextItemBuckets.Add(currentPointer, 12);
                }

                child.Arrange(new Rect(currentPointer, nextItemBuckets[currentPointer], child.DesiredSize.Width, child.DesiredSize.Height));

                nextItemBuckets[currentPointer] += child.DesiredSize.Height;
                currentPointer += child.DesiredSize.Width;

                if ((currentPointer + child.DesiredSize.Width) > finalSize.Width)
                {
                    currentPointer = 0;
                }
            }

            return new Size(finalSize.Width, nextItemBuckets[0]);
        }
    }
}
