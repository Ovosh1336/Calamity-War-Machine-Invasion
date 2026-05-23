using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Projectiles.Minions
{
    public class WulfrumMinis : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.Explosive[Type] = true;
            ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(4);
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            {
                Projectile.PrepareBombToBlow();
            }
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            Projectile.velocity.Y += 0.1f;
            if (Projectile.velocity.Y > 18f) Projectile.velocity.Y = 18f;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f; 
            Projectile.timeLeft = 3; 
            return false;
        }
        public override void PrepareBombToBlow()
        {
            Projectile.tileCollide = false;
            Projectile.alpha = 255; 
            Projectile.Resize(90, 90);
        }

        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }

            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
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