using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
using ReLogic.Content;
using System.Collections.Generic;
using CalamityAddon.Content;
using CalamityAddon.Content.Gores.Wulfrum;
using CalamityAddon.Content.Utilities;
using CalamityAddon.Content.BossBars;
using CalamityAddon.Content.Projectiles;
using CalamityAddon.Content.Particles;
using CalamityAddon.Content.NPCs;
using CalamityAddon.Content.Items.Materials;
using CalamityAddon.Content.Items.Ammo;
using CalamityAddon.Content.Items.Placeables.Furniture.BossRelics;
using CalamityAddon.Content.Items.LoreItems;
using System.IO;

namespace CalamityAddon.Content.NPCs.WulfrumMothership
{
    [AutoloadBossHead]
    public class WulfrumMothership : ModNPC
    {
        // === ЩИТ ===
        private int ShieldMaxHP;
        private const int ShieldRegenDelay = 1200;
        private int ShieldHP;
        private int ShieldRegenTimer = 0;
        private bool ShieldActive => ShieldHP > 0;

        public float GetShieldHP() => ShieldHP;
        public float GetShieldMaxHP() => ShieldMaxHP;

        // === ШЕЙДЕР ===
        private static Effect shieldEffect;
        private static bool shieldEffectLoaded = false;
        private static Asset<Texture2D> noiseTex;

        // === SUPERCHARGE ===
        private const int SuperchargeTime = 600;
        private const int ChargeRadiusMax = 800;
        private int ChargeRadius = 0;
        private bool phase2Triggered = false;

        // === СПАМ-МОБАМИ ===
        private int phase2SpawnTimer = 0;
        private int wormSpawnTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "CalamityAddon/Content/NPCs/WulfrumMothership/WulfrumMothership_Bestiary",
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void FindFrame(int frameHeight)
        {
            bool isSecondPhase = NPC.life < NPC.lifeMax * 0.6f;
            if (isSecondPhase)
                NPC.frame.Y = frameHeight;
            else
                NPC.frame.Y = 0;
        }

