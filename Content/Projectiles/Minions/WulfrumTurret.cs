using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using CalamityAddon.Content.Projectiles.Minions;
using CalamityAddon.Content.Buffs;

namespace CalamityAddon.Content.Projectiles.Minions
{
    public class WulfrumTurret : ModProjectile
    {
        private const float DetectionRange = 600f;
        private const float MaxDistanceToPlayer = 1200f;
        private const int ShotsPerBurst = 2;
        private const int TimeBetweenShots = 14;
        private const int CooldownAfterBurst = 100;
        private const float ProjectileSpeed = 14f;
        private const float ShootOffsetX = 12f;
        private const float ShootOffsetY = 0f;

        private enum AIState { Idle, Targeting, Shooting, Cooldown }

        private AIState State {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private float turretRotation = 0f;
        private float targetTurretRotation = 0f;
        private int shotsFired = 0;
        private int shootTimer = 0;
        private int cooldownTimer = 0;
        private NPC currentTarget = null;

        public override void SetStaticDefaults() {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults() {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI() {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead) {
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<WulfrumTurretBuff>())) {
                Projectile.timeLeft = 2;
            }

            if (currentTarget != null) {
                float distToPlayer = Vector2.Distance(owner.Center, currentTarget.Center);
                if (!currentTarget.active || currentTarget.life <= 0 || distToPlayer > MaxDistanceToPlayer) {
                    currentTarget = null;
                    State = AIState.Idle;
                }
            }

            UpdateAnimation();

            switch (State) {
                case AIState.Idle:
                    IdleBehavior(owner);
                    break;
                case AIState.Targeting:
                    TargetingBehavior(owner);
                    break;
                case AIState.Shooting:
                    ShootingBehavior(owner);
                    break;
                case AIState.Cooldown:
                    CooldownBehavior(owner);
                    break;
            }

            turretRotation = MathHelper.Lerp(turretRotation, targetTurretRotation, 0.15f);
        }

        private void UpdateAnimation() {
            int frameSpeed = 6;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= frameSpeed) {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 0;
            }
        }

        private void IdleBehavior(Player owner) {
            Vector2 idlePosition = owner.Center + new Vector2(0f, -60f);
            float distance = Vector2.Distance(Projectile.Center, idlePosition);

            if (distance > 400f) {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            } else {
                Vector2 direction = idlePosition - Projectile.Center;
                float speed = MathHelper.Clamp(distance * 0.05f, 2f, 12f);
                if (distance > 10f) {
                    direction.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.1f);
                } else {
                    Projectile.velocity *= 0.9f;
                }
            }

            if (Projectile.velocity.Length() > 0.5f)
            {
                Projectile.rotation = Projectile.velocity.X * 0.05f;
            }
            else
            {
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.1f);
            }

            float hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 5f;
            Projectile.position.Y += hoverOffset * 0.01f;

            if (owner.direction == 1) {
                targetTurretRotation = 0f;
                Projectile.spriteDirection = 1;
            } else {
                targetTurretRotation = MathHelper.Pi;
                Projectile.spriteDirection = -1;
            }

