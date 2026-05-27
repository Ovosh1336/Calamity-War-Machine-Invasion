using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityAddon.Content.NPCs.WulfrumMothership;
using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAddon.Content.Events
{
    internal class WulfrumRush : ModSystem
    {
        public static bool isInvasionActive = false;
        public static int invasionKills = 0;
        public static int invasionMaxProgress = 150;
        public const int CustomInvasionType = -67;

        //private bool AmplifiersSpawned = false;
        //private bool AmplifiersSpawned2 = false;

        public override void PostUpdateInvasions()
        {
            if ((Main.invasionType != 0 && Main.invasionType != CustomInvasionType) || Main.pumpkinMoon || Main.snowMoon) return;
            if (isInvasionActive)
            {
                UpdateInvasion();
            }
            //else
            //{
            //TryStartInvasion();
            //}
        }

        //        private void TryStartInvasion()
        //        {
        //            bool playerHasEnoughHP = false;
        //            foreach (Player player in Main.player)
        //            {
        //                if (player.active && player.statLifeMax2 > 200)
        //                {
        //                    playerHasEnoughHP = true;
        //                    break;
        //                }
        //            }
        //            if (!playerHasEnoughHP) return;
        //
        //            if (Main.rand.NextBool(108000))
        //            {
        //                StartInvasion();
        //            }
        //        }

        public static void StartInvasion()
        {
            isInvasionActive = true;
            invasionKills = 0;

            Main.invasionType = CustomInvasionType;
            Main.invasionSize = invasionMaxProgress;
            Main.invasionProgress = 0;
            Main.invasionProgressMax = invasionMaxProgress;
            Main.invasionProgressIcon = 0;
            Main.invasionProgressWave = 0;
            Main.invasionWarn = 600;

            if (Main.netMode == 0)
                Main.NewText("Вы чуствуете движение металла вокруг себя", 175, 75, 255);
        }

        private void UpdateInvasion()
        {
            Main.invasionProgress = invasionKills;
            Main.invasionProgressMax = invasionMaxProgress;
            Main.ReportInvasionProgress(invasionKills, invasionMaxProgress,
                Main.invasionProgressIcon, 0);

            if (invasionKills == 50) //&& !AmplifiersSpawned)
            {
                Player player = Main.LocalPlayer;
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumAmplifier>());
                //AmplifiersSpawned = true;
            }
            if (invasionKills == 100) //&& !AmplifiersSpawned2)
            {
                Player player = Main.LocalPlayer;
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumAmplifier>());
                //AmplifiersSpawned2 = true;
            }

            if (invasionKills >= invasionMaxProgress)
            {
                EndInvasion();
            }

        }

        private void EndInvasion()
        {
            Player player = Main.LocalPlayer;
            DownedBossSystem.downedWulfrumRush = true;
            isInvasionActive = false;
            Main.invasionType = 0;
            Main.invasionSize = 0;
            Main.invasionWarn = 0;

            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.position);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<WulfrumMothership>());
            }

            if (Main.netMode == 0)
                Main.NewText("Тяжелая артеллерия!", 50, 255, 130);
        }
    }
}