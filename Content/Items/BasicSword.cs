using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core.Extensions;
using ECSParticle.Content.Particles;
using ECSParticle.Core.Systems.ParticleSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ECSParticle.Content.Items
{
    internal class BasicSword : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 500;
            Item.DamageType = DamageClass.Melee;
            Item.rare = ItemRarityID.White;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.shoot = ProjectileID.StarCannonStar;
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for(int i = 0; i < 1; i++)
            {
                Arch.Core.Entity entity = ParticleSystem.NewParticle(ModContent.GetInstance<BallLightning>(), player.Center, Vector2.Zero, Color.White);
                entity.Add(new ParticleDrawBehindEntities { });
                entity.Add(new ParticleData<int> { Value = player.whoAmI });

            }


            return false;
        }
    }
}
