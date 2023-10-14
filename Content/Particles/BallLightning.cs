using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using ECSParticle.Core.Systems.ParticleSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ECSParticle.Content.Particles
{
    public struct BallLightningData
    {
        public int FrameCounter { get; set; }
        public int Frame { get; set; }
        public int Timer { get; set; }
    }
    public class BallLightning : ParticleEntity
    {
        public const int FRAME_NUM = 5;
        public const int FRAME_DLT = 10;
        public const int MAX_TIME_LEFT = 240;
        public override void OnSpawn(in Arch.Core.Entity entity)
        {
            entity.Add(new BallLightningData { Frame = 0, FrameCounter = 0, Timer = 0 });

        }
        public override void Update(in Arch.Core.Entity entity)
        {
            // 获取数据的方法
            ref var velocity = ref entity.Get<ParticleVelocity>();
            ref var rotation = ref entity.Get<ParticleRotation>();
            ref var position = ref entity.Get<ParticlePosition>();
            ref var scale = ref entity.Get<ParticleScale>();
            ref var active = ref entity.Get<ParticleActive>();
            ref var owner = ref entity.Get<ParticleData<int>>();
            ref var data = ref entity.Get<BallLightningData>();

            if (++data.FrameCounter >= FRAME_DLT)
            {
                data.FrameCounter = 0;
                data.Frame = (data.Frame + 1) % 3;
            }

            rotation.Value += 0.3f;
            //scale.Value = MathF.Sin(Main.GlobalTimeWrappedHourly) / 3 + 1;

            if (++data.Timer > MAX_TIME_LEFT)
            {
                active.Value = false;
                return;
            }

            // 示例：花式东西
            Player player = Main.player[owner.Value];
            position.Value = player.Center + new Vector2(100).RotatedBy(Main.GlobalTimeWrappedHourly * 3) + new Vector2(100).RotatedBy(Main.GlobalTimeWrappedHourly * 5);
            if(data.Timer % 30 == 0)
            {
                Projectile.NewProjectile(null, position.Value, Main.rand.NextVector2CircularEdge(10, 10), ProjectileID.MagnetSphereBall, 100, 0, owner.Value);
            }

        }
        public override void Draw(in Arch.Core.Entity entity, SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            var pos = entity.Get<ParticlePosition>().Value;
            var color = entity.Get<ParticleColor>().Value;
            var rotation = entity.Get<ParticleRotation>().Value;
            var scale = entity.Get<ParticleScale>().Value;
            var data = entity.Get<BallLightningData>();
            Rectangle rect = texture.Frame(1, FRAME_NUM, 0, data.Frame);

            spriteBatch.Draw(texture, pos - Main.screenPosition, rect, color * MathHelper.Clamp((MAX_TIME_LEFT - data.Timer) / 60f, 0, 1), rotation, rect.Size() / 2, scale, SpriteEffects.None, 0);
        }
    }
}
