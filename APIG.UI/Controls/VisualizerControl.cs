using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace APIG.UI.Controls;

public class VisualizerControl : Control
{
    public static readonly StyledProperty<float[]> CurrentFFTsProperty = AvaloniaProperty.Register<VisualizerControl, float[]>(
        nameof(CurrentFFTs), new float[4096]);

    public float[] CurrentFFTs
    {
        get => GetValue(CurrentFFTsProperty);
        set => SetValue(CurrentFFTsProperty, value);
    }

    public static readonly StyledProperty<float[]> CurrentMaxProperty = AvaloniaProperty.Register<VisualizerControl, float[]>(
        nameof(CurrentMax), new float[4096]);

    public float[] CurrentMax
    {
        get => GetValue(CurrentMaxProperty);
        set => SetValue(CurrentMaxProperty, value);
    }
    
    static VisualizerControl()
    {
        AffectsRender<VisualizerControl>(CurrentFFTsProperty);
        AffectsRender<VisualizerControl>(CurrentMaxProperty);
    }

    public VisualizerControl()
    {
    }

    public override void Render(DrawingContext context)
    {
        if (!IsVisible)
            return;
        //draw all ffts respectively to the max and the bounds using StreamGeometry, with rounded corners
        var fftsToUse = CurrentFFTs;
        var maxToUse = CurrentMax;

        if (Bounds.Width < fftsToUse.Length)
        {
            //find nearest power of 2 value to the width
            var nearestPowerOf2 = (int) MathF.Log2((float) Bounds.Width);
            var nearestPowerOf2Value = (int) MathF.Pow(2, nearestPowerOf2);
            
            //compress the ffts to the nearest power of 2 value
            var compressedFfts = new float[nearestPowerOf2Value];
            var compressedMax = new float[nearestPowerOf2Value];
            
            var fftsPerValue = fftsToUse.Length / nearestPowerOf2Value;
            for (var i = 0; i < nearestPowerOf2Value; i++)
            {
                var ffts = new float[fftsPerValue];
                var max = new float[fftsPerValue];
                Array.Copy(fftsToUse, i * fftsPerValue, ffts, 0, fftsPerValue);
                Array.Copy(maxToUse, i * fftsPerValue, max, 0, fftsPerValue);
                compressedFfts[i] = ffts.Average();
                compressedMax[i] = max.Average();
            }
            
            fftsToUse = compressedFfts;
            maxToUse = compressedMax;
        }
        
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(0, Bounds.Height), true);
            for (int i = 0; i < fftsToUse.Length; i++)
            {
                var x = (Bounds.Width / fftsToUse.Length) * i;
                var y = Bounds.Height - (Bounds.Height / maxToUse[i]) * fftsToUse[i];
                ctx.LineTo(new Point(x, y));
            }
            ctx.LineTo(new Point(Bounds.Width, Bounds.Height));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(Brushes.White, new Pen(Brushes.White, 1), geometry);
        
    }
}