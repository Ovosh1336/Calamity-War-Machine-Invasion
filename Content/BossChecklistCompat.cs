using Terraria.ModLoader;
using Terraria.Localization;
using CalamityAddon.Content.NPCs.WulfrumMothership;
using CalamityAddon.Content.Items.Summons;
using CalamityAddon.Content.Items.TreasureBags;
using CalamityAddon.Content.Items.Placeables.Furniture.BossRelics;
using System.Collections.Generic;

namespace CalamityAddon.Content
{
    public class BossChecklistCompat : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                return;
            
            LocalizedText spawnInfo = Language.GetOrRegister("Mods.CalamityAddon.NPCs.WulfrumMothership.SpawnInfo");
            LocalizedText despawnMsg = Language.GetOrRegister("Mods.CalamityAddon.NPCs.WulfrumMothership.DespawnMessage");
            
            bossChecklistMod.Call(
                "LogBoss",
                Mod,
                nameof(WulfrumMothership), 1.6f, () => DownedBossSystem.downedWulfrumMothership,
                ModContent.NPCType<WulfrumMothership>(),
                new Dictionary<string, object>()
                {
                    ["spawnInfo"] = spawnInfo,
                    ["despawnMessage"] = despawnMsg,
                    ["spawnItems"] = ModContent.ItemType<WulfrumHeart>(),
                    ["treasureBag"] = ModContent.ItemType<WulfrumMothershipBag>(),
                    ["availability"] = () => true,
                    ["collectibles"] = new List<int>() 
                    { 
                        ModContent.ItemType<WulfrumMothershipRelic>(),
                        ModContent.ItemType<Items.LoreItems.LoreWulfrumMothership>()
                    }
                }
            );
        }
    }
}
