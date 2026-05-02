using CalamityAddon.Content.Items.Ammo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.Items.Weapons;

namespace CalamityAddon.Content.Items.TreasureBags
{
    public class WulfrumMothershipBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.4f);

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, Color.White.ToVector3() * 0.4f);

            if (Item.timeSinceItemSpawned % 12 == 0)
            {
                Vector2 center = Item.Center + new Vector2(0f, Item.height * -0.1f);
                Vector2 direction = Main.rand.NextVector2CircularEdge(Item.width * 0.6f, Item.height * 0.6f);
                float distance = 0.3f + Main.rand.NextFloat() * 0.5f;
                Vector2 velocity = new Vector2(0f, -Main.rand.NextFloat() * 0.3f - 1.5f);

                Dust dust = Dust.NewDustPerfect(center + direction * distance, DustID.SilverFlame, velocity);
                dust.scale = 0.5f;
                dust.fadeIn = 1.1f;
                dust.noGravity = true;
                dust.noLight = true;
                dust.alpha = 0;
            }
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Монеты
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(
                ModContent.NPCType<NPCs.WulfrumMothership.WulfrumMothership>()));

            // Материалы и гарантированные предметы
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumLRocket>(), 1, 75, 100));

            // Пул оружия
            int[] weaponPool = new int[] {
                ModContent.ItemType<WulfrumSword>(),
                ModContent.ItemType<WulfrumLuncher>(),
                ModContent.ItemType<WulfrumBook>(),
                ModContent.ItemType<WulfrumStaff>()
            };

            foreach (int weaponID in weaponPool) {
                itemLoot.Add(ItemDropRule.Common(weaponID, 2)); // Шанс 1 из 2 на каждое оружие
            }

            // Аксессуары из Calamity
            if (ModContent.TryFind("CalamityMod", "RoverDrive", out ModItem shield))
                itemLoot.Add(ItemDropRule.Common(shield.Type, 3));
            if (ModContent.TryFind("CalamityMod", "WulfrumBattery", out ModItem battery))
                itemLoot.Add(ItemDropRule.Common(battery.Type, 3));
        }
    }
}