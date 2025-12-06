namespace LocationTracker;

public class App : Application
{
	public App()
	{
		var services = MauiProgram.Services ?? throw new InvalidOperationException("Services not initialized");
		var databaseService = services.GetRequiredService<LocationTracker.Services.DatabaseService>();
		
		MainPage = new MainPage(databaseService);
	}
}

