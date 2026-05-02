using CalamityAddon.Content.Tiles.Banners;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Placeables.Banners
{
	public class WulfrumTankBanner : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), (int)EnemyBanner.StyleID.WulfrumTank);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
}