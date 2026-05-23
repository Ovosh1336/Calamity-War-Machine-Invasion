using CalamityAddon.Content.Events;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Summons
{
	public class UnstableCore : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 20;
			Item.value = 100;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.None;
            Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			return !WulfrumRush.isInvasionActive;
		}

		public override bool? UseItem(Player player)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				WulfrumRush.StartInvasion();
			}
			return true;
		}
	}
}