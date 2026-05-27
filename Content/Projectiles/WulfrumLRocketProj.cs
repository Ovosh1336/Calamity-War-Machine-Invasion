using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;

namespace CalamityAddon.Content.Projectiles
{
    public class WulfrumLRocketProj : ModProjectile
    { 
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 10;
            Projectile.aiStyle = -1; 
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
        }
        
        private int frameCounter = 0;
        private const float HomingDuration = 180f;

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.5f, 0.1f);

            Projectile.ai[0]++;

            float desiredSpeed = 8f;

            if (Projectile.ai[0] < 20f)
            {
                if (Projectile.velocity.Length() < 5f)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 3f;
                }
            }
            else 
            {
                if (Projectile.ai[0] < HomingDuration)
                {
                    NPC target = FindClosestNPC();
                    if (target != null && target.CanBeChasedBy())
                    {
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float homingStrength = 0.08f;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * desiredSpeed, homingStrength);
                    }
                }

                Vector2 offsetVector = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                
                float waveFrequency = 0.25f; // Частота
                float waveAmplitude = 1.05f; // Амплитуда
                
                Projectile.velocity += offsetVector * (float)Math.Sin(Projectile.ai[0] * waveFrequency) * waveAmplitude;

                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * desiredSpeed;
            }

            // Анимация кадров
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

        private NPC FindClosestNPC()
        {
            NPC closestNPC = null;
            float closestDistance = 800f; 

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }
            return closestNPC;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f; 
            Projectile.timeLeft = 3; 
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Explode();
        }
 
        private void Explode()
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            
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
        }
    }
}