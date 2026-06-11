using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Projectiles
{
    public class WulfrumBigis : ModProjectile
    {
        private bool hasExploded = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.Explosive[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(4);
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 3 && !hasExploded)
            {
                PrepareBombToBlow();
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;

            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            PrepareBombToBlow();
            return false;
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            PrepareBombToBlow();
        }

        public override void PrepareBombToBlow()
        {
            if (hasExploded) return;
            hasExploded = true;

            Projectile.tileCollide = false;
            Projectile.alpha = 255;
           
            Vector2 oldCenter = Projectile.Center;
            Projectile.Resize(80, 80);
            Projectile.Center = oldCenter;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
        }

        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int i = 0; i < 25; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }

            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch,
                    dustVel.X, dustVel.Y, 100, default, 1.5f);
            }

            for (int k = 0; k < 20; k++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, 0f, 0f, 100, default, 1f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 3f;
                Main.dust[dustIndex].scale = 1.2f;
            }
        }
    }
}