            if (Projectile.velocity.Length() > 0.5f) {
                Projectile.rotation = Projectile.velocity.X * 0.05f;
            } else {
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.1f);
            }

            currentTarget = FindClosestEnemy(owner, DetectionRange);
            if (currentTarget != null) {
                State = AIState.Targeting;
                Projectile.netUpdate = true;
            }
        }

        private void TargetingBehavior(Player owner) {
            if (currentTarget == null) { State = AIState.Idle; return; }

            Vector2 attackPosition = GetAttackPosition(owner, currentTarget);
            float distance = Vector2.Distance(Projectile.Center, attackPosition);
            Vector2 direction = (attackPosition - Projectile.Center).SafeNormalize(Vector2.Zero);

            float speed = MathHelper.Clamp(distance * 0.08f, 3f, 15f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.15f);

            Vector2 toTarget = currentTarget.Center - Projectile.Center;
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, toTarget.X * 0.08f, 0.2f);
            targetTurretRotation = toTarget.ToRotation();
            Projectile.spriteDirection = toTarget.X > 0 ? 1 : -1;

            if (Collision.CanHit(Projectile.Center, 1, 1, currentTarget.Center, 1, 1) && distance < 400f) {
                State = AIState.Shooting;
                shotsFired = 0;
                shootTimer = 0;
            }
        }

        private void ShootingBehavior(Player owner) {
            if (currentTarget == null) { State = AIState.Idle; return; }

            Projectile.velocity *= 0.9f;
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.2f);

            Vector2 toTarget = currentTarget.Center - Projectile.Center;
            targetTurretRotation = toTarget.ToRotation();
            Projectile.spriteDirection = toTarget.X > 0 ? 1 : -1;

            if (++shootTimer >= TimeBetweenShots) {
                shootTimer = 0;
                if (Projectile.owner == Main.myPlayer) ShootProjectile(currentTarget);
                shotsFired++;
                Projectile.velocity += (targetTurretRotation + MathHelper.Pi).ToRotationVector2() * 2f;

                if (shotsFired >= ShotsPerBurst) {
                    State = AIState.Cooldown;
                    cooldownTimer = 0;
                }
            }
        }

        private void CooldownBehavior(Player owner) {
            cooldownTimer++;
            if (currentTarget != null) {
                Vector2 toTarget = currentTarget.Center - Projectile.Center;
                targetTurretRotation = toTarget.ToRotation();
                Projectile.spriteDirection = toTarget.X > 0 ? 1 : -1;
            }

            Vector2 idlePos = owner.Center + new Vector2(0f, -60f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, (idlePos - Projectile.Center).SafeNormalize(Vector2.Zero) * 4f, 0.05f);

            if (cooldownTimer >= CooldownAfterBurst) {
                State = currentTarget != null ? AIState.Targeting : AIState.Idle;
            }
        }

        private NPC FindClosestEnemy(Player owner, float maxDistance) {
            if (owner.MinionAttackTargetNPC != -1) {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, npc.Center) < MaxDistanceToPlayer) {
                    return npc;
                }
            }

            NPC closestNPC = null;
            float closestDistance = maxDistance;
            foreach (NPC npc in Main.ActiveNPCs) {
                if (npc.CanBeChasedBy(Projectile)) {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDistance) {
                        closestDistance = dist;
                        closestNPC = npc;
                    }
                }
            }
            return closestNPC;
        }

        private Vector2 GetAttackPosition(Player owner, NPC target) {
            Vector2 toTarget = target.Center - owner.Center;
            toTarget.Normalize();
            Vector2 perpendicular = new Vector2(-toTarget.Y, toTarget.X);
            return target.Center + perpendicular * 100f;
        }

        private void ShootProjectile(NPC target) {
            Vector2 shootPosition = GetShootPosition();
            Vector2 predictedPos = target.Center + target.velocity * (Vector2.Distance(shootPosition, target.Center) / ProjectileSpeed);
            Vector2 velocity = (predictedPos - shootPosition).SafeNormalize(Vector2.UnitX) * ProjectileSpeed;

            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), shootPosition, velocity.RotatedByRandom(0.08f), ModContent.ProjectileType<WulfrumMinis>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            if (proj != Main.maxProjectiles) {
                Main.projectile[proj].originalDamage = Projectile.damage;
                Main.projectile[proj].ContinuouslyUpdateDamageStats = false;
            }
            SoundEngine.PlaySound(SoundID.Item11, shootPosition);
        }

        private Vector2 GetShootPosition() {
            Vector2 shootDirection = (currentTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            return Projectile.Center + shootDirection * ShootOffsetX;
        }

        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;
            SpriteEffects spriteEffect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float rotation = turretRotation;
            if (Projectile.spriteDirection == -1) rotation += MathHelper.Pi;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, lightColor, rotation, origin, Projectile.scale, spriteEffect, 0);
            return false;
        }
    }
}