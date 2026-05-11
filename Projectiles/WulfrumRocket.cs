using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityAddon.Content.Projectiles
{
    public class WulfrumRocket : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 10;
            Projectile.aiStyle = 0;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
        }

        private int frameCounter = 0;
        private const float HomingDuration = 240f;

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.5f, 0.1f);

            Projectile.ai[0]++;

            if (Projectile.ai[0] < 20f)
            {
                if (Projectile.velocity.Length() < 5f)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f;
                }
            }
            else if (Projectile.ai[0] < HomingDuration)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];

                if (target != null && target.active && !target.dead)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                    float homingStrength = 0.08f;
                    float desiredSpeed = 9f;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * desiredSpeed, homingStrength);
                }
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead && Projectile.Hitbox.Intersects(player.Hitbox))
                {
                    Projectile.Kill();
                    break;
                }
            }

            frameCounter++;
            if (frameCounter >= 5)
            {
                frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
                Main.dust[dust].velocity *= 0.5f;
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Explode();
        }

        private void Explode()
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 2f);
            }

            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch,
                    dustVel.X, dustVel.Y, 100, default, 1.5f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }
    }
}