        public override void SetDefaults()
        {
            NPC.width = 88;
            NPC.height = 62;
            NPC.damage = 30;
            NPC.defense = 15;
            NPC.lifeMax = 2000;

            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");

            NPC.value = Item.buyPrice(0, 2, 0, 0);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;

            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.BossBar = ModContent.GetInstance<BossBarWulfrumMothership>();

            if (Main.getGoodWorld)
                ShieldMaxHP = 500;
            else if (Main.masterMode)
                ShieldMaxHP = 400;
            else if (Main.expertMode)
                ShieldMaxHP = 300;
            else
                ShieldMaxHP = 200;

            ShieldHP = ShieldMaxHP;

            if (!Main.dedServ) {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Sounds/Music/MechanismWarfare");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumMothership.Bestiary"))
            });
        }

        // === СИНХРОНИЗАЦИЯ ДЛЯ МУЛЬТИПЛЕЕРА ===
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ShieldHP);
            writer.Write(ShieldRegenTimer);
            writer.Write(phase2SpawnTimer);
            writer.Write(wormSpawnTimer);
            writer.Write(phase2Triggered);
            writer.Write(phase3Triggered);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ShieldHP = reader.ReadInt32();
            ShieldRegenTimer = reader.ReadInt32();
            phase2SpawnTimer = reader.ReadInt32();
            wormSpawnTimer = reader.ReadInt32();
            phase2Triggered = reader.ReadBoolean();
            phase3Triggered = reader.ReadBoolean();
        }

        private static void LoadShieldEffect(Mod mod)
        {
            if (shieldEffectLoaded) return;
            shieldEffectLoaded = true;

            try
            {
                shieldEffect = mod.Assets.Request<Effect>(
                    "Content/Effects/RoverDriveShield",
                    AssetRequestMode.ImmediateLoad
                ).Value;
            }
            catch
            {
                shieldEffect = null;
            }
        }

        public override void Unload()
        {
            shieldEffect = null;
            shieldEffectLoaded = false;
            noiseTex = null;
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (ShieldActive)
            {
                // Используем исходный урон ДО модификации
                int actualDamage = (int)hit.Damage;
                ApplyDamageToShield(actualDamage);
            }
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (ShieldActive)
            {
                // Используем исходный урон ДО модификации
                int actualDamage = (int)hit.Damage;
                ApplyDamageToShield(actualDamage);
            }
        }

        private void ApplyDamageToShield(int damage)
        {
            // Только сервер/синглплеер обрабатывает урон
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            ShieldHP -= damage;

            if (ShieldHP <= 0)
            {
                ShieldHP = 0;
                ShieldRegenTimer = ShieldRegenDelay;
                SoundEngine.PlaySound(new SoundStyle("CalamityAddon/Content/Sounds/RoverDriveBreak"), NPC.Center);
            }
            else
            {
                // Звук попадания по щиту
                SoundEngine.PlaySound(SoundID.NPCHit4 with { Volume = 0.5f, Pitch = 0.3f }, NPC.Center);
            }

            // Визуальный эффект попадания
            if (!Main.dedServ)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height) * 0.5f, 
                        DustID.Electric, speed, Scale: 1.5f);
                    d.noGravity = true;
                }
            }

            NPC.netUpdate = true;
        }

        private void UpdateShield()
        {
            // Только сервер/синглплеер обновляет регенерацию
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!ShieldActive)
            {
                ShieldRegenTimer--;
                if (ShieldRegenTimer <= 0)
                {
                    ShieldHP = ShieldMaxHP;
                    ShieldRegenTimer = 0;
                    SoundEngine.PlaySound(new SoundStyle("CalamityAddon/Content/Sounds/RoverDriveActivate"), NPC.Center);

                    // Визуальный эффект восстановления
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 20f;
                            Vector2 pos = NPC.Center + angle.ToRotationVector2() * 60f;
                            Dust d = Dust.NewDustPerfect(pos, DustID.Vortex, 
                                (NPC.Center - pos).SafeNormalize(Vector2.Zero) * 3f);
                            d.noGravity = true;
                            d.scale = 1.2f;
                        }
                    }

                    NPC.netUpdate = true;
                }
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!ShieldActive) return;

            LoadShieldEffect(Mod);

            if (shieldEffect == null) return;

            float healthRatio = (float)ShieldHP / ShieldMaxHP;

            if (noiseTex == null)
            {
                try
                {
                    noiseTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/TechyNoise");
                }
                catch
                {
                    return;
                }
            }

            shieldEffect.Parameters["time"]?.SetValue(Main.GlobalTimeWrappedHourly * 0.24f);
            shieldEffect.Parameters["blowUpPower"]?.SetValue(2.5f);
            shieldEffect.Parameters["blowUpSize"]?.SetValue(0.5f);
            shieldEffect.Parameters["noiseScale"]?.SetValue(0.5f);

            float baseShieldOpacity = 0.9f + 0.1f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            shieldEffect.Parameters["shieldOpacity"]?.SetValue(baseShieldOpacity * healthRatio);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"]?.SetValue(4f);

            Color greenTint = new Color(100, 255, 180);
            Color cyanTint = new Color(71, 202, 255);
            Color wulfGreen = new Color(194, 255, 67);

            float t = Main.GlobalTimeWrappedHourly * 0.2f % 1f;
            Color edgeColor;
            if (t < 0.33f)
                edgeColor = Color.Lerp(greenTint, cyanTint, t / 0.33f);
            else if (t < 0.66f)
                edgeColor = Color.Lerp(cyanTint, wulfGreen, (t - 0.33f) / 0.33f);
            else
                edgeColor = Color.Lerp(wulfGreen, greenTint, (t - 0.66f) / 0.34f);

            shieldEffect.Parameters["shieldColor"]?.SetValue(greenTint.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"]?.SetValue(edgeColor.ToVector3());

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = noiseTex.Value;
            Vector2 pos = NPC.Center + NPC.gfxOffY * Vector2.UnitY - screenPos;
            float scale = NPC.scale * 0.25f;
            spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private bool IsRevengeance()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                var calWorldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
                if (calWorldType != null)
                {
                    var revengeField = calWorldType.GetField("revenge",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (revengeField != null)
                        return (bool)revengeField.GetValue(null);
                }
            }
            return false;
        }
        private bool IsDeath()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                var calWorldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
                if (calWorldType != null)
                {
                    var deathField = calWorldType.GetField("death",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (deathField != null)
                        return (bool)deathField.GetValue(null);
                }
            }
            return false;
        }


        private bool phase3Triggered = false;
        public override void AI()
        {
            NPC.rotation = NPC.velocity.X * 0.05f;

            // === ХОВЕР ЭФФЕКТ ===
            if (!Main.dedServ && Main.GameUpdateCount % 7 == 0)
            {
                Vector2 offset = new Vector2(0f, NPC.height / 2f + 4f).RotatedBy(NPC.rotation);
                Vector2 spawnPos = NPC.Center + offset;
                WulfrumExhaustSystem.Spawn(spawnPos, NPC.velocity, 1f, 60, NPC.rotation);
            }

            // === ФАЗЫ ===
            bool isSecondPhase = NPC.life < NPC.lifeMax * 0.6f;

            bool isHardDifficulty = Main.masterMode || IsRevengeance() || IsDeath();
            bool isThirdPhase = isHardDifficulty && NPC.life < NPC.lifeMax * 0.35f;

            // === ПЕРЕХОД ВО ВТОРУЮ ФАЗУ ===
            if (isSecondPhase && !phase2Triggered)
            {
                phase2Triggered = true;
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    SpawnPhase2Mobs();

                SoundEngine.PlaySound(new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHurry1"), NPC.Center); 
                
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0, 0, 100, default, 2f);
                    }
                }

                NPC.netUpdate = true;
            }

            // === ПЕРЕХОД В ТРЕТЬЮ ФАЗУ ===
            if (isThirdPhase && !phase3Triggered)
            {
                phase3Triggered = true;
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    SpawnPhase3Worms();

                SoundEngine.PlaySound(new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHurry2"), NPC.Center); 

                NPC.netUpdate = true;
            }

            if (isThirdPhase)
                Lighting.AddLight(NPC.Center, 0.2f, 0.8f, 1f);
            else if (isSecondPhase)
                Lighting.AddLight(NPC.Center, 0.2f, 0.8f, 1f);
            else
                Lighting.AddLight(NPC.Center, 0.1f, 0.5f, 0.1f);

            UpdateShield();

            if (isSecondPhase)
                UpdateSuperchargeField();
            else
                ChargeRadius = 0;

            if (isSecondPhase)
            {
                phase2SpawnTimer++;
                if (phase2SpawnTimer >= 1800)
                {
                    phase2SpawnTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnPhase2Mobs();
                        NPC.netUpdate = true;
                    }
                }
            }
            else
            {
                phase2SpawnTimer = 0;
            }

            if (isThirdPhase)
            {
                wormSpawnTimer++;
                if (wormSpawnTimer >= 1500)
                {
                    wormSpawnTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnPhase3Worms();
                        NPC.netUpdate = true;
                    }
                }
            }
            else
            {
                wormSpawnTimer = 0;
            }


            int cooldown = isSecondPhase ? 120 : 240;
            float moveSpeed = isSecondPhase ? 10f : 7f;
            float inertia = isSecondPhase ? 20f : 25f;

            int rocketTimeBetweenShots = isSecondPhase ? 20 : 25;
            int rocketShotsCount = 5;

            int spawnTimeDelay = isSecondPhase ? 60 : 90;
            int mobsToSpawn = isSecondPhase ? 3 : 2;
            int mobSpawnInterval = isSecondPhase ? 15 : 30;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest(true);

            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.velocity.Y -= 0.1f;
                NPC.EncourageDespawn(10);
                return;
            }

            NPC.ai[0]++;

            if (NPC.ai[0] < cooldown)
            {
                NPC.noTileCollide = true;

                float hoverHeight = -300f;
                Vector2 targetPosition = player.Center + new Vector2(0, hoverHeight);
                Vector2 moveDirection = targetPosition - NPC.Center;

                float distance = moveDirection.Length();
                if (distance > moveSpeed)
                {
                    moveDirection.Normalize();
                    moveDirection *= moveSpeed;
                    NPC.velocity = (NPC.velocity * (inertia - 1) + moveDirection) / inertia;
                }
                else if (NPC.velocity == Vector2.Zero)
                {
                    NPC.velocity.X = -0.15f;
                    NPC.velocity.Y = -0.05f;
                }

                if (NPC.velocity.X > 0.1f) NPC.spriteDirection = 1;
                else if (NPC.velocity.X < -0.1f) NPC.spriteDirection = -1;
                
                if (isThirdPhase && IsDeath() && NPC.ai[0] % 35 == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        ShootLaser(player);
                }
            }
            else
            {
                float attackTimer = NPC.ai[0] - cooldown;

                if (NPC.ai[1] == 0 || NPC.ai[1] == 1)
                {
                    if (NPC.ai[1] == 0) // === РАКЕТЫ ===
                    {
                        float hoverHeight = -300f;
                        Vector2 targetPosition = player.Center + new Vector2(0, hoverHeight);
                        Vector2 moveDirection = targetPosition - NPC.Center;
                        float distance = moveDirection.Length();

                        if (distance > moveSpeed)
                        {
                            moveDirection.Normalize();
                            moveDirection *= moveSpeed;
                            NPC.velocity = (NPC.velocity * (inertia - 1) + moveDirection) / inertia;
                        }

                        if (NPC.velocity.X > 0.1f) NPC.spriteDirection = 1;
                        else if (NPC.velocity.X < -0.1f) NPC.spriteDirection = -1;

                        if (attackTimer % rocketTimeBetweenShots == 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                ShootHomingRocket(player, isSecondPhase);
                        }
                        
                        if (attackTimer >= (rocketShotsCount - 1) * rocketTimeBetweenShots + 20)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                ResetAttackCycle(isSecondPhase);
                        }
                    }
                    else if (NPC.ai[1] == 1) // === ДЕСАНТ ===
                    {
                        NPC.velocity *= 0.9f;
                        if (NPC.velocity.Length() < 0.1f) NPC.velocity = Vector2.Zero;

                        if (attackTimer >= spawnTimeDelay)
                        {
                            float timeSinceSpawningStarted = attackTimer - spawnTimeDelay;
                            if (timeSinceSpawningStarted % mobSpawnInterval == 0 &&
                                timeSinceSpawningStarted < mobsToSpawn * mobSpawnInterval)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    SpawnMob();
                            }
                            
                            if (timeSinceSpawningStarted >= (mobsToSpawn * mobSpawnInterval) + 30)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    ResetAttackCycle(isSecondPhase);
                            }
                        }
                    }
                }
                else if (NPC.ai[1] == 3)
                {
                    PerformSwoop(player, attackTimer, isSecondPhase, cooldown);
                }
            }
        }

        private void SpawnPhase3Worms()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Player player = Main.player[NPC.target];

            int wormCount = Main.getGoodWorld ? 5 : Main.masterMode ? 4 : 3;
            float totalWidth = 900f;
            float spacing = totalWidth / (wormCount - 1);

            for (int i = 0; i < wormCount; i++)
            {
                float xOffset = -totalWidth / 2f + (i * spacing);
                Vector2 spawnPos = new Vector2(
                    player.Center.X + xOffset,
                    player.Center.Y + 500f
                );

                int index = NPC.NewNPC(NPC.GetSource_FromAI(),
                    (int)spawnPos.X, (int)spawnPos.Y,
                    ModContent.NPCType<WulfrumWormHead>());

                if (index != Main.maxNPCs && Main.npc[index].active)
                {
                    Main.npc[index].velocity.Y = -10f;
                    Main.npc[index].velocity.X = Main.rand.NextFloat(-2f, 2f);

                    if (!Main.dedServ)
                    {
                        for (int j = 0; j < 15; j++)
                            Dust.NewDust(Main.npc[index].position, Main.npc[index].width, Main.npc[index].height,
                                DustID.Dirt, Main.rand.NextFloat(-3f, 3f), -5f, 100, default, 2f);
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item14, player.Center);
        }

        private void SpawnPhase2Mobs()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Player player = Main.player[NPC.target];

            List<int> possibleMobs = new List<int>();
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                if (calamity.TryFind<ModNPC>("WulfrumDrone", out ModNPC drone)) possibleMobs.Add(drone.Type);
                if (calamity.TryFind<ModNPC>("WulfrumHovercraft", out ModNPC hovercraft)) possibleMobs.Add(hovercraft.Type);
            }

            if (possibleMobs.Count == 0) return;

            int mobCount = Main.getGoodWorld ? 8 : Main.masterMode ? 7 : Main.expertMode ? 6 : 4;
            float totalWidth = 1200f;
            float spacing = totalWidth / (mobCount - 1);

            for (int i = 0; i < mobCount; i++)
            {
                float xOffset = -totalWidth / 2f + (i * spacing);
                Vector2 spawnPos = new Vector2(
                    player.Center.X + xOffset,
                    player.Center.Y - Main.screenHeight / 2 - 100f
                );

                int selectedType = possibleMobs[Main.rand.Next(possibleMobs.Count)];
                int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, selectedType);

                if (index != Main.maxNPCs && Main.npc[index].active)
                {
                    Main.npc[index].velocity.Y = 5f;
                    Main.npc[index].velocity.X = Main.rand.NextFloat(-2f, 2f);

                    if (!Main.dedServ)
                    {
                        for (int j = 0; j < 10; j++)
                            Dust.NewDust(Main.npc[index].position, Main.npc[index].width, Main.npc[index].height,
                                DustID.Electric, 0f, 2f, 100, default, 1.5f);
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item113, NPC.Center);
        }


        private void PerformSwoop(Player player, float timer, bool isSecondPhase, int cooldown)
        {
            int maxSwoops = isSecondPhase ? 3 : 2;
            int prepTime = isSecondPhase ? 30 : 60;
            int pauseTime = isSecondPhase ? 10 : 20;
            int swoopDuration = isSecondPhase ? 70 : 90;

            if (timer < prepTime)
            {
                if (timer == 1)
                {
                    if (NPC.ai[3] == 0)
                        NPC.ai[2] = (player.Center.X > NPC.Center.X) ? -1 : 1;
                    else
                        NPC.ai[2] = -NPC.ai[2];
                }

                float searchXOffset = -400f;
                Vector2 destination = player.Center + new Vector2(searchXOffset * NPC.ai[2], -200f);
                Vector2 moveDir = destination - NPC.Center;
                float dist = moveDir.Length();

                if (dist > 30f)
                {
                    moveDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 20f, 0.1f);
                }
                else
                {
                    NPC.velocity *= 0.6f;
                    if (NPC.velocity.Length() < 1f) NPC.velocity = Vector2.Zero;
                    NPC.rotation = 0f;
                }
                NPC.spriteDirection = (int)NPC.ai[2];
            }
            else if (timer < prepTime + pauseTime)
            {
                NPC.velocity *= 0.8f;
                if (!Main.dedServ && timer % 5 == 0)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0, 0, 0, default, 0.5f);
            }
            else if (timer < prepTime + pauseTime + swoopDuration)
            {
                float subphaseTime = timer - (prepTime + pauseTime);
                float direction = NPC.ai[2];
                Vector2 swoopVelocity = Vector2.UnitY.RotatedBy(MathHelper.Pi * subphaseTime / swoopDuration * -direction);
                float speed = isSecondPhase ? 20f : 15f;
                swoopVelocity *= speed;
                swoopVelocity.Y *= 0.5f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, swoopVelocity, 0.2f);
                NPC.rotation = NPC.velocity.X * 0.05f;
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (NPC.velocity.Length() < 2f)
                {
                    NPC.ai[3]++;
                    if (NPC.ai[3] < maxSwoops)
                        NPC.ai[0] = cooldown;
                    else
                    {
                        NPC.ai[3] = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            ResetAttackCycle(isSecondPhase);
                    }
                }
            }
        }

        private void UpdateSuperchargeField()
        {
            ChargeRadius = (int)MathHelper.Lerp(ChargeRadius, ChargeRadiusMax, 0.05f);

            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                float dustCount = MathHelper.TwoPi * ChargeRadius / 10f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Vortex);
                    dust.position = NPC.Center + angle.ToRotationVector2() * ChargeRadius;
                    dust.scale = 0.6f;
                    dust.noGravity = true;
                    dust.velocity = NPC.velocity;
                }
            }

            // Только сервер/синглплеер применяет суперзаряд
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            List<int> wulfrumTypes = GetWulfrumNPCTypes();

            foreach (var npc in Main.ActiveNPCs)
            {
                if (!wulfrumTypes.Contains(npc.type)) continue;
                if (npc.ai[3] > 0f) continue;
                if (NPC.Distance(npc.Center) > ChargeRadius) continue;

                npc.ai[3] = SuperchargeTime;
                npc.netUpdate = true;

                if (!Main.dedServ)
                {
                    for (int j = 0; j < 10; j++)
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
                }
            }
        }

        private void ResetAttackCycle(bool isSecondPhase)
        {
            int previousAttack = (int)NPC.ai[1];
            NPC.ai[0] = 0;
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;

            bool isThirdPhase = (Main.masterMode || IsRevengeance() || IsDeath()) && NPC.life < NPC.lifeMax * 0.35f;

            int newAttack;
            if (isThirdPhase)
            {
                int[] attacks = { 0, 1, 3 };
                do { newAttack = attacks[Main.rand.Next(attacks.Length)]; }
                while (newAttack == previousAttack);
            }
            else
            {
                int[] attacks = { 0, 1, 3 };
                do { newAttack = attacks[Main.rand.Next(attacks.Length)]; }
                while (newAttack == previousAttack);
            }

            NPC.ai[1] = newAttack;
            NPC.netUpdate = true;
        }

        private void ShootLaser(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            int laserCount = 5;
            float maxAngle = MathHelper.ToRadians(20f);
            float speed = 6f;
            int damage = Main.masterMode ? 8 : Main.expertMode ? 9 : 12;

            float shipWidth = NPC.width - 20f;

            for (int i = 0; i < laserCount; i++)
            {
                float xOffset = -shipWidth / 2f + (shipWidth / (laserCount - 1)) * i;
                Vector2 spawnPos = new Vector2(NPC.Center.X + xOffset, NPC.Center.Y + 60f);

                float angle = maxAngle - (2f * maxAngle / (laserCount - 1)) * i;
                Vector2 velocity = Vector2.UnitY.RotatedBy(angle) * speed;

                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                    ProjectileID.SaucerLaser, damage, 0f);
            }

            SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
        }

        private List<int> GetWulfrumNPCTypes()
        {
            List<int> types = new List<int>();
            
            types.Add(ModContent.NPCType<WulfrumTank>());
            types.Add(ModContent.NPCType<WulfrumWormHead>());
            
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                string[] names = { "WulfrumDrone", "WulfrumHovercraft", "WulfrumRover", "WulfrumGyrator" };
                foreach (string name in names)
                {
                    if (calamity.TryFind<ModNPC>(name, out ModNPC modNpc))
                        types.Add(modNpc.Type);
                }
            }
            return types;
        }

        private void SpawnMob()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 spawnPos = NPC.Center;
            spawnPos.Y += NPC.height / 2;
            
            List<int> possibleMobs = new List<int>();
            possibleMobs.Add(ModContent.NPCType<WulfrumTank>());
            
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                if (calamity.TryFind<ModNPC>("WulfrumRover", out ModNPC rover)) possibleMobs.Add(rover.Type);
                if (calamity.TryFind<ModNPC>("WulfrumGyrator", out ModNPC gyrator)) possibleMobs.Add(gyrator.Type);
            }
            
            if (possibleMobs.Count > 0)
            {
                int selectedType = possibleMobs[Main.rand.Next(possibleMobs.Count)];
                int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, selectedType);
                
                if (index != Main.maxNPCs && Main.npc[index].active)
                {
                    Main.npc[index].velocity.Y = 10f;
                    
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 10; i++)
                            Dust.NewDust(Main.npc[index].position, Main.npc[index].width, Main.npc[index].height, 
                                DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                    }
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item113, NPC.Center);
        }

        private void ShootHomingRocket(Player target, bool isSecondPhase)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 offset = new Vector2(-40 * NPC.spriteDirection, -15);
            Vector2 spawnPosition = NPC.Center + offset;
            
            float speedMult = isSecondPhase ? 1.3f : 1.0f;
            float launchSpeedX = 6f * speedMult;
            float launchSpeedY = 8f * speedMult;
            
            Vector2 velocity = new Vector2(-launchSpeedX * NPC.spriteDirection, -launchSpeedY);
            float spread = isSecondPhase ? 20f : 15f;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(spread));
            velocity *= Main.rand.NextFloat(0.9f, 1.1f);
            
            int damage = 15;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity, 
                ModContent.ProjectileType<WulfrumRocket>(), damage, 2f, Main.myPlayer);
            
            SoundEngine.PlaySound(SoundID.Item11, NPC.position);
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

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore1>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore2>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore3>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore4>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore5>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore6>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore7>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WGore8>(), 1f);

                int randomGoreCount = Main.rand.Next(4, 8);
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
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.TreasureBags.WulfrumMothershipBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumLRocket>(), 1, 20, 30));
            
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap))
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 20, 40));
            
            if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore))
                npcLoot.Add(ItemDropRule.Common(energyCore.Type, 1, 3, 6));

            npcLoot.Add(ItemDropRule.ByCondition(new MasterOrRevengeanceCondition(), ModContent.ItemType<WulfrumMothershipRelic>()));
            
            npcLoot.AddConditionalPerPlayer(
                () => !DownedBossSystem.downedWulfrumMothership,
                ModContent.ItemType<LoreWulfrumMothership>(), 
                desc: DropHelper.FirstKillText.Value
            );
        }

        public override void OnKill()
        {
            DownedBossSystem.downedWulfrumMothership = true;

            if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    NetworkText.FromLiteral("-GG#-Blup.."),
                    new Color(50, 255, 130));
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("-GG#-Blup..", 50, 255, 130);
            }
        }
    }

    public class WulfrumMothershipSpawnControl : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<WulfrumMothership>()))
            {
                maxSpawns = 0;
                spawnRate = int.MaxValue;
            }
        }

        public override bool PreKill(NPC npc)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<WulfrumMothership>()) && IsWulfrumNPC(npc))
            {
                npc.value = 0;
                return false;
            }
            return true;
        }

        private bool IsWulfrumNPC(NPC npc)
        {
            if (npc.type == ModContent.NPCType<WulfrumTank>())
                return true;
            if (npc.type == ModContent.NPCType<WulfrumWormHead>())
                return true;

            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return false;

            string[] names = { "WulfrumDrone", "WulfrumHovercraft", "WulfrumRover", "WulfrumGyrator" };
            foreach (string name in names)
            {
                if (calamity.TryFind<ModNPC>(name, out ModNPC modNpc) && npc.type == modNpc.Type)
                    return true;
            }
            return false;
        }
    }
}