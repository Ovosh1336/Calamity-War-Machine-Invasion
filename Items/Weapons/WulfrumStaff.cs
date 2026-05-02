using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using CalamityAddon.Content.Projectiles.Minions;
using CalamityAddon.Content.Buffs;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumStaff : ModItem
    {
        public override void SetDefaults()
        {
            // Базовые параметры оружия
            Item.damage = 16;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 16;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;

            // Параметры призыва
            Item.shoot = ModContent.ProjectileType<WulfrumTurret>();
            Item.buffType = ModContent.BuffType<WulfrumTurretBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Спавн рядом с игроком
            position = player.Center;
            velocity = Vector2.Zero;
        }
        public override bool CanUseItem(Player player)
        {

            return player.ownedProjectileCounts[ModContent.ProjectileType<WulfrumTurret>()] == 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Добавляем бафф
            player.AddBuff(Item.buffType, 2);

            // Создаём турель
            var projectile = Projectile.NewProjectileDirect(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            projectile.originalDamage = Item.damage;
            projectile.ContinuouslyUpdateDamageStats = false;

            return false;
        }
    }
}