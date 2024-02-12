using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.ProgHelper; 

public static class PlatformExtensions {
    public static void Load() {
        IL.Celeste.Platform.Update += Platform_Update_il;
        IL.Celeste.Platform.MoveH_float += PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveH_float_float += PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveV_float += PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveV_float_float += PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHNaive += PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVNaive += PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHCollideSolids += PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVCollideSolids += PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHCollideSolidsAndBounds += PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3 += PatchLiftspeedProtectionY;
    }

    public static void Unload() {
        IL.Celeste.Platform.Update -= Platform_Update_il;
        IL.Celeste.Platform.MoveH_float -= PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveH_float_float -= PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveV_float -= PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveV_float_float -= PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHNaive -= PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVNaive -= PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHCollideSolids -= PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVCollideSolids -= PatchLiftspeedProtectionY;
        IL.Celeste.Platform.MoveHCollideSolidsAndBounds -= PatchLiftspeedProtectionX;
        IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3 -= PatchLiftspeedProtectionY;
    }

    private static void SetZeroLiftspeed(Platform platform) {
        if (ProgHelperModule.Session.LiftboostProtection)
            platform.LiftSpeed = Vector2.Zero;
    }
    
    private static Vector2 GetZeroLiftspeed(Vector2 value, Platform platform)
        => ProgHelperModule.Session.LiftboostProtection ? platform.LiftSpeed : value;

    private static float GetNewLiftspeedX(float value, Platform platform)
        => ProgHelperModule.Session.LiftboostProtection && value == 0f ? platform.LiftSpeed.X : value;
    
    private static float GetNewLiftspeedY(float value, Platform platform)
        => ProgHelperModule.Session.LiftboostProtection && value == 0f ? platform.LiftSpeed.Y : value;

    private static void Platform_Update_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(SetZeroLiftspeed)));

        cursor.GotoNext(instr => instr.MatchStfld<Platform>("LiftSpeed"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(GetZeroLiftspeed)));
    }

    private static void PatchLiftspeedProtectionX(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("X"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(GetNewLiftspeedX)));
        }
    }
    
    private static void PatchLiftspeedProtectionY(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("Y"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(GetNewLiftspeedY)));
        }
    }
}