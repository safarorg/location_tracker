using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationTracker.Models;
using LocationTracker.Services;
using LocationTracker.Controls;

namespace LocationTracker;

public class MainPage : ContentPage
{
    private Microsoft.Maui.Controls.Maps.Map _map;
    private Button _addButton;
    private Button _clearButton;
    private Entry _latitudeEntry;
    private Entry _longitudeEntry;
    private Label _statusLabel;
    private Label _countLabel;
    private List<Circle> _heatMapCircles = new();

    private readonly DatabaseService _databaseService;
    private System.Timers.Timer? _updateTimer;

    public MainPage(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        Title = "Location Tracker";
        BuildUI();
        
        // Initialize synchronously on UI thread to ensure UI is ready
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        try
        {
            StartUpdateTimer();
            await LoadDefaultCoordinates();
            await LoadExistingLocations();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private void BuildUI()
    {
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Header with title and status
        var headerGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = new Thickness(20, 15),
            BackgroundColor = Colors.White
        };

        var titleLabel = new Label
        {
            Text = "Location Tracker",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var statusRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        _statusLabel = new Label
        {
            Text = "Ready to track",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.Black
        };

        var statusIcon = new Label
        {
            Text = "✓",
            FontSize = 16,
            TextColor = Color.FromRgb(76, 175, 80),
            Margin = new Thickness(5, 0, 0, 0)
        };

        var statusContainer = new HorizontalStackLayout
        {
            Children = { _statusLabel, statusIcon },
            VerticalOptions = LayoutOptions.Center
        };

        _countLabel = new Label
        {
            Text = "Locations: 0",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            TextColor = Colors.Black
        };

        statusRow.Add(statusContainer, 0, 0);
        statusRow.Add(_countLabel, 1, 0);

        headerGrid.Add(titleLabel, 0, 0);
        headerGrid.Add(statusRow, 0, 1);

        // Map - initialize with default location so it's visible immediately
        _map = new Microsoft.Maui.Controls.Maps.Map
        {
            MapType = MapType.Street
        };
        
        // Set initial map region so map is visible immediately
        _map.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(37.7749, -122.4194),
            Distance.FromKilometers(10)));

        // Input and control buttons
        var inputGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(20, 15),
            ColumnSpacing = 10,
            BackgroundColor = Colors.White
        };

        var latLabel = new Label
        {
            Text = "Latitude:",
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.Black
        };

        _latitudeEntry = new Entry
        {
            Placeholder = "37.7749",
            Keyboard = Keyboard.Numeric,
            FontSize = 14
        };

        var lonLabel = new Label
        {
            Text = "Longitude:",
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.Black
        };

        _longitudeEntry = new Entry
        {
            Placeholder = "-122.4194",
            Keyboard = Keyboard.Numeric,
            FontSize = 14
        };

        _addButton = new Button
        {
            Text = "Add Location",
            BackgroundColor = Color.FromRgb(76, 175, 80),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = 14,
            Padding = new Thickness(15, 10)
        };
        _addButton.Clicked += OnAddButtonClicked;

        inputGrid.Add(latLabel, 0, 0);
        inputGrid.Add(_latitudeEntry, 1, 0);
        inputGrid.Add(lonLabel, 2, 0);
        inputGrid.Add(_longitudeEntry, 3, 0);
        inputGrid.Add(_addButton, 4, 0);

