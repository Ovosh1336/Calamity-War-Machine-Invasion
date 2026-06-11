using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using ReLogic.Content;
using CalamityAddon.Content.Projectiles;

namespace CalamityAddon.Content.NPCs.WulfrumJumper
{
    public class WulfrumJumper : ModNPC
    {
        private int springFrame = 0;
        private int animTimer = 0;
        private static Asset<Texture2D> springTex;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 30;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 76;
            NPC.height = 58;
            NPC.damage = 35;
            NPC.defense = 12;
            NPC.lifeMax = 1200;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(0, 1, 5, 0);

            if (!Main.dedServ)
            {
                Music = MusicID.Boss1;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 6) NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            NPC.spriteDirection = (player.Center.X < NPC.Center.X) ? 1 : -1;

            bool onGround = NPC.velocity.Y == 0;

            // ai[0] - Состояние: 0 - прыжки, 1 - стрельба ракетами
            // ai[1] - Универсальный таймер
            // ai[2] - Счетчик прыжков
            // ai[3] - Счетчик выпущенных ракет
            // localAI[0] - Флаг текущего прыжка (0 - малый, 1 - высокий)

            if (NPC.ai[0] == 0) // ФАЗА ПРЫЖКОВ
            {
                if (onGround)
                {
                    if (NPC.localAI[0] == 1)
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                        NPC.localAI[0] = 0;
                        NPC.netUpdate = true;
                        return;
                    }

                    NPC.velocity.X *= 0.8f;
                    NPC.ai[1]++;
                    springFrame = 0;
                    animTimer = 0;

                    if (NPC.ai[1] >= 60)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2]++;
                        NPC.TargetClosest(true);

                        if (NPC.ai[2] >= 4) // ВЫСОКИЙ ПРЫЖОК
                        {
                            NPC.velocity.Y = -14f;
                            NPC.velocity.X = 6f * NPC.direction;
                            NPC.localAI[0] = 1; // Запоминаем, что прыжок высокий
                            NPC.ai[2] = 0;
                            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                        }
                        else // МАЛЕНЬКИЙ ПРЫЖОК
                        {
                            NPC.velocity.Y = -10f;
                            NPC.velocity.X = 4f * NPC.direction;
                            NPC.localAI[0] = 0; // Запоминаем, что прыжок малый
                            SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                        }
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    int maxFrame = (NPC.localAI[0] == 1) ? 4 : 2;

                    if (springFrame < maxFrame)
                    {
                        animTimer++;
                        if (animTimer >= 4)
                        {
                            animTimer = 0;
                            springFrame++;
                        }
                    }
                }
            }
            else if (NPC.ai[0] == 1) // ФАЗА РАКЕТ
            {
                NPC.velocity.X *= 0.7f;
                NPC.ai[1]++;
                springFrame = 0;

                if (NPC.ai[1] % 25 == 0 && NPC.ai[3] < 3)
                {
                    ShootRocket(player);
                    NPC.ai[3]++;
                    NPC.netUpdate = true;
                }

                if (NPC.ai[3] >= 3 && NPC.ai[1] >= 120)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                    NPC.ai[3] = 0;
                    NPC.netUpdate = true;
                }
            }
        }

        private void ShootRocket(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            float launchSpeedX = 8f;
            float launchSpeedY = 9f;
            int damage = 20;

            Vector2 offset = new Vector2(-30 * NPC.spriteDirection, -20);
            Vector2 spawnPosition = NPC.Center + offset;

            Vector2 velocity = new Vector2(-launchSpeedX * NPC.spriteDirection, -launchSpeedY);
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(15f));
            velocity *= Main.rand.NextFloat(0.9f, 1.1f);

            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity,
                ModContent.ProjectileType<WulfrumRocket>(), damage, 2f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item11, NPC.position);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (springTex == null) springTex = ModContent.Request<Texture2D>(Texture + "Spring");
            Texture2D sTex = springTex.Value;
            Texture2D hTex = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;

            Vector2 drawPos = new Vector2((int)(NPC.Center.X - screenPos.X), (int)(NPC.position.Y + NPC.height - screenPos.Y));
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Пружина
            int sFrameHeight = sTex.Height / 5;
            Rectangle sRect = new Rectangle(0, sFrameHeight * springFrame, sTex.Width, sFrameHeight);
            Vector2 sOrigin = new Vector2(sTex.Width / 2f, sFrameHeight);
            spriteBatch.Draw(sTex, drawPos, sRect, drawColor, NPC.rotation, sOrigin, NPC.scale, effects, 0f);

            // Корпус
            float hullBaseHeight = 20f;
            float extensionStep = 8f;
            int hullVisualOffset = (int)(hullBaseHeight + (springFrame * extensionStep));
            int hFrameHeight = hTex.Height / Main.npcFrameCount[NPC.type];
            Vector2 hOrigin = new Vector2(hTex.Width / 2f, hFrameHeight);
            Vector2 hullDrawPos = new Vector2(drawPos.X, drawPos.Y - hullVisualOffset);
            spriteBatch.Draw(hTex, hullDrawPos, NPC.frame, drawColor, NPC.rotation, hOrigin, NPC.scale, effects, 0f);

            return false;
        }

        public override bool? CanFallThroughPlatforms()
        {
            if (NPC.target < 0 || NPC.target == 255) return false;
            Player player = Main.player[NPC.target];
            return player.active && !player.dead && player.Center.Y > NPC.position.Y + NPC.height;
        }
    }
}