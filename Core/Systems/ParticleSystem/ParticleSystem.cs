using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Arch.Core;
using Arch.Core.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Entity = Arch.Core.Entity;
using ECSParticle.Core.Systems.ParticleSystem;

namespace ECSParticle.Core.Systems.ParticleSystem
{

    [Autoload(Side = ModSide.Client)]
    public sealed class ParticleSystem : ModSystem
    {
        public static ParticleSystem Instance { get; private set; }
        public World ParticleWorld { get; } = World.Create();
        public int ParticleCount { get; private set; }

        public override void OnModLoad()
        {
            Instance = ModContent.GetInstance<ParticleSystem>();
            On_Main.UpdateParticleSystems += UpdateParticles;
            On_Main.DoDraw_DrawNPCsOverTiles += DrawParticlesUnderEntities;
            On_Main.DrawDust += DrawParticles;
        }

        public override void OnModUnload()
        {
            On_Main.UpdateParticleSystems -= UpdateParticles;
            On_Main.DoDraw_DrawNPCsOverTiles -= DrawParticlesUnderEntities;
            On_Main.DrawDust -= DrawParticles;
            Instance = null;
        }
        public override void OnWorldUnload()
        {
            var query = new QueryDescription().WithAll<Particle>();
            ParticleWorld.Query(
                in query,
                (in Entity entity) =>
                {
                    ParticleWorld.Destroy(entity);
                    ParticleCount--;
                }
            );
        }

        public void UpdateParticles()
        {
            if (Main.dedServ || Main.gamePaused || Main.netMode == NetmodeID.Server)
                return;

            var query = new QueryDescription().WithAll<Particle, ParticlePosition, ParticleVelocity, ParticleActive>();
            ParticleWorld.Query(
                in query,
                (in Entity entity) =>
                {
                    ref var particle = ref entity.Get<Particle>();
                    ref var position = ref entity.Get<ParticlePosition>();
                    ref var velocity = ref entity.Get<ParticleVelocity>();
                    ref var active = ref entity.Get<ParticleActive>();

                    particle.Value.Update(in entity);
                    position.Value += velocity.Value;
                    if (!active.Value)
                    {
                        ParticleWorld.Destroy(entity);
                        ParticleCount--;
                    }
                }
            );

            // Main.NewText($"Particle count: {ParticleCount}");
        }

        public void DrawParticlesUnderEntities(SpriteBatch spriteBatch)
        {
            if (Main.dedServ || Main.gameMenu || Main.netMode == NetmodeID.Server)
                return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            float drawMinX = Main.screenPosition.X - Main.screenWidth / 2;
            float drawMaxX = Main.screenPosition.X + Main.screenWidth * 3 / 2;
            float drawMinY = Main.screenPosition.Y - Main.screenHeight / 2;
            float drawMaxY = Main.screenPosition.Y + Main.screenHeight * 3 / 2;

            var query = new QueryDescription().WithAll<Particle, ParticlePosition, ParticleDrawBehindEntities>();
            ParticleWorld.Query(
                in query,
                (in Entity entity) =>
                {
                    ref var particle = ref entity.Get<Particle>();
                    ref var position = ref entity.Get<ParticlePosition>();

                    if (position.Value.X > drawMinX && position.Value.X < drawMaxX &&
                        position.Value.Y > drawMinY && position.Value.Y < drawMaxY)
                        particle.Value.Draw(in entity, spriteBatch);
                }
            );

            spriteBatch.End();
        }

        public void DrawParticles(SpriteBatch spriteBatch)
        {
            if (Main.dedServ || Main.gameMenu || Main.netMode == NetmodeID.Server)
                return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            float drawMinX = Main.screenPosition.X - Main.screenWidth / 2;
            float drawMaxX = Main.screenPosition.X + Main.screenWidth * 3 / 2;
            float drawMinY = Main.screenPosition.Y - Main.screenHeight / 2;
            float drawMaxY = Main.screenPosition.Y + Main.screenHeight * 3 / 2;

            var query = new QueryDescription().WithAll<Particle, ParticlePosition>().WithNone<ParticleDrawBehindEntities>();
            ParticleWorld.Query(
                in query,
                (in Entity entity) =>
                {
                    ref var particle = ref entity.Get<Particle>();
                    ref var position = ref entity.Get<ParticlePosition>();

                    if (position.Value.X > drawMinX && position.Value.X < drawMaxX &&
                        position.Value.Y > drawMinY && position.Value.Y < drawMaxY)
                        particle.Value.Draw(in entity, spriteBatch);
                }
            );

            spriteBatch.End();
        }

        private void DrawParticlesUnderEntities(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
        {
            DrawParticlesUnderEntities(Main.spriteBatch);
            orig(self);
        }

        private void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            DrawParticles(Main.spriteBatch);
            orig(self);
        }

        private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);
            UpdateParticles();
        }
        public static Entity NewParticle(ParticleEntity particleEntity, Vector2 position, Vector2 velocity, Color color, float scale = 1f)
        {
            if (Main.gamePaused || Main.dedServ)
                return Entity.Null;

            var particle = new Particle { Value = particleEntity };
            var particlePosition = new ParticlePosition { Value = position };
            var particleVelocity = new ParticleVelocity { Value = velocity };
            var particleColor = new ParticleColor { Value = color };
            var particleScale = new ParticleScale { Value = scale };
            var particleRotation = new ParticleRotation { Value = velocity.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f) * MathHelper.TwoPi };
            var particleActive = new ParticleActive { Value = true };
            var entity = Instance.ParticleWorld.Create(particle, particlePosition, particleVelocity, particleColor, particleScale, particleRotation, particleActive);
            Instance.ParticleCount++;
            particleEntity.OnSpawn(in entity);
            return entity;
        }
    }
}
