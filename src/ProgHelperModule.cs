using System;

namespace Celeste.Mod.ProgHelper;

public class ProgHelperModule : EverestModule {
    public static ProgHelperModule Instance { get; private set; }

    public static ProgHelperSession Session => (ProgHelperSession) Instance._Session;

    public override Type SessionType => typeof(ProgHelperSession);

    public bool VivHelperLoaded { get; private set; }

    public ProgHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ProgHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ProgHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        ActorExtensions.Load();
        BackgroundTilesExtensions.Load();
        PlayerExtensions.Load();
        SolidTilesExtensions.Load();
        TileGridExtensions.Load();

        VivHelperLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata {
            Name = "VivHelper",
            Version = new Version(1, 14, 2)
        });
    }

    public override void Unload() {
        ActorExtensions.Unload();
        BackgroundTilesExtensions.Unload();
        PlayerExtensions.Unload();
        SolidTilesExtensions.Unload();
        TileGridExtensions.Unload();
    }
}