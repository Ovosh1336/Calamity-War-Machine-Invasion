using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAddon.Content
{
    public class DownedBossSystem : ModSystem
    {
        // Флаг: побеждён ли босс
        public static bool downedWulfrumMothership = false;

        // Сброс при создании нового мира
        public override void OnWorldUnload()
        {
            downedWulfrumMothership = false;
        }

        // Сохранение в мир
        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedWulfrumMothership"] = downedWulfrumMothership;
        }

        // Загрузка из мира
        public override void LoadWorldData(TagCompound tag)
        {
            downedWulfrumMothership = tag.GetBool("downedWulfrumMothership");
        }
    }
}