using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ProgHelper;

public static class Util {
    private const BindingFlags ALL_FLAGS = BindingFlags.Instance |
                                           BindingFlags.Static |
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic;

    public static float Mod(float a, float b) => (a % b + b) % b;

    public static Color MultiplyKeepAlpha(Color color, float scale) => new((int) (color.R * scale), (int) (color.G * scale), (int) (color.B * scale), color.A);

    public static bool CheckFlag(string flag, Session session, bool inverted = false)
        => string.IsNullOrWhiteSpace(flag) || session.GetFlag(flag) != inverted;

    public static void EmitCall(this ILCursor cursor, Delegate d) => cursor.Emit(OpCodes.Call, d.Method);

    public static ILHook CreateHook(this Type type, string name, ILContext.Manipulator manipulator)
        => new(type.GetMethod(name, ALL_FLAGS), manipulator);
}