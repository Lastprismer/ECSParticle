#nullable enable

using System;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Entity = Arch.Core.Entity;

namespace ECSParticle.Core.Systems.ParticleSystem;

[Autoload(Side = ModSide.Client)]
public class ParticleSystem : ModSystem
{
    public World ParticleWorld => World.Create();
    public int ParticleCount { get; private set; }

    public override void OnModLoad()
    {
        On_Main.UpdateParticleSystems += UpdateParticles;
        On_Main.DoDraw_DrawNPCsOverTiles += DrawParticlesUnderEntities;
        On_Main.DrawDust += DrawParticles;
    }

    public override void OnModUnload()
    {
        On_Main.UpdateParticleSystems -= UpdateParticles;
        On_Main.DoDraw_DrawNPCsOverTiles -= DrawParticlesUnderEntities;
        On_Main.DrawDust -= DrawParticles;
    }
    public override void OnWorldUnload()
    {
        base.OnWorldUnload();
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
        Main.NewText(ParticleCount);
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
    }

    public void DrawParticlesUnderEntities(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || Main.gameMenu || Main.netMode == NetmodeID.Server)
            return;

        float drawMinX = Main.screenPosition.X - Main.screenWidth / 2;
        float drawMinY = Main.screenPosition.Y - Main.screenHeight / 2;
        float drawMaxX = Main.screenPosition.X + 3 * Main.screenWidth / 2;
        float drawMaxY = Main.screenPosition.Y + 3 * Main.screenHeight / 2;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var query = new QueryDescription().WithAll<Particle, ParticlePosition, ParticleDrawBehindEntities>();
        ParticleWorld.Query(
            in query,
            (in Entity entity) =>
            {
                ref var particle = ref entity.Get<Particle>();
                ref var position = ref entity.Get<ParticlePosition>();

                if (position.Value.X < drawMaxX && position.Value.X > drawMinX &&
                    position.Value.Y < drawMaxY && position.Value.Y > drawMinY)
                    particle.Value.Draw(in entity, spriteBatch);
            }
        );

        spriteBatch.End();
    }

    public void DrawParticles(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || Main.gameMenu || Main.netMode == NetmodeID.Server)
            return;

        float drawMinX = Main.screenPosition.X - Main.screenWidth / 2;
        float drawMinY = Main.screenPosition.Y - Main.screenHeight / 2;
        float drawMaxX = Main.screenPosition.X + 3 * Main.screenWidth / 2;
        float drawMaxY = Main.screenPosition.Y + 3 * Main.screenHeight / 2;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var query = new QueryDescription().WithAll<Particle, ParticlePosition>().WithNone<ParticleDrawBehindEntities>();
        ParticleWorld.Query(
            in query,
            (in Entity entity) =>
            {
                ref var particle = ref entity.Get<Particle>();
                ref var position = ref entity.Get<ParticlePosition>();

                if (position.Value.X < drawMaxX && position.Value.X > drawMinX &&
                    position.Value.Y < drawMaxY && position.Value.Y > drawMinY)
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

    public static Entity NewParticle(ParticleEntity particleBehavior, Vector2 position, Vector2 velocity, Color color, float scale = 1f, float rotation = 0)
    {
        if (Main.gamePaused || Main.dedServ || ModContent.GetInstance<ParticleSystem>() is not { } particleSystem)
            return Entity.Null;

        var particle = new Particle { Value = particleBehavior };
        var particlePosition = new ParticlePosition { Value = position };
        var particleVelocity = new ParticleVelocity { Value = velocity };
        var particleColor = new ParticleColor { Value = color };
        var particleScale = new ParticleScale { Value = scale };
        var particleRotation = new ParticleRotation { Value = rotation };
        var particleActive = new ParticleActive { Value = true };
        var entity = particleSystem.ParticleWorld.Create(particle, particlePosition, particleVelocity, particleColor, particleScale, particleRotation, particleActive);
        particleSystem.ParticleCount++;
        particleBehavior.OnSpawn(in entity);
        return entity;
    }
}
