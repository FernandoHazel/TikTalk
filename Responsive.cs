using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace TikTalk
{
    public class Responsive
    {
        double screenWidth;
        double screenHeight;
        public Responsive() 
        {
            ReadDeviceDisplay();
        }
        private void ReadDeviceDisplay()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"Pixel width: {DeviceDisplay.Current.MainDisplayInfo.Width} / Pixel Height: {DeviceDisplay.Current.MainDisplayInfo.Height}");
            screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height;

            /*
            sb.AppendLine($"Density: {DeviceDisplay.Current.MainDisplayInfo.Density}");
            sb.AppendLine($"Orientation: {DeviceDisplay.Current.MainDisplayInfo.Orientation}");
            sb.AppendLine($"Rotation: {DeviceDisplay.Current.MainDisplayInfo.Rotation}");
            sb.AppendLine($"Refresh Rate: {DeviceDisplay.Current.MainDisplayInfo.RefreshRate}");
            */

            string screenInfo = sb.ToString();
        }

        public RowDefinitionCollection rows()
        {
            //Depending on certain screen height de row will adjust size
            RowDefinitionCollection rows;

            if( screenHeight < 2000)
            {
                rows = new RowDefinitionCollection()
                {
                    new RowDefinition { Height = 90},
                    new RowDefinition { Height = GridLength.Auto},
                    new RowDefinition { Height = 90}
                };
            }
            else
            {
                rows = new RowDefinitionCollection()
                {
                    new RowDefinition { Height = 190},
                    new RowDefinition { Height = GridLength.Auto},
                    new RowDefinition { Height = 190}
                };
            }

            return rows;
        }

        public ColumnDefinitionCollection colums()
        {
            //Depending on certain screen width de row will adjust size
            ColumnDefinitionCollection colums;

            colums = new ColumnDefinitionCollection()
            {
                new ColumnDefinition { Width = GridLength.Star},
                new ColumnDefinition { Width = GridLength.Auto},
                new ColumnDefinition { Width = GridLength.Star}
            };

            return colums;
        }

        public double spacing()
        {
            //Depending on certain screen height de spacing will adjust
            double spacing;
            if (screenHeight < 2000)
            {
                spacing = 50;
            }
            else
            {
                spacing = 70;
            }

            return spacing;
        }

        public RowDefinitionCollection qrRows()
        {
            RowDefinitionCollection qrRows;

            if (screenHeight < 2000)
            {
                qrRows = new RowDefinitionCollection()
                {
                    new RowDefinition { Height = 20},
                    new RowDefinition { Height = GridLength.Auto},
                    new RowDefinition { Height = 20}
                };
            }
            else
            {
                qrRows = new RowDefinitionCollection()
                {
                    new RowDefinition { Height = 150},
                    new RowDefinition { Height = GridLength.Auto},
                    new RowDefinition { Height = 150}
                };
            }

            return qrRows;
        }

        public ColumnDefinitionCollection qrColums()
        {
            ColumnDefinitionCollection qrColums;

            qrColums = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Auto},
                    new ColumnDefinition { Width = GridLength.Star}
                };

            return qrColums;
        }

        public double qrSpacing()
        {
            double spacing = 20;
            //...

            return spacing;
        }
    }
}
