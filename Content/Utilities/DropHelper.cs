using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using System;
using Terraria.Localization;

namespace CalamityAddon.Content.Utilities
{
    public static class DropHelper
    {
        // Берем готовый текст первого убийства прямо из локализации Calamity
        public static LocalizedText FirstKillText => Language.GetText("Mods.CalamityMod.DropConditions.FirstKill");

        // Метод для добавления лута, который падает каждому игроку при условии
        public static IItemDropRule AddConditionalPerPlayer(this NPCLoot npcLoot, Func<bool> condition, int itemID, string desc = "")
        {
            var rule = new DropPerPlayerOnThePlayer(itemID, 1, 1, 1, new FuncDropCondition(condition, desc));
            npcLoot.Add(rule);
            return rule;
        }
    }

    // Само условие
    public class FuncDropCondition : IItemDropRuleCondition
    {
        private readonly Func<bool> _condition;
        private readonly string _desc;

        public FuncDropCondition(Func<bool> condition, string desc)
        {
            _condition = condition;
            _desc = desc;
        }

        public bool CanDrop(DropAttemptInfo info) => _condition();
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => _desc;
    }
}