using System;
using System.Collections;
using System.Reflection;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.ProgHelper; 

public static class Util {
    private const BindingFlags ALL_FLAGS = BindingFlags.Instance |
                                           BindingFlags.Static |
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic;

    public static MethodInfo GetMethodUnconstrained(this Type type, string name) => type.GetMethod(name, ALL_FLAGS);
    
    public static PropertyInfo GetPropertyUnconstrained(this Type type, string name) => type.GetProperty(name, ALL_FLAGS);
}