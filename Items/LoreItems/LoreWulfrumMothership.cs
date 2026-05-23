using Terraria.ID;

namespace CalamityAddon.Content.Items.LoreItems
{
    public class LoreWulfrumMothership : LoreItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.rare = ItemRarityID.Green;
            Item.consumable = false;
            Item.value = 0;
        }
    }
}