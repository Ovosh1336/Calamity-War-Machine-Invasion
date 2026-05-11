using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumSword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Melee;
            Item.width = 62;
            Item.height = 66;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8;
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noUseGraphic = true; 
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<WulfrumSwordPlayer>();
            if (player.altFunctionUse == 2)
            {
                if (modPlayer.hitCounter < 3) return false;
                Item.useTime = 20;
                Item.useAnimation = 20;
            }
            else
            {
                Item.useTime = 45;
                Item.useAnimation = 45;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<WulfrumSwordPlayer>();
            if (player.altFunctionUse == 2)
            {
                modPlayer.hitCounter = 0;
                SoundEngine.PlaySound(SoundID.Item14, player.Center);
            }
            return true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var modPlayer = player.GetModPlayer<WulfrumSwordPlayer>();
            if (player.altFunctionUse != 2 && modPlayer.hitCounter < 3)
            {
                modPlayer.hitCounter++;
                for (int i = 0; i < 5; i++)
                    Dust.NewDust(player.position, player.width, player.height, DustID.Electric);
            }
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (player.altFunctionUse == 2) modifiers.FinalDamage *= 2f;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.altFunctionUse == 2 && player.itemAnimation == player.itemAnimationMax - 1)
            {
                player.velocity.X = player.direction * 16f;
                player.immune = true;
                player.immuneTime = 10;
            }
        }
    }

    public class WulfrumSwordPlayer : ModPlayer
    {
        public int hitCounter = 0;
        public override void OnEnterWorld() => hitCounter = 0;
    }

    public class WulfrumSwordDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            Item item = drawPlayer.HeldItem;

            if (item.type != ModContent.ItemType<WulfrumSword>() || drawPlayer.itemAnimation <= 0)
                return;

            WulfrumSwordPlayer modPlayer = drawPlayer.GetModPlayer<WulfrumSwordPlayer>();

            string texturePath = item.ModItem.Texture + "_Held"; 
            if (!ModContent.HasAsset(texturePath)) return;

            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;

            int frameIndex = (drawPlayer.altFunctionUse == 2) ? 4 : modPlayer.hitCounter;
            int frameHeight = texture.Height / 5;
            Rectangle sourceRect = new Rectangle(0, frameHeight * frameIndex, texture.Width, frameHeight);
            
            Vector2 position = drawPlayer.itemLocation - Main.screenPosition;
            Vector2 origin = new Vector2(drawPlayer.direction == 1 ? 0 : sourceRect.Width, sourceRect.Height);

            drawInfo.DrawDataCache.Add(new DrawData(
                texture,
                position,
                sourceRect,
                drawInfo.itemColor,
                drawPlayer.itemRotation,
                origin,
                drawPlayer.GetAdjustedItemScale(item),
                drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            ));
        }
    }
}