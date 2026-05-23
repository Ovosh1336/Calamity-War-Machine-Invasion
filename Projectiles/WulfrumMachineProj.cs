using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Projectiles
{
    public class WulfrumMachineProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.Explosive[Type] = true;
            ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(5);
        }

        public override void AI()
        {
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            {
                Projectile.PrepareBombToBlow();
            }
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            Projectile.velocity.Y += 0.2f;
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
            Projectile.velocity *= 0f; 
            Projectile.timeLeft = 3; 
            return false;
        }
        public override void PrepareBombToBlow()
        {
            Projectile.tileCollide = false;
            Projectile.alpha = 255; 
            Projectile.Resize(120, 120);
        }
        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            for (int k = 0; k < 20; k++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, 0f, 0f, 100, default, 1f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 3f;
                Main.dust[dustIndex].scale = 1.2f;
            }

            int randomGoreCount = Main.rand.Next(2, 4); 
            for (int g = 0; g < randomGoreCount; g++)
            {
                int index = Main.rand.Next(1, 11);
                if (ModContent.TryFind("CalamityMod", "WulfrumEnemyGore" + index, out ModGore calGore))
                {
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(3f, 3f), calGore.Type, 1f);
                }
            }
        }
    }
}