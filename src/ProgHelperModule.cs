﻿using System;
using Celeste.Mod.ArchieCollab2024Helper;

namespace Celeste.Mod.ProgHelper;

public class ProgHelperModule : EverestModule {
    public static ProgHelperModule Instance { get; private set; }

    public static ProgHelperSession Session => (ProgHelperSession) Instance._Session;

    public override Type SessionType => typeof(ProgHelperSession);

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
    }

    public override void Unload() {
        ActorExtensions.Unload();
        BackgroundTilesExtensions.Load();
        PlayerExtensions.Unload();
        SolidTilesExtensions.Load();
        TileGridExtensions.Load();
    }
}