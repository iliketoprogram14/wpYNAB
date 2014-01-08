using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace YNABv1
{
    public partial class Help : PhoneApplicationPage
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Quick_Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform transform = QuickTextBlock.TransformToVisual(Scroller);
            Point position = transform.Transform(new Point(0, 0));
            Scroller.ScrollToVerticalOffset(position.Y);
        }

        private void Full_Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform transform = FullTextBlock.TransformToVisual(Scroller);
            Point position = transform.Transform(new Point(0, 0));
            Scroller.ScrollToVerticalOffset(position.Y);
        }

        private void FAQ_Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralTransform transform = FAQTextBlock.TransformToVisual(Scroller);
            Point position = transform.Transform(new Point(0, 0));
            Scroller.ScrollToVerticalOffset(position.Y);
        }
    }
}