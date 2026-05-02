using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Projectiles
{
    public class WulfrumMachineProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(5);
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            Projectile.velocity.Y += 0.2f; // Гравитация
            if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

        // Этот метод срабатывает при ударе о блоки
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Возвращаем true, чтобы снаряд уничтожился и вызвался метод Kill()
            return true; 
        }

        public override void Kill(int timeLeft)
        {
            // 1. Звук взрыва
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // 2. Визуальный эффект взрыва (пыль)
            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            for (int k = 0; k < 20; k++) // Изменил i на k, чтобы не было конфликта
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, 0f, 0f, 100, default, 1f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 3f;
                Main.dust[dustIndex].scale = 1.2f;
            }

            // 3. СОЗДАНИЕ ОБЛОМКОВ (Gores)
            // Вынесено из цикла пыли, чтобы не спавнить их сотнями
            int randomGoreCount = Main.rand.Next(2, 4); 
            for (int g = 0; g < randomGoreCount; g++)
            {
                int index = Main.rand.Next(1, 11);
                // Пытаемся найти горы из Calamity
                if (ModContent.TryFind<ModGore>("CalamityMod", "WulfrumEnemyGore" + index, out ModGore calGore))
                {
                    // Используем Projectile.GetSource_Death() и разбрасываем их в разные стороны
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(3f, 3f), calGore.Type, 1f);
                }
            }

            // 4. Логика нанесения урона по площади (ВЗРЫВ)
            // Сохраняем центр, расширяем хитбокс
            Vector2 oldCenter = Projectile.Center;
            Projectile.width = 120; 
            Projectile.height = 120;
            Projectile.Center = oldCenter;

            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            
            Projectile.Damage(); 
        }
    }
}