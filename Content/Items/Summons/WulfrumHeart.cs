using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityAddon.Content.NPCs.WulfrumMothership;

namespace CalamityAddon.Content.Items.Summons
{
    public class WulfrumHeart : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.maxStack = 20;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<WulfrumMothership>()) && Main.dayTime && player.ZoneOverworldHeight;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.position);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<WulfrumMothership>());
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod)) 
            {
                if (calamityMod.TryFind("WulfrumMetalScrap", out ModItem wulfrumMetalScrap))
                    recipe.AddIngredient(wulfrumMetalScrap.Type, 15);
                if (calamityMod.TryFind("EnergyCore", out ModItem energyCore))
                    recipe.AddIngredient(energyCore.Type, 3);
            }
            
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}