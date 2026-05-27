using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace CalamityAddon.Content
{
    public class SuperchargedCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return info.npc.ai[3] > 0;
        }

        public bool CanShowItemDropInUI() => false;
        public string GetConditionDescription() => "";
    }
}
