using Avalonia.ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using System;

namespace AutoCruz.Ui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Target resolution: 960x540 or 1920x1080 scaling
        Width = 960;
        Height = 540;
        MinWidth = 640;
        MinHeight = 360;

        // Enforce aspect ratio (16:9)
        this.GetObservable(ClientSizeProperty).Subscribe(size =>
        {
            double targetRatio = 16.0 / 9.0;
            double currentRatio = size.Width / size.Height;

            if (Math.Abs(currentRatio - targetRatio) > 0.01)
            {
                double newHeight = size.Width / targetRatio;
                double newWidth = size.Height * targetRatio;

                if (currentRatio > targetRatio)
                {
                    Width = newWidth;
                }
                else
                {
                    Height = newHeight;
                }
            }
        });
    }
}