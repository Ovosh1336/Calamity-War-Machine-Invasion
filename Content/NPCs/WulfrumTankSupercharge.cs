using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CalamityAddon.Content.NPCs
{
    public class WulfrumTankSupercharge : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return false;

            if (calamity.TryFind<ModNPC>("WulfrumAmplifier", out ModNPC amplifier))
                return entity.type == amplifier.Type;

            return false;
        }

        public override void PostAI(NPC npc)
        {
            float chargeRadius = 800f;
            int superchargeTime = 600;

            foreach (var other in Main.ActiveNPCs)
            {
                if (other.type != ModContent.NPCType<WulfrumTank>())
                    continue;
                if (other.ai[3] > 0f)
                    continue;
                if (npc.Distance(other.Center) > chargeRadius)
                    continue;

                other.ai[3] = superchargeTime;
                other.netUpdate = true;

                if (!Main.dedServ)
                {
                    for (int j = 0; j < 10; j++)
                        Dust.NewDust(other.position, other.width, other.height, Terraria.ID.DustID.Electric);
                }
            }
        }
    }
}