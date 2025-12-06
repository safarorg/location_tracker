using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Graphics;
using LocationTracker.Models;

namespace LocationTracker.Controls;

public class HeatMapOverlay
{
    private readonly List<LocationPoint> _locations;
    private readonly int _gridSize;
    private readonly double _radius;

    public HeatMapOverlay(List<LocationPoint> locations, int gridSize = 50, double radius = 0.002)
    {
        _locations = locations;
        _gridSize = Math.Min(gridSize, 10); // Cap at 10 for performance
        _radius = radius;
    }

    public List<Circle> GenerateHeatMapCircles(MapSpan mapSpan)
    {
        var circles = new List<Circle>();
        
        if (_locations.Count == 0)
            return circles;

        var minLat = _locations.Min(l => l.Latitude);
        var maxLat = _locations.Max(l => l.Latitude);
        var minLon = _locations.Min(l => l.Longitude);
        var maxLon = _locations.Max(l => l.Longitude);

        var latStep = (maxLat - minLat) / _gridSize;
        var lonStep = (maxLon - minLon) / _gridSize;

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                var centerLat = minLat + (i * latStep) + (latStep / 2);
                var centerLon = minLon + (j * lonStep) + (lonStep / 2);

                var intensity = CalculateHeatIntensity(centerLat, centerLon);

                if (intensity > 0.1) // Only draw if there's some activity
                {
                    var color = GetHeatColor(intensity);
                    var circle = CreateHeatCircle(centerLat, centerLon, latStep, lonStep, color);
                    if (circle != null)
                        circles.Add(circle);
                }
            }
        }

        return circles;
    }

    private double CalculateHeatIntensity(double latitude, double longitude)
    {
        double intensity = 0;

        foreach (var location in _locations)
        {
            var distance = Math.Sqrt(
                Math.Pow(latitude - location.Latitude, 2) +
                Math.Pow(longitude - location.Longitude, 2));

            if (distance < _radius)
            {
                // Gaussian-like falloff
                var weight = Math.Exp(-(distance * distance) / (_radius * _radius * 0.5));
                intensity += weight;
            }
        }

        return intensity;
    }

    private Color GetHeatColor(double intensity)
    {
        // All circles are blue
        return Color.FromRgb(0, 0, 255); // Blue
    }

    private Circle CreateHeatCircle(double centerLat, double centerLon, double latStep, double lonStep, Color color)
    {
        // Calculate radius based on grid step size (convert degrees to meters approximately)
        // Make circles much smaller so distances between nearby points are visible
        var radiusMeters = Math.Max(latStep, lonStep) * 111000 * 0.3; // Reduce to 30% of original size
        
        var circle = new Circle
        {
            Center = new Location(centerLat, centerLon),
            Radius = new Distance(radiusMeters),
            StrokeColor = Colors.Transparent,
            FillColor = color.WithAlpha(0.5f) // Slightly more transparent
        };
        
        return circle;
    }
}
