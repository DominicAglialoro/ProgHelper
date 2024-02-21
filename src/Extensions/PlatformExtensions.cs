using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.ProgHelper; 

public static class PlatformExtensions {
    public static void Load() {
        On.Celeste.Platform.Update += Platform_Update;
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
        On.Celeste.Platform.Update -= Platform_Update;
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

    private static float SetSafeLiftSpeedX(float value, Platform platform) {
        if (!ProgHelperModule.Session.LiftboostProtection || value == 0f)
            return value;
        
        var dynamicData = DynamicData.For(platform);
        var safeLiftSpeed = dynamicData.Get<Vector2?>("safeLiftSpeed") ?? Vector2.Zero;

        safeLiftSpeed.X = value;
        dynamicData.Set("safeLiftSpeed", safeLiftSpeed);

        return value;
    }

    private static float SetSafeLiftSpeedY(float value, Platform platform) {
        if (!ProgHelperModule.Session.LiftboostProtection || value == 0f)
            return value;
        
        var dynamicData = DynamicData.For(platform);
        var safeLiftSpeed = dynamicData.Get<Vector2?>("safeLiftSpeed") ?? Vector2.Zero;

        safeLiftSpeed.Y = value;
        dynamicData.Set("safeLiftSpeed", safeLiftSpeed);

        return value;
    }

    private static void Platform_Update(On.Celeste.Platform.orig_Update update, Platform platform) {
        if (ProgHelperModule.Session.LiftboostProtection)
            DynamicData.For(platform).Set("safeLiftSpeed", Vector2.Zero);

        update(platform);
    }

    private static void PatchLiftspeedProtectionX(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("X"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(SetSafeLiftSpeedX)));
        }
    }
    
    private static void PatchLiftspeedProtectionY(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("Y"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(PlatformExtensions).GetMethodUnconstrained(nameof(SetSafeLiftSpeedY)));
        }
    }
}