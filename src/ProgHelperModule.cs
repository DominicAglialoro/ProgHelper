using System;

namespace Celeste.Mod.ProgHelper;

public class ProgHelperModule : EverestModule {
    public static ProgHelperModule Instance { get; private set; }

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
        PlayerExtensions.Load();
    }

    public override void Unload() {
        ActorExtensions.Unload();
        PlayerExtensions.Unload();
    }
}