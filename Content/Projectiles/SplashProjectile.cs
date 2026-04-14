using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityAddon.Content.Projectiles
{
    public class SplashProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Explosive Shot"); // Для 1.4.4 это делается в локализации
            Main.projFrames[Projectile.type] = 1; 
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;  // Ширина хитбокса самого снаряда
            Projectile.height = 8; // Высота хитбокса
            
            // --- НАСТРОЙКИ ТИПА ---
            Projectile.aiStyle = -1; 
            Projectile.friendly = false; 
            Projectile.hostile = true;   
            
            Projectile.penetrate = 1; // Исчезает после 1 попадания
            Projectile.timeLeft = 300; // Время жизни (5 секунд)
            Projectile.tileCollide = true; // Сталкивается со стенами
        }

        public override void AI()
        {
            // 1. Поворот спрайта по направлению полета
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 2. Визуальный эффект (шлейф/дым)
            if (Main.rand.NextBool(2)) // 50% шанс каждый кадр
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
                Main.dust[dust].velocity *= 0.5f; // Дым отстает от снаряда
                Main.dust[dust].noGravity = true;
            }

            // 3. (Опционально) Гравитация
            // Projectile.velocity.Y += 0.1f; 
        }

        // Вызывается при ударе об блок
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill(); // Вызываем смерть снаряда (взрыв)
            return false;
        }

        // Вызывается при попадании в Игрока (если hostile) или NPC (если friendly)
        // Но мы хотим нанести урон через Kill(), поэтому здесь просто убиваем снаряд
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.Kill();
        }

        // Основная логика взрыва
        public override void Kill(int timeLeft)
        {
            // === 1. ВИЗУАЛ ВЗРЫВА ===
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position); // Звук взрыва

            // Пыль (Огонь и Дым)
            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            for (int i = 0; i < 20; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 5f;
                dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 3f;
            }

            // === 2. ЛОГИКА СПЛЕШ УРОНА ===
            float explosionRadius = 200f; // Радиус взрыва в пикселях (~12 блоков)

            // Если снаряд ВРАЖДЕБНЫЙ (от Босса) -> Бьем ИГРОКОВ
            if (Projectile.hostile)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player target = Main.player[i];
                    if (target.active && !target.dead)
                    {
                        float distance = Vector2.Distance(Projectile.Center, target.Center);
                        if (distance <= explosionRadius)
                        {
                            // Проверка стен (чтобы не бил сквозь блоки)
                            if (Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1))
                            {
                                int direction = target.Center.X < Projectile.Center.X ? -1 : 1;
                                target.Hurt(PlayerDeathReason.ByProjectile(Main.myPlayer, Projectile.whoAmI), Projectile.damage, direction);
                            }
                        }
                    }
                }
            }
        }
    }
}