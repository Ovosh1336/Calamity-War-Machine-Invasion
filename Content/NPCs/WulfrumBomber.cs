//WulfrumBomber

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityAddon.Content.Projectiles;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.ItemDropRules;

namespace CalamityAddon.Content.NPCs
{
    public class WulfrumBomber : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 16;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 50;
            NPC.damage = 5;
            NPC.defense = 6;
            NPC.lifeMax = 50;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            NPC.value = Item.buyPrice(0, 0, 1, 20);
            NPC.knockBackResist = 0.3f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumBomber.Bestiary"))
            });
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            bool supercharged = NPC.ai[3] > 0;

            if (supercharged)
            {
                NPC.ai[3]--;
                Lighting.AddLight(NPC.Center, 0.3f, 0.8f, 0.2f);
            }

            // ========== ЛОГИКА ПЕРЕДВИЖЕНИЯ ==========
            NPC.ai[0]++; // Основной таймер перезарядки
            float attackDelay = supercharged ? 150f : 210f;
            float hoverHeight = 170f;
            Vector2 targetPos;

            // Если идет перезарядка — улетаем в сторону
            if (NPC.ai[0] < attackDelay)
            {
                // Определяем сторону: если мы левее игрока, летим еще левее, и наоборот
                float retreatSide = (NPC.Center.X < player.Center.X) ? -1f : 1f;
                targetPos = player.Center + new Vector2(retreatSide * 600f, -hoverHeight - 100f);
            }
            else
            {
                // Если перезарядились — возвращаемся к игроку для захода на цель
                float horizontalOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.2f) * 190f;
                targetPos = player.Center + new Vector2(horizontalOffset, -hoverHeight);
            }

            float speed = supercharged ? 7f : 5f;
            float inertia = supercharged ? 20f : 35f;
            int bombDamage = supercharged ? 16 : 10;

            Vector2 desiredVelocity = (targetPos - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
            NPC.velocity = (NPC.velocity * (inertia - 1) + desiredVelocity) / inertia;

            NPC.spriteDirection = (NPC.velocity.X > 0) ? 1 : -1;
            NPC.rotation = NPC.velocity.X * 0.05f;

            // ========== ЛОГИКА АТАКИ ==========
            // Условие сброса: перезарядка завершена и X-координата близка к игроку
            if (NPC.ai[0] >= attackDelay && Math.Abs(NPC.Center.X - player.Center.X) < 30f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    DropBomb(bombDamage);

                    if (supercharged)
                    {
                        NPC.ai[1] = 25f;
                    }
                }
                NPC.ai[0] = 0; // Сброс таймера перезарядки (теперь он начнет улетать)
                NPC.netUpdate = true;
            }

            // Логика интервала для второй бомбы при Supercharge
            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]--;
                if (NPC.ai[1] == 1)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        DropBomb(bombDamage);
                }
            }
        }

        private void DropBomb(int damage)
        {
            int projectileType = ModContent.ProjectileType<WulfrumMinisEnemy>();
            Vector2 bombVel = new Vector2(NPC.velocity.X * 0.2f, 3f);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 10), bombVel, projectileType, damage, 0f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item10, NPC.Center);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            bool supercharged = NPC.ai[3] > 0;

            int minFrame = supercharged ? 8 : 0;
            int maxFrame = supercharged ? 16 : 8;

            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                int currentFrame = NPC.frame.Y / frameHeight;

                if (currentFrame < minFrame || currentFrame >= maxFrame)
                {
                    currentFrame = minFrame;
                }
                else
                {
                    currentFrame++;
                    if (currentFrame >= maxFrame)
                        currentFrame = minFrame;
                }

                NPC.frame.Y = currentFrame * frameHeight;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (ModContent.TryFind<ModGore>("CalamityMod", "WulfrumEnemyGore" + i, out ModGore g))
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, g.Type);
                }
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 2, 3));
            if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 1));
                }
            }
        }
    }
}