//WulfrumTank

using CalamityAddon.Content.Items.Placeables.Banners;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using CalamityAddon.Content.Gores.Wulfrum;
using CalamityAddon.Content.Projectiles;
using Terraria.GameContent.ItemDropRules;
using CalamityAddon.Content.Items.Materials;
using CalamityAddon.Content.Items.Ammo;

namespace CalamityAddon.Content.NPCs
{
    public class WulfrumTank : ModNPC
    {       
        public const float MaxMovementSpeedX = 3f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = -1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.width = 60;
            NPC.height = 44;
            NPC.defense = 7;
            NPC.lifeMax = 70;
            NPC.knockBackResist = 0.3f;
            NPC.value = Item.buyPrice(0, 0, 1, 25);
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");

            Banner = Type;
			BannerItem = ModContent.ItemType<WulfrumTankBanner>();
			ItemID.Sets.KillsToBanner[BannerItem] = 50;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumTank.Bestiary"))
            });
        }

        private int shootCooldown = 0;
        private const int ShootCooldownMax = 150;

        public override void AI()
        {
            NPC.TargetClosest(true);
            Lighting.AddLight(NPC.Center, 0.2f, 0.8f, 1f);

            // === СУПЕРЗАРЯД ===
            bool supercharged = NPC.ai[3] > 0;
            if (supercharged)
            {
                NPC.ai[3]--;
                Lighting.AddLight(NPC.Center, 0.4f, 1f, 0.3f);
            }

            float moveSpeed = supercharged ? MaxMovementSpeedX * 1.5f : MaxMovementSpeedX;
            float stopDist = supercharged ? 450f : 320f;
            int rocketDamage = supercharged ? 15 : 8;
            float rocketSpeed = supercharged ? 8f : 5f;
            int cooldownMax = supercharged ? 90 : ShootCooldownMax;

            Player player = null;
            if (NPC.target >= 0 && NPC.target < Main.maxPlayers)
                player = Main.player[NPC.target];

            bool playerNearby = player.active && !player.dead && Vector2.Distance(NPC.Center, player.Center) < 1200f;

            if (!playerNearby)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, 0.4f);
                shootCooldown = 0;
            }
            else
            {
                float distanceX = player.Center.X - NPC.Center.X;
                float absDistanceX = Math.Abs(distanceX);

                if (absDistanceX > stopDist)
                {
                    int newDirection = Math.Sign(distanceX);
                    if (NPC.direction != newDirection)
                    {
                        NPC.direction = newDirection;
                        NPC.netUpdate = true;
                    }

                    float wantedSpeed = moveSpeed * NPC.direction;
                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, wantedSpeed, 0.2f);
                }
                else
                {
                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, 0.4f);

                    shootCooldown--;
                    if (shootCooldown <= 0)
                    {
                        ShootHomingRocket(player, rocketDamage, rocketSpeed);
                        shootCooldown = cooldownMax;
                    }
                }
            }

            NPC.velocity.Y += 0.3f;
            if (NPC.velocity.Y > 10f)
                NPC.velocity.Y = 10f;

            if (Math.Abs(NPC.velocity.X) > 0.5f)
            {
                NPC.stairFall = true;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            }

            NPC.spriteDirection = NPC.direction;
        }

        private void ShootHomingRocket(Player target, int damage, float speed)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 offset = new Vector2(28 * NPC.spriteDirection, 8);
            Vector2 spawnPosition = NPC.Center + offset;

            Vector2 velocity = new Vector2(speed * NPC.spriteDirection, 0f);

            float knockBack = 1f;

            int projectileType = ModContent.ProjectileType<WulfrumRocket>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity, projectileType, damage, knockBack, Main.myPlayer);
        }

        private int animationCounter = 0;

        public override void FindFrame(int frameHeight)
        {
            bool supercharged = NPC.ai[3] > 0;
            int frameOffset = supercharged ? 2 : 0; 

            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                animationCounter++;
                if (animationCounter > 5)
                {
                    animationCounter = 0;
                    NPC.frame.Y = (NPC.frame.Y == frameOffset * frameHeight) 
                        ? (frameOffset + 1) * frameHeight 
                        : frameOffset * frameHeight;
                }
            }
            else
            {
                NPC.frame.Y = (frameOffset + 1) * frameHeight;
                animationCounter = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                calamity.TryFind<ModWall>("SulphurousSand", out var wall1);
                calamity.TryFind<ModWall>("SulphurousSandWall", out var wall2);
                calamity.TryFind<ModWall>("SulphurousSandstone", out var wall3);

                int tileX = spawnInfo.SpawnTileX;
                int tileY = spawnInfo.SpawnTileY;

                for (int i = -5; i < 5; i++)
                {
                    for (int j = -5; j < 5; j++)
                    {
                        if (WorldGen.InWorld(tileX + i, tileY + j))
                        {
                            ushort currentWall = Main.tile[tileX + i, tileY + j].WallType;

                            if ((wall1 != null && currentWall == wall1.Type) ||
                                (wall2 != null && currentWall == wall2.Type) ||
                                (wall3 != null && currentWall == wall3.Type))
                            {
                                return 0f;
                            }
                        }
                    }
                }
            }
            return SpawnCondition.OverworldDaySlime.Chance * 0.1f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;

            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1.5f);
                }

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<TankGore1>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<TankGore2>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<TankGore3>(), 1f);

                int randomGoreCount = Main.rand.Next(2, 5);
                for (int i = 0; i < randomGoreCount; i++)
                {
                    int index = Main.rand.Next(1, 11);

                    if (ModContent.TryFind<ModGore>("CalamityMod", "WulfrumEnemyGore" + index, out ModGore calGore))
                    {
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * 0.5f, calGore.Type, 1f);
                    }
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) 
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumLRocket>(), 2, 5, 10));
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 2, 3));
            if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 2));
            }
            }
        }
    }
}
