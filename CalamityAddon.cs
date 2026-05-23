using CalamityAddon.Content.Events;
using CalamityAddon.Content.Items.Summons;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAddon
{
    public class CalamityAddon : Mod
    {
        private static Hook rightClickHook;
        private static Func<Func<object, int, int, bool>, object, int, int, bool> ourDetour;

        public override void Load()
        {
            Type targetTileType = Type.GetType("CalamityMod.Tiles.Furniture.WulfrumLure, CalamityMod");
            if (targetTileType != null)
            {
                MethodInfo targetMethod = targetTileType.GetMethod("RightClick", new Type[] { typeof(int), typeof(int) });
                if (targetMethod != null)
                {
                    ourDetour = RightClick_Detour;
                    rightClickHook = new Hook(targetMethod, ourDetour);
                }
            }
        }

        public override void Unload()
        {
            rightClickHook?.Dispose();
        }
        private static bool RightClick_Detour(Func<object, int, int, bool> orig, object self, int i, int j)
        {
            if (Main.LocalPlayer.HasItem(ModContent.ItemType<UnstableCore>()) && !WulfrumRush.isInvasionActive)
            {
                WulfrumRush.StartInvasion();
                Main.LocalPlayer.ConsumeItem(ModContent.ItemType<UnstableCore>(), true); //Замени трубу на свою призывалку
                return true;
            }
            else
                return orig(self, i, j);
        }
    }
}