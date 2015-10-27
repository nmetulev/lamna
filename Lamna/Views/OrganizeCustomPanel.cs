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

            if (!this.Children.Any())
            {
                return resultSize;
            }

            double y = 0;
            double x = 0;

            double maxX = 0;

            foreach (var child in Children)
            {

                if (x + child.DesiredSize.Width > availableSize.Width)
                {
                    x = 0;
                    y += child.DesiredSize.Height;
                }

                child.Measure(availableSize);

                x += child.DesiredSize.Width;

                maxX = Math.Max(x, maxX);
                
                
            }

            resultSize.Width = maxX;
            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? y + Children.Last().DesiredSize.Height : availableSize.Height;

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!this.Children.Any())
            {
                return finalSize;
            }
            
            double y = 0;
            double x = 0;

            foreach (var child in this.Children)
            {
                if (x + child.DesiredSize.Width > finalSize.Width)
                {
                    x = 0;
                    y += child.DesiredSize.Height;
                }


                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));

                x += child.DesiredSize.Width;
            }

            return new Size(finalSize.Width, y + Children.Last().DesiredSize.Height);
        }
    }
}
