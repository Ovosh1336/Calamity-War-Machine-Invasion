using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using System.Collections.Generic;

namespace CalamityAddon.Content.Particles
{
    public class ExhaustParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Opacity;
        public int Life;
        public int MaxLife;
        public bool Active;
        public float Rotation;
    }

    public class WulfrumExhaustSystem : ModSystem
    {
        private static List<ExhaustParticle> particles = new();
        private static Asset<Texture2D> texture;

        public override void Load()
        {
            On_Main.DrawNPCs += DrawParticles;
        }

        public override void Unload()
        {
            On_Main.DrawNPCs -= DrawParticles;
            particles = null;
            texture = null;
        }

        public static void Spawn(Vector2 pos, Vector2 velocity, float scale, int life, float rotation)
        {
            particles.Add(new ExhaustParticle
            {
                Position = pos,
                Velocity = velocity,
                Scale = scale,
                Opacity = 0.6f,
                Life = life,
                MaxLife = life,
                Active = true,
                Rotation = rotation
            });
        }

        public override void PostUpdateNPCs()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Life--;

                float progress = 1f - (float)p.Life / p.MaxLife;
                p.Opacity = 0.6f * (1f - progress);
                p.Scale *= 0.97f;

                // Летит вниз по наклону босса
                float speed = 1.5f;
                p.Position.X += (float)System.Math.Sin(p.Rotation) * speed;
                p.Position.Y += (float)System.Math.Cos(p.Rotation) * speed;

                if (p.Life <= 0)
                    particles.RemoveAt(i);
            }
        }

        private void DrawParticles(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);

            if (behindTiles || particles.Count == 0) return;

            if (texture == null)
                texture = ModContent.Request<Texture2D>("CalamityAddon/Content/Particles/WulfrumExhaust");

            Texture2D tex = texture.Value;
            Vector2 origin = tex.Size() / 2f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in particles)
            {
                Vector2 drawPos = p.Position - Main.screenPosition;
                Color color = Color.Cyan * p.Opacity;
                Main.spriteBatch.Draw(tex, drawPos, null, color, p.Rotation, origin, p.Scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}