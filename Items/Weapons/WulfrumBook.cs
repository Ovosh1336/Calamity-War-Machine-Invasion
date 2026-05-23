using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.Projectiles;
using Terraria.DataStructures;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumBook : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.width = 28;
            Item.height = 34;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(0, 1, 30, 0);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WulfrumMachineProj>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {   
            Vector2 offset = velocity.SafeNormalize(Vector2.Zero) * 35f; 
            
            if (Collision.CanHit(position, 0, 0, position + offset, 0, 0))
            {
                position += offset;
            }

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            return false;
        }
    }
}