using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.LoreItems
{
    public class LoreWulfrumMothership : LoreItem
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.rare = ItemRarityID.Green;
            Item.consumable = false;
        }
    }
}