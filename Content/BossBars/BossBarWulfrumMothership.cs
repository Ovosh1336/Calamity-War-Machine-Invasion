using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.BigProgressBar;
using CalamityAddon.Content.NPCs.WulfrumMothership;

namespace CalamityAddon.Content.BossBars
{
    public class BossBarWulfrumMothership : ModBossBar
    {
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            return ModContent.Request<Texture2D>("CalamityAddon/Content/NPCs/WulfrumMothership/WulfrumMothership_Head_Boss");
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];

            if (!npc.active || npc.type != ModContent.NPCType<WulfrumMothership>())
                return false;

            life = npc.life;
            lifeMax = npc.lifeMax;

            // Получаем данные щита из босса
            if (npc.ModNPC is WulfrumMothership mothership)
            {
                shield = mothership.GetShieldHP();
                shieldMax = mothership.GetShieldMaxHP();
            }

            return true;
        }
    }
}