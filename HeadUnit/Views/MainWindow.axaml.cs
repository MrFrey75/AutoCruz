using Avalonia;
using Avalonia.Controls;
using System;

namespace HeadUnit.Views;

public partial class MainWindow : Window
{
    private const double AspectRatio = 16.0 / 9.0;
    private bool _isResizing = false;

    public MainWindow()
    {
        InitializeComponent();
        
        // Subscribe to size change events to maintain aspect ratio
        this.GetObservable(BoundsProperty).Subscribe(OnBoundsChanged);
    }

    private void OnBoundsChanged(Rect bounds)
    {
        if (_isResizing || bounds.Width == 0 || bounds.Height == 0)
            return;

        _isResizing = true;

        try
        {
            var currentAspectRatio = bounds.Width / bounds.Height;
            
            // Allow some tolerance for aspect ratio
            if (Math.Abs(currentAspectRatio - AspectRatio) > 0.01)
            {
                // Determine which dimension to adjust based on which changed more
                var targetWidth = bounds.Height * AspectRatio;
                var targetHeight = bounds.Width / AspectRatio;
                
                // Prefer adjusting height to maintain width when possible
                if (targetHeight >= MinHeight && targetHeight <= MaxHeight)
                {
                    Height = targetHeight;
                }
                else if (targetWidth >= MinWidth && targetWidth <= MaxWidth)
                {
                    Width = targetWidth;
                }
            }
        }
        finally
        {
            _isResizing = false;
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Ensure we start with correct aspect ratio
        if (Height > 0)
        {
            Width = Height * AspectRatio;
        }
    }
}