        // Control buttons
        var buttonGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star }
            },
            Padding = new Thickness(20, 0, 20, 15),
            BackgroundColor = Colors.White
        };

        _clearButton = new Button
        {
            Text = "Clear Data",
            BackgroundColor = Color.FromRgb(244, 67, 54),
            TextColor = Colors.White,
            CornerRadius = 8,
            FontSize = 16,
            Padding = new Thickness(0, 12)
        };
        _clearButton.Clicked += OnClearButtonClicked;

        buttonGrid.Add(_clearButton, 0, 0);

        mainGrid.Add(headerGrid, 0, 0);
        mainGrid.Add(_map, 0, 1);
        
        // Bottom section with inputs and buttons
        var bottomGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        bottomGrid.Add(inputGrid, 0, 0);
        bottomGrid.Add(buttonGrid, 0, 1);
        
        mainGrid.Add(bottomGrid, 0, 2);

        Content = mainGrid;
    }

    private async Task LoadDefaultCoordinates()
    {
        var existing = await _databaseService.GetAllLocationsAsync();
        
        // Always clear and reload defaults to ensure fresh data
        if (existing.Count > 0)
        {
            await _databaseService.DeleteAllLocationsAsync();
        }
        
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _statusLabel.Text = "Loading default coordinates...";
        });
        
        // Sample coordinates around San Francisco area for a nice heat map
        // Creating clusters for better heat map visualization
        var defaultCoords = new[]
        {
            // Cluster 1 - Downtown SF (high density)
            new LocationPoint { Latitude = 37.7749, Longitude = -122.4194, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7750, Longitude = -122.4195, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7751, Longitude = -122.4193, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7748, Longitude = -122.4196, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7752, Longitude = -122.4192, Timestamp = DateTime.Now },
            
            // Cluster 2 - Mission District (medium density)
            new LocationPoint { Latitude = 37.7599, Longitude = -122.4148, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7600, Longitude = -122.4149, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7598, Longitude = -122.4147, Timestamp = DateTime.Now },
            
            // Cluster 3 - Marina District (medium density)
            new LocationPoint { Latitude = 37.8024, Longitude = -122.4058, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.8025, Longitude = -122.4059, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.8023, Longitude = -122.4057, Timestamp = DateTime.Now },
            
            // Cluster 4 - Golden Gate Park area (low density)
            new LocationPoint { Latitude = 37.7694, Longitude = -122.4862, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7695, Longitude = -122.4863, Timestamp = DateTime.Now },
            
            // Cluster 5 - Financial District (high density)
            new LocationPoint { Latitude = 37.7849, Longitude = -122.4094, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7850, Longitude = -122.4095, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7848, Longitude = -122.4093, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7851, Longitude = -122.4092, Timestamp = DateTime.Now },
            
            // Cluster 6 - SOMA (medium density)
            new LocationPoint { Latitude = 37.7749, Longitude = -122.4094, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7750, Longitude = -122.4095, Timestamp = DateTime.Now },
            new LocationPoint { Latitude = 37.7748, Longitude = -122.4093, Timestamp = DateTime.Now },
        };

        foreach (var coord in defaultCoords)
        {
            await _databaseService.SaveLocationAsync(coord);
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _statusLabel.Text = $"Loaded {defaultCoords.Length} default coordinates";
        });
    }

    private async void OnAddButtonClicked(object? sender, EventArgs e)
    {
        if (double.TryParse(_latitudeEntry.Text, out double latitude) &&
            double.TryParse(_longitudeEntry.Text, out double longitude))
        {
            // Validate coordinates
            if (latitude < -90 || latitude > 90)
            {
                await DisplayAlert("Error", "Latitude must be between -90 and 90", "OK");
                return;
            }

            if (longitude < -180 || longitude > 180)
            {
                await DisplayAlert("Error", "Longitude must be between -180 and 180", "OK");
                return;
            }

            var locationPoint = new LocationPoint
            {
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.Now
            };

            await _databaseService.SaveLocationAsync(locationPoint);
            
            // Reload all locations to update pins and heat map
            await LoadExistingLocations();

            // Clear input fields
            _latitudeEntry.Text = string.Empty;
            _longitudeEntry.Text = string.Empty;

            _statusLabel.Text = "Location added";
        }
        else
        {
            await DisplayAlert("Error", "Please enter valid latitude and longitude values", "OK");
        }
    }


    private async Task ShowHeatMap(List<LocationPoint> locations)
    {
        if (locations.Count == 0)
            return;

        // Hide existing heat map first
        HideHeatMap();

        // Use smaller grid for better performance
        var mapSpan = _map.VisibleRegion ?? new MapSpan(new Location(37.7749, -122.4194), 0.1, 0.1);
        var heatMap = new HeatMapOverlay(locations, gridSize: 10, radius: 0.01); // Much smaller grid
        _heatMapCircles = heatMap.GenerateHeatMapCircles(mapSpan);

        // Add circles to map (limit to prevent performance issues)
        int added = 0;
        foreach (var circle in _heatMapCircles)
        {
            if (added++ < 200) // Limit to 200 circles max
            {
                _map.MapElements.Add(circle);
            }
        }
    }

    private void HideHeatMap()
    {
        if (_heatMapCircles == null || _heatMapCircles.Count == 0)
            return;
            
        foreach (var circle in _heatMapCircles)
        {
            _map.MapElements.Remove(circle);
        }
        _heatMapCircles.Clear();
    }

    private async void OnClearButtonClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlert(
            "Clear All Data",
            "Are you sure you want to delete all location data?",
            "Yes",
            "No");

        if (confirmed)
        {
            await _databaseService.DeleteAllLocationsAsync();
            HideHeatMap();
            await UpdateLocationCount();
            _statusLabel.Text = "Data cleared";
        }
    }

    private async Task LoadExistingLocations()
    {
        var locations = await _databaseService.GetAllLocationsAsync();
        
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Clear existing map elements
            HideHeatMap();
            
            if (locations.Count > 0)
            {
                // Calculate center point for all locations
                var avgLat = locations.Average(l => l.Latitude);
                var avgLon = locations.Average(l => l.Longitude);
                
                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(avgLat, avgLon),
                    Distance.FromKilometers(15)));

                // Show heat map (no pins)
                await ShowHeatMap(locations);
                
                _statusLabel.Text = $"Showing {locations.Count} locations";
            }
            else
            {
                // Default to San Francisco
                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(37.7749, -122.4194),
                    Distance.FromKilometers(10)));
                _statusLabel.Text = "No locations yet";
            }

            await UpdateLocationCount();
        });
    }

    private async Task UpdateLocationCount()
    {
        var count = await _databaseService.GetLocationCountAsync();
        _countLabel.Text = $"Locations: {count}";
    }

    private void StartUpdateTimer()
    {
        _updateTimer = new System.Timers.Timer(5000); // Update every 5 seconds
        _updateTimer.Elapsed += async (s, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await UpdateLocationCount();
            });
        };
        _updateTimer.Start();
    }
}
