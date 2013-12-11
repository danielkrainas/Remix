namespace Atlana
{
    using System;
    using Atlana.Engine;
    using Atlana.Log;
    using Atlana.Configuration;

    class Program
    {
        public static void Main(string[] args)
        {
            Logger.Info("Starting up...");
            GameEngine engine = GameEngine.Instance;

            Logger.Info("Initializing Game Engine...");
            if (!engine.Initialize())
            {
                Logger.Info("GameEngine.Initialize: error initializing");
            }
            else
            {
                Logger.Info("Running {0}({2}) on Port {1}", SettingsManager.MudName, SettingsManager.Port, SettingsManager.MudVersion);
                if (!engine.Run())
                {
                    Logger.Bug("GameEngine.Run: error while running game");
                }
            }

            if (!engine.Shutdown())
            {
                Logger.Bug("GameEngine.Shutdown: error shutting down game");
            }

            Logger.Info("Shutdown complete!");
        }
    }
}