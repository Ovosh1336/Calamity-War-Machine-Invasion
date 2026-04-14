//WulfrumWorm

using CalamityAddon.Content.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using CalamityAddon.Content.Gores.Wulfrum;

namespace CalamityAddon.Content.NPCs
{
    internal class WulfrumWormHead : WormHead
    {
        public override int BodyType => ModContent.NPCType<WulfrumWormBody>();
        public override int TailType => ModContent.NPCType<WulfrumWormTail>();

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "CalamityAddon/Content/NPCs/WulfrumWorm_Bestiary",
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerHead);
            NPC.aiStyle = -1;
            NPC.damage = 8;
            NPC.defense = 6;
            NPC.lifeMax = 54;
            NPC.value = Item.buyPrice(0, 0, 1, 50);
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            Banner = Type;
            BannerItem = ModContent.ItemType<WulfrumWormBanner>();
            ItemID.Sets.KillsToBanner[BannerItem] = 50;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumWormHead.Bestiary"))
            });
        }

        public override void Init()
        {
            int minLen = 8;
            int maxLen = 8;

            if (Main.expertMode)
            {
                minLen = 12;
                maxLen = 12;
            }
            else if (Main.masterMode)
            {
                minLen = 16;
                maxLen = 16;
            }

            MinSegmentLength = minLen;
            MaxSegmentLength = maxLen;

            CommonWormInit(this);
        }
        internal static void CommonWormInit(Worm worm)
        {
            worm.MoveSpeed = 5f;
            worm.Acceleration = 0.045f;
        }

        private int attackCounter;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackCounter);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackCounter = reader.ReadInt32();
        }

        public override void AI()
        {
            bool supercharged = NPC.ai[3] > 0;
            if (supercharged)
            {
                NPC.ai[3]--;
                Lighting.AddLight(NPC.Center, 0.4f, 1f, 0.3f);

                MoveSpeed = 8f;
                Acceleration = 0.07f;
                NPC.damage = 15;

                PropagateSupercharge();
            }
            else
            {
                MoveSpeed = 5f;
                Acceleration = 0.045f;
                NPC.damage = 8;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (attackCounter > 0)
                    attackCounter--;

                Player target = Main.player[NPC.target];
            }
        }

        private void PropagateSupercharge()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC segment = Main.npc[i];
                if (!segment.active) continue;

                bool isMySegment = (segment.type == ModContent.NPCType<WulfrumWormBody>() ||
                                    segment.type == ModContent.NPCType<WulfrumWormTail>());

                if (!isMySegment) continue;

                if (IsPartOfThisWorm(segment))
                {
                    segment.ai[3] = NPC.ai[3];
                }
            }
        }

        private bool IsPartOfThisWorm(NPC segment)
        {
            NPC current = segment;
            int safety = 100;

            while (safety > 0)
            {
                safety--;
                int followIndex = (int)current.ai[1];

                if (followIndex < 0 || followIndex >= Main.maxNPCs)
                    return false;

                NPC followed = Main.npc[followIndex];
                if (!followed.active)
                    return false;

                if (followed.whoAmI == NPC.whoAmI)
                    return true;

                current = followed;
            }

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            bool supercharged = NPC.ai[3] > 0;
            NPC.frame.Y = supercharged ? frameHeight : 0;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.Underground.Chance * 0.05f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;

            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1.5f);
                }

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WormGore1>(), 1f);

                int randomGoreCount = Main.rand.Next(1, 2);
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
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 3, 4));
			if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 2));
			}
            }
        }
    }

    internal class WulfrumWormBody : WormBody
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;

            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<WulfrumWormHead>();
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerBody);
            NPC.aiStyle = -1;
            NPC.damage = 8;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            Banner = Type;
            BannerItem = ModContent.ItemType<WulfrumWormBanner>();
            ItemID.Sets.KillsToBanner[BannerItem] = 50;
        }

        public override void Init()
        {
            WulfrumWormHead.CommonWormInit(this);
        }

        public override void FindFrame(int frameHeight)
        {
            bool supercharged = NPC.ai[3] > 0;
            NPC.frame.Y = supercharged ? frameHeight : 0;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;

            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1.5f);
                }

                int randomGoreCount = Main.rand.Next(0, 1);
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
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 3, 4));
			if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 2));
			}
			}
        }
    }

    internal class WulfrumWormTail : WormTail
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;

            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<WulfrumWormHead>();
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerTail);
            NPC.aiStyle = -1;
            NPC.damage = 8;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            Banner = Type;
            BannerItem = ModContent.ItemType<WulfrumWormBanner>();
            ItemID.Sets.KillsToBanner[BannerItem] = 50;
        }

        public override void Init()
        {
            WulfrumWormHead.CommonWormInit(this);
        }

        public override void FindFrame(int frameHeight)
        {
            bool supercharged = NPC.ai[3] > 0;
            NPC.frame.Y = supercharged ? frameHeight : 0;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;

            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1.5f);
                }

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<WormGore2>(), 1f);

                int randomGoreCount = Main.rand.Next(0, 1);
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
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 3, 4));
			if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 2));
			}
            }
        }
    }
}