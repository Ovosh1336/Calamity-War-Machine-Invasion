using CalamityAddon.Content.Tiles.BaseTiles;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Tiles.Furniture.BossRelics
{
    public class WulfrumMothershipRelic : BaseBossRelic
    {
        public override string RelicTextureName => "CalamityAddon/Content/Tiles/Furniture/BossRelics/WulfrumMothershipRelic";

        public override int AssociatedItem => ModContent.ItemType<Items.Placeables.Furniture.BossRelics.WulfrumMothershipRelic>();
    }
}