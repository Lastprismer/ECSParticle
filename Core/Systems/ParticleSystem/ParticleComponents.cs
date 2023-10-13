using Microsoft.Xna.Framework;
using Arch.Core;

namespace ECSParticle.Core.Systems.ParticleSystem;

/// <summary>
/// 粒子实体，包括更新、绘制等函数
/// </summary>
public struct Particle
{
    public ParticleEntity Value { get; set; }
}

/// <summary>
/// 粒子存活状态，不会自动设定，会自动检查。每帧更新结束后会将值为 <c>false</c> 的粒子销毁并释放资源 <br/> <b><i>需要手动设置以保证粒子消亡！</i></b>
/// </summary>
public struct ParticleActive
{
    public bool Value { get; set; }
}


/// <summary>
/// 粒子位置，会根据速度自动更新
/// </summary>
public struct ParticlePosition
{
    public Vector2 Value { get; set; }
}

/// <summary>
/// 粒子速度，无自动更新
/// </summary>
public struct ParticleVelocity
{
    public Vector2 Value { get; set; }
}

/// <summary>
/// 粒子旋转，无自动更新，粒子生成时默认为0
/// </summary>
public struct ParticleRotation
{
    public float Value { get; set; }
}

/// <summary>
/// 粒子缩放，无自动更新，粒子生成时默认为1
/// </summary>
public struct ParticleScale
{
    public float Value { get; set; }
}

/// <summary>
/// 粒子缩放，无自动更新
/// </summary>
public struct ParticleColor
{
    public Color Value { get; set; }
}

/// <summary>
/// 用于存储额外的数据【比如为与其他同类粒子相分辨而额外打上的tag，或是直接接收粒子系统外的信息（<see cref="Terraria.Entity.whoAmI"/> 等）】
/// <list type="bullet">
/// <item>应作用于 <see cref="ParticleSystem.NewParticle(ParticleEntity, Vector2, Vector2, Color, float, float)"/> 返回的粒子实体</item>
/// <item>添加直接面向粒子的复杂数据时，建议在粒子实体类同文件中自行设计结构体并在 <see cref="ParticleEntity.OnSpawn(in Entity)"/> 中进行组件添加</item>
/// <item>两种方法本无区别，只是这么写逻辑清晰一些且便于传参</item>
/// </list>
/// </summary>
/// <typeparam name="T">额外数据的类型</typeparam>
public struct ParticleData<T>
{
    public T Value { get; set; }
}

/// <summary>
/// 标识粒子是否会绘制在NPC层背后。默认情况下粒子仅绘制在原版Dust层背后，在其他所有实体之前
/// <list type="bullet">
/// <item>添加位置与效果可参考 <see cref="ParticleData{T}"/> 相关注释</item>
/// </list>
/// </summary>
public struct ParticleDrawBehindEntities { }
