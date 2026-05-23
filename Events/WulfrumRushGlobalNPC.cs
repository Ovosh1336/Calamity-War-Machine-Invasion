using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.NPCs;

namespace CalamityAddon.Content.Events
{
    internal class WulfrumRushGlobalNPC : GlobalNPC
    {
        public static int[] invasionMobs = { ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumDrone>(),
                                             ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumHovercraft>(),
                                             ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumRover>(),
                                             ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumGyrator>()
                                             }; //мобы в нашествии

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (WulfrumRush.isInvasionActive)
            {
                pool.Clear();
                foreach (int mobID in invasionMobs)
                {
                    pool.Add(mobID, 1f);
                }
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (WulfrumRush.isInvasionActive)
            {
                spawnRate = 60;
                maxSpawns = 30;
            }
        }

        public override void PostAI(NPC npc)
        {
            if (WulfrumRush.isInvasionActive)
            {
                npc.timeLeft = 1000;
                npc.TargetClosest();
            }
        }

        public override void OnKill(NPC npc)
        {
            if (WulfrumRush.isInvasionActive)
            {
                foreach (int mobID in invasionMobs)
                {
                    if (npc.type == mobID)
                    {
                        WulfrumRush.invasionKills++;
                        break;
                    }
                }
            }
        }
    }
}