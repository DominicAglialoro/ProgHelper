﻿namespace Celeste.Mod.ProgHelper;

public class ProgHelperSession : EverestModuleSession {
    public float TilePulseBaseBrightness { get; set; } = 1f;
    public float TilePulseInterval { get; set; } = 1f;
    public float TilePulseLength { get; set; } = 1f;
    public float TilePulseStep { get; set; }
}