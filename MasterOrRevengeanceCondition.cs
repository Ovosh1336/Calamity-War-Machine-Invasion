using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using System.Reflection;

namespace CalamityAddon.Content
{
    public class MasterOrRevengeanceCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            if (Main.masterMode) return true;

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                var calWorldType = calamity.Code.GetType("CalamityMod.World.CalamityWorld");
                if (calWorldType != null)
                {
                    var revengeField = calWorldType.GetField("revenge", BindingFlags.Public | BindingFlags.Static);
                    if (revengeField != null)
                        return (bool)revengeField.GetValue(null);
                }
            }

            return false;
        }

        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => "Мастер или месть";
    }
}