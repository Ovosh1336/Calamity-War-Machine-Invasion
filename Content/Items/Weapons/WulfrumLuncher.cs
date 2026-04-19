using CalamityAddon.Content.Items.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumLuncher : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName и Tooltip лучше задать через локализацию
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 18;
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.WulfrumLRocketProj>();
            Item.shootSpeed = 3f;

            Item.useAmmo = ModContent.ItemType<WulfrumLRocket>();

            Item.value = Item.buyPrice(0, 1, 20, 0);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item11;
        }

        public override void UseItemFrame(Player player)
        {
            // Thank you Mr. IbanPlay (CoralSprout.cs)
            // Calculate the dirction in which the players arms should be pointing at.
            float armPointingDirection = player.itemRotation;
            if (player.direction < 0)
                armPointingDirection += MathHelper.Pi;

            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-18f, -3f);
    }
}

// 1-Спрайт развернут | +
// 2-Переисовать рокетлунчер | +
// 3-Сделать ИИ Вульфрумовой ракете и саму ракету | +
// 4-Добавить в крафт вульфрумовые обломки | +
// 5-Добавить босса с которого падает ракетомет| +
// Это пиздец | +++