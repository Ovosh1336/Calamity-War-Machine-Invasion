using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Ammo
{
    public class WulfrumLRocket : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName и Tooltip лучше задать через локализацию
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 10;
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 2f;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(0, 0, 0, 50);
            Item.rare = ItemRarityID.Blue;

            // Тип боеприпаса
            Item.ammo = ModContent.ItemType<WulfrumLRocket>();
            // Какой снаряд вылетает, когда оружие использует этот боеприпас
            Item.shoot = ModContent.ProjectileType<Projectiles.WulfrumLRocketProj>();
            Item.shootSpeed = 7f;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.noMelee = true;
        }

        public override void AddRecipes()
        {
        Recipe recipe = Recipe.Create(ModContent.ItemType<WulfrumLRocket>(), 25);
        if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod) && calamityMod.TryFind("WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
            recipe.AddIngredient(wulfrumMetalScrap.Type, 5);
            calamityMod.TryFind("EnergyCore", out ModItem energyCore);
            recipe.AddIngredient(energyCore.Type, 1);
        }
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
        }
    }
}
