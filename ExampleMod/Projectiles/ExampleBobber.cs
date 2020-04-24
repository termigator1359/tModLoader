using ExampleMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
    public class ExampleBobber : ModProjectile
    {
		private bool initialized = false;
		private int color = 0;
		public static Color fishingLineColor = new Color(255, 215, 0);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Example Bobber");
		}

        public override void SetDefaults()
        {
			//These are copied through CloneDefaults
			//projectile.width = 14;
			//projectile.height = 14;
			//projectile.aiStyle = 61;
            //projectile.bobber = true;
            //projectile.penetrate = -1;
			projectile.CloneDefaults(ProjectileID.WoodenBobber);
        }

		//What if we want to randomize the line color
		public override AI()
		{
			if (!initialized)
			{
				color = Main.rand.Next(2);
				initialized = true;
			}
			//50% chance for the line to be blue instead of the default gold
			if (color == 1)
			{
				fishingLineColor = new Color(0, 191, 255);
			}
		}

        public override bool PreDrawExtras(SpriteBatch spriteBatch)
        {
			//Create some bluish light, this could also be in the AI function
			Lighting.AddLight(projectile.Center, 0f, 0.45f, 0.46f);

			//Change these two values in order to change the origin of where the line is being drawn
			int xPositionAdditive = 45;
			float yPositionAdditive = 35f;

            Player player = Main.player[projectile.owner];
            if (projectile.bobber && player.inventory[player.selectedItem].holdStyle > 0)
            {
                float pPosX = player.MountedCenter.X;
                float pPosY = player.MountedCenter.Y;
                pPosY += player.gfxOffY;
                int type = player.inventory[player.selectedItem].type;
                float gravDir = player.gravDir;

                if (type == ItemType<ExampleFishingRod>())
                {
                    pPosX += (float)(xPositionAdditive * player.direction);
                    if (player.direction < 0)
                    {
                        pPosX -= 13f;
                    }
                    pPosY -= yPositionAdditive * gravDir;
                }

                if (gravDir == -1f)
                {
                    pPosY -= 12f;
                }
                Vector2 mountedCenter = new Vector2(pPosX, pPosY);
                mountedCenter = player.RotatedRelativePoint(mountedCenter + new Vector2(8f), true) - new Vector2(8f);
                float projPosX = projectile.position.X + (float)projectile.width * 0.5f - mountedCenter.X;
                float projPosY = projectile.position.Y + (float)projectile.height * 0.5f - mountedCenter.Y;
                Math.Sqrt((double)(projPosX * projPosX + projPosY * projPosY));
                bool canDraw = true;
                if (projPosX == 0f && projPosY == 0f)
                {
                    canDraw = false;
                }
                else
                {
                    float projPosXY = (float)Math.Sqrt((double)(projPosX * projPosX + projPosY * projPosY));
                    projPosXY = 12f / projPosXY;
                    projPosX *= projPosXY;
                    projPosY *= projPosXY;
                    mountedCenter.X -= projPosX;
                    mountedCenter.Y -= projPosY;
                    projPosX = projectile.position.X + (float)projectile.width * 0.5f - mountedCenter.X;
                    projPosY = projectile.position.Y + (float)projectile.height * 0.5f - mountedCenter.Y;
                }
                while (canDraw)
                {
                    float height = 12f;
                    float positionMagnitude = (float)Math.Sqrt((double)(projPosX * projPosX + projPosY * projPosY));
                    if (float.IsNaN(positionMagnitude) || float.IsNaN(positionMagnitude))
                    {
                        canDraw = false;
                    }
                    else
                    {
                        if (positionMagnitude < 20f)
                        {
                            height = positionMagnitude - 8f;
                            canDraw = false;
                        }
                        positionMagnitude = 12f / positionMagnitude;
                        projPosX *= positionMagnitude;
                        projPosY *= positionMagnitude;
                        mountedCenter.X += projPosX;
                        mountedCenter.Y += projPosY;
                        projPosX = projectile.position.X + (float)projectile.width * 0.5f - mountedCenter.X;
                        projPosY = projectile.position.Y + (float)projectile.height * 0.1f - mountedCenter.Y;
                        if (positionMagnitude > 12f)
                        {
                            float positionInverseMultiplier = 0.3f;
                            float absVelocitySum = Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
                            if (absVelocitySum > 16f)
                            {
                                absVelocitySum = 16f;
                            }
                            absVelocitySum = 1f - absVelocitySum / 16f;
                            positionInverseMultiplier *= absVelocitySum;
                            absVelocitySum = positionMagnitude / 80f;
                            if (absVelocitySum > 1f)
                            {
                                absVelocitySum = 1f;
                            }
                            positionInverseMultiplier *= absVelocitySum;
                            if (positionInverseMultiplier < 0f)
                            {
                                positionInverseMultiplier = 0f;
                            }
                            absVelocitySum = 1f - projectile.localAI[0] / 100f;
                            positionInverseMultiplier *= absVelocitySum;
                            if (projPosY > 0f)
                            {
                                projPosY *= 1f + positionInverseMultiplier;
                                projPosX *= 1f - positionInverseMultiplier;
                            }
                            else
                            {
                                absVelocitySum = Math.Abs(projectile.velocity.X) / 3f;
                                if (absVelocitySum > 1f)
                                {
                                    absVelocitySum = 1f;
                                }
                                absVelocitySum -= 0.5f;
                                positionInverseMultiplier *= absVelocitySum;
                                if (positionInverseMultiplier > 0f)
                                {
                                    positionInverseMultiplier *= 2f;
                                }
                                projPosY *= 1f + positionInverseMultiplier;
                                projPosX *= 1f - positionInverseMultiplier;
                            }
                        }
                        float rotation2 = (float)Math.Atan2((double)projPosY, (double)projPosX) - MathHelper.PiOver2;
						//This color decides the color of the fishing line. It defaults to gold but has a 50% chance to be blue as decided in the AI.
                        Color lineColor = Lighting.GetColor((int)mountedCenter.X / 16, (int)(mountedCenter.Y / 16f), fishingLineColor);

                        Main.spriteBatch.Draw(Main.fishingLineTexture, new Vector2(mountedCenter.X - Main.screenPosition.X + (float)Main.fishingLineTexture.Width * 0.5f, mountedCenter.Y - Main.screenPosition.Y + (float)Main.fishingLineTexture.Height * 0.5f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, Main.fishingLineTexture.Width, (int)height)), lineColor, rotation2, new Vector2((float)Main.fishingLineTexture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
                    }
                }
            }
            return false;
		}
    }
}
