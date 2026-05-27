using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Materials
{
    public class WulfrumPipe : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.None;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.autoReuse = false;
            Item.createTile = -1;
        }

        public override void AddRecipes()
        {
            // Пусто — предмет не крафтится
        }
    }
}
