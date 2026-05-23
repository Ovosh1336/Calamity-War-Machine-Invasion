using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using System.Reflection;
using CalamityMod;

namespace CalamityAddon.Content
{
    public class MasterOrRevengeanceCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            if (Main.masterMode || CalamityMod.World.CalamityWorld.revenge) return true;
            return false;
        }

        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => "Мастер или месть";
    }
}