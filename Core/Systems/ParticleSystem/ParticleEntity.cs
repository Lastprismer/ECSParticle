using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Entity = Arch.Core.Entity;

namespace ECSParticle.Core.Systems.ParticleSystem
{
    /// <summary>
    /// 粒子的基类，简单来说可以当作获取参数更复杂的纯视效弹幕来写，具体信息可以查看更多注释
    /// </summary>
    public abstract class ParticleEntity : ModTexturedType
    {
        /// <summary>
        /// 贴图路径，默认为<code>ECSParticle/Assets/Particles/TypeName</code>
        /// <list type="bullet">
        /// <item>粒子没有统一的TextureAssets，所以不会被自动调用</item>
        /// <item>需要在绘制时通过 <see cref="ModContent.Request{T}(string, ReLogic.Content.AssetRequestMode)"/> 来获取贴图</item>
        ///     <item>其他的想到再写</item>
        /// </list>
        /// </summary>
        public override string Texture => Assets.Assets.ParticlePath + Name;

        /// <summary>
        /// 类似于弹幕AI，逐帧更新
        /// <list type="bullet">
        /// 执行完后会调用一次 <c>position += velocity</c> 来自动更新速度。
        ///     <list type="bullet">
        ///     <item>没有类似 <see cref="ModProjectile.ShouldUpdatePosition"/> 的重写函数，可通过在函数的 <b>最后</b> 补充 <c>position -= velocity</c> 来模拟行为</item>
        ///     <item>没有类似 <see cref="Projectile.extraUpdates"/> 的重写参数，可通过循环执行更新代码，并在最后特判位置更新来模拟行为</item>
        ///     </list>
        /// <item>执行位置为 <b>所有更新的开始处</b> ，与原版的ParticleSystem相同</item>
        ///     <list type="bullet">
        ///     <item>注意获取某些实体（如玩家、弹幕等）的信息时并不同步（读取时未更新，仍未前一帧状态）</item>
        ///     </list>
        ///     <item>其他的想到再写</item>
        /// </list>
        /// </summary>
        /// <param name="entity">粒子信息实例</param>
        public virtual void Update(in Entity entity) { }

        /// <summary>
        /// 用于绘制粒子
        /// <list type="bullet">
        /// <item>仅当粒子在以屏幕中心为中心，长、宽为两倍显示器长、宽的矩形中时才绘制</item>
        /// <item>没有传入<c>lightColor</c>，有需求可以自行调用 <see cref="Lighting.GetColor(int, int)"/> </item>
        /// <item>绘制层取决于 <see cref="ParticleDrawBehindEntities"/> 是否是实体的一个组件</item>
        /// <item>默认 <c>spriteBatch.Begin()</c> 已调用，且混合模式为 <c>AlphaBlend</c>，可自行调整以更改绘制信息 </item>
        /// <item> <i>TODO：加速 <c>Additive</c>混合模式的绘制</i> </item>
        /// </list>
        /// </summary>
        /// <param name="entity">粒子信息实例</param>
        /// <param name="spriteBatch"><c>spriteBatch</c></param>
        public virtual void Draw(in Entity entity, SpriteBatch spriteBatch) { }

        /// <summary>
        /// 用于生成粒子时更改更多信息
        /// <list type="bullet">
        /// <item>在 <see cref="ParticleSystem.NewParticle(ParticleEntity, Vector2, Vector2, Color, float, float)"/> 的最后调用</item>
        ///     <list type="bullet">
        ///     <item>在函数内获取的粒子相关信息是在 <c>NewParticle</c> <i><b>赋值之后</b></i> 的，这点与 <see cref="Projectile.NewProjectile(Terraria.DataStructures.IEntitySource, float, float, float, float, int, int, float, int, float, float, float)"/> 不同</item>
        ///     </list>
        /// <item>可用于从粒子的角度增加更多数据</item>
        /// </list>
        /// </summary>
        /// <param name="entity">粒子信息实例</param>
        public virtual void OnSpawn(in Entity entity) { }

        protected sealed override void Register()
        {
            ModTypeLookup<ParticleEntity>.Register(this);
        }

        public sealed override void SetupContent()
        {
            SetStaticDefaults();
        }
    }
}
