using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        string _fileName = Path.Combine(FileSystem.AppDataDirectory, "notes.txt");
        public const double MyFontSize = 28;

        public MainPage()
        {
            InitializeComponent();

            if (File.Exists(_fileName))
            {
                MyStackLayout.Padding =
    DeviceInfo.Platform == DevicePlatform.iOS
        ? new Thickness(30, 60, 30, 30) // Shift down by 60 points on iOS only
        : new Thickness(30); // Set the default margin to be 30 points
                UploadGrid.IsVisible = false;
            }
        }

        public async Task<FileResult> PickFractionResourceFile(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select a json file with data about your fraction"
            };

            await PickFractionResourceFile(options);
        }
    }

}
