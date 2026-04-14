using Terraria.ModLoader;
using CalamityAddon.Content.NPCs.WulfrumMothership;
using CalamityAddon.Content.Items.Summons;
using CalamityAddon.Content.Items.TreasureBags;
using System.Collections.Generic;

namespace CalamityAddon.Content
{
    public class BossChecklistCompat : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                return;
            
            bossChecklistMod.Call(
                "LogBoss",
                Mod,
                nameof(WulfrumMothership), 1.6f, () => DownedBossSystem.downedWulfrumMothership,
                ModContent.NPCType<WulfrumMothership>(),
                new Dictionary<string, object>()
                {
                    ["spawnItems"] = ModContent.ItemType<WulfrumHeart>(),
                    ["treasureBag"] = ModContent.ItemType<WulfrumMothershipBag>(),
                }
            );
        }
    }
}