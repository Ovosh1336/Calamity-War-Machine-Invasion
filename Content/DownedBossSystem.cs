using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAddon.Content
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedWulfrumMothership = false;
        public static bool downedWulfrumRush = false;

        public override void OnWorldUnload()
        {
            downedWulfrumMothership = false;
            downedWulfrumRush = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedWulfrumMothership"] = downedWulfrumMothership;
            tag["downedWulfrumRush"] = downedWulfrumRush;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedWulfrumMothership = tag.GetBool("downedWulfrumMothership");
            downedWulfrumRush = tag.GetBool("downedWulfrumRush");
        }
    }
}