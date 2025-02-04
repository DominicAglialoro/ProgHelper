using System;
using MonoMod.ModInterop;

namespace Celeste.Mod.ProgHelper;

[ModImportName("GravityHelper")]
public static class GravityHelperImports {
    public static Action<int, float> SetPlayerGravity;
}