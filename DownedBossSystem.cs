using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAddon.Content
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedWulfrumMothership = false;

        public override void OnWorldUnload()
        {
            downedWulfrumMothership = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedWulfrumMothership"] = downedWulfrumMothership;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedWulfrumMothership = tag.GetBool("downedWulfrumMothership");
        }
    }
}