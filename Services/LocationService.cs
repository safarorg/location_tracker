using Microsoft.Maui.ApplicationModel;
using LocationTracker.Models;

namespace LocationTracker.Services;

public class LocationService
{
    private CancellationTokenSource? _cancelTokenSource;
    private bool _isTracking = false;
    private readonly DatabaseService _databaseService;

    public LocationService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public bool IsTracking => _isTracking;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        
        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied)
            return false;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    public async Task StartTrackingAsync(int intervalSeconds = 30)
    {
        if (_isTracking)
            return;

        var hasPermission = await RequestPermissionAsync();
        if (!hasPermission)
        {
            throw new Exception("Location permission not granted");
        }

        _isTracking = true;
        _cancelTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_cancelTokenSource!.Token.IsCancellationRequested && _isTracking)
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(10)
                        });

                    if (location != null)
                    {
                        var locationPoint = new LocationPoint
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            Timestamp = DateTime.Now,
                            Accuracy = location.Accuracy,
                            Altitude = location.Altitude
                        };

                        await _databaseService.SaveLocationAsync(locationPoint);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), _cancelTokenSource.Token);
            }
        });
    }

    public void StopTracking()
    {
        _isTracking = false;
        _cancelTokenSource?.Cancel();
    }
}

