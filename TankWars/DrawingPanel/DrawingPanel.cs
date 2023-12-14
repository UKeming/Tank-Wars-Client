using System.Windows.Forms;
using Model;
using GameController;
using System;
using System.Drawing;
using System.Collections.Generic;
using TankWars;

namespace View
{
    public class DrawingPanel : Panel
    {
        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        // Control game data.
        public Controller controller;

        // Image resources, save here temporarily.
        // Todo: move them to "Model".
        public Image powerup = Image.FromFile("../../../Resources/Images/Powerup.png");
        public Image tank = Image.FromFile("../../../Resources/Images/BlueTank.png");
        public Image turret = Image.FromFile("../../../Resources/Images/BlueTurret.png");
        public Image shot = Image.FromFile("../../../Resources/Images/shot_blue.png");
        public Image background = Image.FromFile("../../../Resources/Images/Background.png");
        public Image wall = Image.FromFile("../../../Resources/Images/WallSprite.png");

        /// <summary>
        /// Construct the Panel to draw world.
        /// </summary>
        /// <param name="world"></param>
        public DrawingPanel()
        {
            DoubleBuffered = true;
            controller = new Controller();
            // Register events.
            controller.UpdateArrived += OnFrame;
        }

        /// <summary>
        /// Refresh the Panel.
        /// </summary>
        private void OnFrame()
        {
            this.Invoke(new MethodInvoker(() => Invalidate(true)));
        }

        /// <summary>
        /// Draw beam.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="org"></param>
        /// <param name="dir"></param>
        private void OnBeamAttack(int owner, Vector2D org, Vector2D dir)
        {
            // Todo: play Gif animation of beam.
            Bitmap animatedImage = new Bitmap("../../../Resources/Images/laser.gif");
        }

