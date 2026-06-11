using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.NPCs;

namespace CalamityAddon.Content.Events
{
    internal class WulfrumRushGlobalNPC : GlobalNPC
    {
        public static int[] BaseInvasionMobs => new int[] {
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumDrone>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumHovercraft>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumRover>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumGyrator>()
        };

        private bool IsInvasionMob(int type)
        {
            return BaseInvasionMobs.Contains(type) ||
                   type == ModContent.NPCType<WulfrumTank>() ||
                   type == ModContent.NPCType<WulfrumBomber>();
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (WulfrumRush.isInvasionActive)
            {
                if (IsInvasionMob(npc.type))
                {
                    WulfrumRush.invasionKills++;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.WorldData);
                    }
                }
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (WulfrumRush.isInvasionActive)
            {
                pool.Clear();
                foreach (int mobID in BaseInvasionMobs)
                {
                    pool.Add(mobID, 1f);
                }

                if (WulfrumRush.invasionKills > 50)
                {
                    pool.Add(ModContent.NPCType<WulfrumBomber>(), 0.6f);
                }

                if (WulfrumRush.invasionKills > 75)
                {
                    pool.Add(ModContent.NPCType<WulfrumTank>(), 0.5f);
                }
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (WulfrumRush.isInvasionActive)
            {
                spawnRate = 75;
                maxSpawns = 30;
            }
        }
    }
}