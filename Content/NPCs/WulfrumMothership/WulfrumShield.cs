// NOT USING //

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ReLogic.Content;
using System;

namespace CalamityAddon.Content.NPCs.WulfrumMothership
{
    public class WulfrumShield : ModNPC
    {
        private static Asset<Texture2D> noiseTex;
        private static Effect shieldEffect;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 90;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 300; 
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.boss = false; 
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/RoverDriveBreak");
        }

        public override void AI()
        {
            int bossIndex = (int)NPC.ai[0];

            if (bossIndex < 0 || bossIndex >= Main.maxNPCs || !Main.npc[bossIndex].active || Main.npc[bossIndex].type != ModContent.NPCType<WulfrumMothership>())
            {
                NPC.life = 0;
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            NPC.Center = Main.npc[bossIndex].Center;
            //NPC.velocity = Main.npc[bossIndex].velocity;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            float boost = 1f + (numPlayers - 1) * 0.35f;

            NPC.lifeMax = (int)(NPC.lifeMax * balance * boost);
            
            NPC.life = NPC.lifeMax;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawShieldEffect(spriteBatch, screenPos);
            return false;
        }

        private void DrawShieldEffect(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (shieldEffect == null)
            {
                shieldEffect = ModContent.Request<Effect>("CalamityAddon/Content/Effects/RoverDriveShield", AssetRequestMode.ImmediateLoad).Value;
            }

            if (noiseTex == null)
            {
                noiseTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/TechyNoise");
            }

            float healthRatio = (float)NPC.life / NPC.lifeMax;

            shieldEffect.Parameters["time"]?.SetValue(Main.GlobalTimeWrappedHourly * 0.24f);
            shieldEffect.Parameters["blowUpPower"]?.SetValue(2.5f);
            shieldEffect.Parameters["blowUpSize"]?.SetValue(0.5f);
            shieldEffect.Parameters["noiseScale"]?.SetValue(0.5f);
            shieldEffect.Parameters["shieldOpacity"]?.SetValue(0.8f * healthRatio);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"]?.SetValue(4f);

            Color greenTint = new Color(100, 255, 180);
            shieldEffect.Parameters["shieldColor"]?.SetValue(greenTint.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"]?.SetValue(Color.Cyan.ToVector3());

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

            Texture2D tex = noiseTex.Value;
            Vector2 pos = NPC.Center - screenPos;
            spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, NPC.scale * 0.25f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric);
        }
    }
}