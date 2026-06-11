using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityAddon.Content.NPCs.WulfrumJumper;
using Terraria.Chat;

namespace CalamityAddon.Content.Events
{
    internal class WulfrumRush : ModSystem
    {
        public static bool isInvasionActive = false;
        public static int invasionKills = 0;
        public static int invasionMaxProgress = 100;
        public const int CustomInvasionType = -67;

        private static bool AmplifiersSpawned = false;

        // Синхронизация данных между игроками
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(isInvasionActive);
            writer.Write(invasionKills);
            writer.Write(AmplifiersSpawned);
        }

        public override void NetReceive(BinaryReader reader)
        {
            isInvasionActive = reader.ReadBoolean();
            invasionKills = reader.ReadInt32();
            AmplifiersSpawned = reader.ReadBoolean();
        }

        public override void PostUpdateInvasions()
        {
            if ((Main.invasionType != 0 && Main.invasionType != CustomInvasionType) || Main.pumpkinMoon || Main.snowMoon) return;

            if (isInvasionActive)
            {
                UpdateInvasion();
            }
        }

        public static void StartInvasion()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            isInvasionActive = true;
            invasionKills = 0;
            AmplifiersSpawned = false;

            Main.invasionType = CustomInvasionType;
            Main.invasionSize = invasionMaxProgress;
            Main.invasionProgress = 0;
            Main.invasionProgressMax = invasionMaxProgress;
            Main.invasionWarn = 600;

            string message = "Вы чувствуете движение металла вокруг себя";
            Color color = new Color(175, 75, 255);

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), color);
            else
                Main.NewText(message, color);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private void UpdateInvasion()
        {
            Main.invasionProgress = invasionKills;
            Main.invasionProgressMax = invasionMaxProgress;

            if (Main.netMode != NetmodeID.Server)
            {
                Main.ReportInvasionProgress(invasionKills, invasionMaxProgress, Main.invasionProgressIcon, 0);
            }

            // Спавн Усилителя на 50% прогресса
            if (Main.netMode != NetmodeID.MultiplayerClient && invasionKills >= 50 && !AmplifiersSpawned)
            {
                int targetPlayer = Player.FindClosest(new Vector2(Main.maxTilesX / 2, Main.maxTilesY / 2) * 16, 0, 0);
                if (targetPlayer != -1)
                {
                    NPC.SpawnOnPlayer(targetPlayer, ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumAmplifier>());
                    AmplifiersSpawned = true;
                    if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.WorldData);
                }
            }

            if (invasionKills >= invasionMaxProgress)
            {
                EndInvasion();
            }
        }

        private void EndInvasion()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            DownedBossSystem.downedWulfrumRush = true;
            isInvasionActive = false;
            Main.invasionType = 0;
            Main.invasionSize = 0;
            Main.invasionWarn = 0;

            string message = "Тяжелая артиллерия!";
            Color color = new Color(50, 255, 130);

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), color);
            else
                Main.NewText(message, color);

            // Поиск игрока для спавна босса
            int targetPlayer = -1;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead)
                {
                    targetPlayer = i;
                    break;
                }
            }

            if (targetPlayer != -1)
            {
                NPC.SpawnOnPlayer(targetPlayer, ModContent.NPCType<WulfrumJumper>());
            }

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }
    }
}