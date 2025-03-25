using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ProgHelper;

public static class GameplayRendererExtensions {
   public static void Load() {
      // On.Celeste.GameplayRenderer.Render += GameplayRenderer_Render;
   }

   public static void Unload() {
      // On.Celeste.GameplayRenderer.Render -= GameplayRenderer_Render;
   }

   private static void GameplayRenderer_Render(On.Celeste.GameplayRenderer.orig_Render render, GameplayRenderer gameplayRenderer, Scene scene) {
      Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffersExtensions.InvertMask);
      Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
      Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);

      render(gameplayRenderer, scene);
   }
}