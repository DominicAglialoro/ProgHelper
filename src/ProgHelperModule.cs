using System;

namespace Celeste.Mod.ProgHelper;

public class ProgHelperModule : EverestModule {
    public static ProgHelperModule Instance { get; private set; }

    public override Type SessionType => typeof(RushHelperSession);
    public static RushHelperSession Session => (RushHelperSession) Instance._Session;

    public ProgHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ProgHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(HeavenRushModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        ActorExtensions.Load();
        InputExtensions.Load();
        PlayerExtensions.Load();
    }

    public override void Unload() {
        ActorExtensions.Unload();
        InputExtensions.Unload();
        PlayerExtensions.Unload();
    }
}