        /// <summary>
        ///     This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">
        ///     The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever
        ///     it wants
        /// </param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle,
            ObjectDrawer drawer)
        {
            // "push" the current transform
            var oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }
        // Redraw example.
        protected override void OnPaint(PaintEventArgs e)
        {
            if (controller.gameHasStarted())
            {
                // Get player's position and transform the view to the center of player's position.
                int viewSize = Size.Width;
                double playerX = controller.GetPlayerByID(controller.getPlayerID()).GetLocation().GetX();
                double playerY = controller.GetPlayerByID(controller.getPlayerID()).GetLocation().GetY();
                e.Graphics.TranslateTransform((float)(-playerX + (viewSize / 2)), (float)(-playerY + (viewSize / 2)));
                World world = controller.getWorld();


                // Draw the background
                e.Graphics.DrawImage(background, -world.size / 2, -world.size / 2, world.size, world.size);

                // Draw Powerups
                lock (Controller.powerupLock)
                {
                    foreach (KeyValuePair<int, Powerup> p in world.powerups)
                    {
                        Powerup powerup = p.Value;
                        DrawObjectWithTransform(e, powerup, powerup.GetLocation().GetX(), powerup.GetLocation().GetY(), 0, PowerupDrawer);
                    }

                }

                // Draw projectile.
                lock (Controller.projectileLock)
                {
                    foreach (KeyValuePair<int, Projectile> s in world.projectiles)
                    {
                        Projectile shot = s.Value;
                        DrawObjectWithTransform(e, shot, shot.GetLocation().GetX(), shot.GetLocation().GetY(), shot.GetOrientation().ToAngle(), ShotDrawer);
                    }

                }







                // Draw walls
                foreach (KeyValuePair<int, Wall> w in world.walls)
                {
                    Wall wall = w.Value;
                    if ((wall.getStartPoint() - wall.getEndPoint()).GetX() == 0)
                    {
                        if (wall.getStartPoint().GetY() < wall.getEndPoint().GetY())
                        {
                            for (double pixel = wall.getStartPoint().GetY(); pixel <= wall.getEndPoint().GetY(); pixel += 50)
                            {
                                DrawObjectWithTransform(e, wall, wall.getStartPoint().GetX(), pixel, 0, WallDrawer);
                            }
                        }
                        else
                        {
                            for (double pixel = wall.getEndPoint().GetY(); pixel <= wall.getStartPoint().GetY(); pixel += 50)
                            {
                                DrawObjectWithTransform(e, wall, wall.getStartPoint().GetX(), pixel, 0, WallDrawer);
                            }
                        }

                    }
                    else
                    {
                        if (wall.getStartPoint().GetX() < wall.getEndPoint().GetX())
                        {
                            for (double pixel = wall.getStartPoint().GetX(); pixel <= wall.getEndPoint().GetX(); pixel += 50)
                            {
                                DrawObjectWithTransform(e, wall, pixel, wall.getStartPoint().GetY(), 0, WallDrawer);
                            }
                        }
                        else
                        {
                            for (double pixel = wall.getEndPoint().GetX(); pixel <= wall.getStartPoint().GetX(); pixel += 50)
                            {
                                DrawObjectWithTransform(e, wall, pixel, wall.getStartPoint().GetY(), 0, WallDrawer);
                            }

                        }
                    }
                }

                // Draw beams;
                lock (Controller.beamLock)
                {
                    foreach (KeyValuePair<int, Beam> b in world.beams)
                    {
                        Beam beam = b.Value;
                        DrawObjectWithTransform(e, beam, beam.GetOrigin().GetX(), beam.GetOrigin().GetY(), beam.GetOrientation().ToAngle(), BeamDrawer);
                    }
                    // Remove completed beams.
                    foreach (int id in Beam.CompletedBeams)
                        world.beams.Remove(id);
                    Beam.CompletedBeams = new List<int>();
                }

                // Draw Tanks
                lock (Controller.tankLock)
                {

                    foreach (KeyValuePair<int, Tank> t in world.tanks)
                    {
                        Tank tank = t.Value;
                        if (tank.GetHP() > 0)
                        {
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(), tank.GetOrientation().ToAngle(), TankDrawer);
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(), tank.GetAiming().ToAngle(), TurretDrawer);

                            // Draw the life bar
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(), 0, ProfileDrawer);
                        }
                        if (tank.IsDead())
                            world.explosions[tank.GetID()] = new Explosion(tank.GetID());
                    }
                }
                // Draw explosions.
                foreach (KeyValuePair<int, Explosion> ex in world.explosions)
                {
                    Explosion explosion = ex.Value;
                    DrawObjectWithTransform(e, explosion, world.tanks[explosion.GetID()].GetLocation().GetX(), world.tanks[explosion.GetID()].GetLocation().GetY(), 0, ExplosionDrawer);
                }
                // Remove completed explosion.
                foreach (int ID in Explosion.CompletedExplosions)
                    world.explosions.Remove(ID);

                Explosion.CompletedExplosions = new List<int>();
                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);

            }
        }

        /// <summary>
        /// Draw shot.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ShotDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(shot, -15, -15, 30, 30);
        }

        /// <summary>
        /// Draw powerup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(powerup, -15, -15, 30, 30);
        }

        /// <summary>
        /// Draw turret.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(turret, -30, -30, 60, 60);
        }

        /// <summary>
        /// Draw tank.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(tank, -30, -30, 60, 60);
        }

        /// <summary>
        /// Draw wall.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(wall, -25, -25, 50, 50);
        }

        /// <summary>
        /// Draw life bar, score, id, name of a tank.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProfileDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            // Draw life bar.
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Black), new Rectangle(-30, 40, 60, 5));
            e.Graphics.FillRectangle(tank.GetHP() > 2 ? new System.Drawing.SolidBrush(System.Drawing.Color.Green) : tank.GetHP() > 1 ? new System.Drawing.SolidBrush(System.Drawing.Color.Yellow) : new System.Drawing.SolidBrush(System.Drawing.Color.Red), new Rectangle(-30, 40, (tank.GetHP()) * 20, 5));
            // Draw details.
            e.Graphics.DrawString(tank.GetID() + " " + tank.GetName() + ": " + tank.GetScore(), new Font(new FontFamily("Microsoft YaHei"), 12), new System.Drawing.SolidBrush(System.Drawing.Color.Black), -40, 40);
        }

        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam beam = o as Beam;
            float startX = (float)beam.GetStartPosition().GetX();
            float startY = (float)beam.GetStartPosition().GetY();
            float endX = (float)beam.GetEndPosition().GetX();
            float endY = (float)beam.GetEndPosition().GetY();
            Random random = new Random();
            e.Graphics.DrawLine(new System.Drawing.Pen(Color.FromArgb(182, 220, 255)), startX, startY, endX, endY);
            for (int i = 0; i < 100; i++)
                e.Graphics.FillEllipse(new System.Drawing.SolidBrush(Color.FromArgb(182, 220, 255)), random.Next((int)startX - beam.currentFrame, (int)endX + beam.currentFrame), random.Next((int)endY, (int)startY), random.Next(3, 5), random.Next(3, 5));
            if (beam.nextFrame() == -1)
                Beam.CompletedBeams.Add(beam.GetID());
        }
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            Explosion explosion = o as Explosion;
            Random random = new Random();

            System.Drawing.Drawing2D.Matrix currentMatrix = e.Graphics.Transform;
            Point[] points = new Point[] { new Point(random.Next(-8, 8), random.Next(-8, 8)), new Point(random.Next(-8, 8), random.Next(-8, 8)), new Point(random.Next(-8, 8), random.Next(-8, 8)) };
            for (int i = 0; i < 10; i++)
            {
                e.Graphics.TranslateTransform((float)(random.Next(-explosion.currentFrame*2, explosion.currentFrame*2)), (float)(random.Next(-explosion.currentFrame*2, explosion.currentFrame*2)));
                e.Graphics.FillPolygon(new System.Drawing.SolidBrush(System.Drawing.Color.White), points);
                e.Graphics.Transform = currentMatrix;
            }

            if (explosion.nextFrame() == -1)
                Explosion.CompletedExplosions.Add(explosion.GetID());

        }
    }
}