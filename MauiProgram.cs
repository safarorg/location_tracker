using LocationTracker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LocationTracker;

public static class MauiProgram
{
	public static IServiceProvider? Services { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		// Register services
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<LocationService>(sp => 
			new LocationService(sp.GetRequiredService<DatabaseService>()));

		var app = builder.Build();
		Services = app.Services;
		return app;
	}
}

