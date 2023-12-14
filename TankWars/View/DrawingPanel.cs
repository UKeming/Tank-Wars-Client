// @File: DrawingPanel.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/07
// @Author: Keming Chen, Yifei Sun

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GameController;
using Model;
using TankWars;

namespace View
{
    public class DrawingPanel : Panel
    {
        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        public Image background = Image.FromFile("../../../Resources/Images/Background.png");

        // Control game data.
        public Controller controller;

        // Image resources.
        public Image powerup = Image.FromFile("../../../Resources/Images/Powerup.png");
        public Image[] Shots;
        public Image[] Tanks;
        public Image[] Turrets;
        public Image wall = Image.FromFile("../../../Resources/Images/WallSprite.png");

        /// <summary>
        ///     Construct the Panel to draw world.
        /// </summary>
        /// <param name="world"></param>
        public DrawingPanel()
        {
            DoubleBuffered = true;
            controller = new Controller();
            // Register events.
            controller.UpdateArrived += OnFrame;

            // Load assets.
            Tanks = new Image[8]
            {
                Image.FromFile("../../../Resources/Images/BlueTank.png"),
                Image.FromFile("../../../Resources/Images/RedTank.png"),
                Image.FromFile("../../../Resources/Images/OrangeTank.png"),
                Image.FromFile("../../../Resources/Images/DarkTank.png"),
                Image.FromFile("../../../Resources/Images/GreenTank.png"),
                Image.FromFile("../../../Resources/Images/LightGreenTank.png"),
                Image.FromFile("../../../Resources/Images/PurpleTank.png"),
                Image.FromFile("../../../Resources/Images/YellowTank.png")
            };
            Turrets = new Image[8]
            {
                Image.FromFile("../../../Resources/Images/BlueTurret.png"),
                Image.FromFile("../../../Resources/Images/RedTurret.png"),
                Image.FromFile("../../../Resources/Images/OrangeTurret.png"),
                Image.FromFile("../../../Resources/Images/DarkTurret.png"),
                Image.FromFile("../../../Resources/Images/GreenTurret.png"),
                Image.FromFile("../../../Resources/Images/LightGreenTurret.png"),
                Image.FromFile("../../../Resources/Images/PurpleTurret.png"),
                Image.FromFile("../../../Resources/Images/YellowTurret.png")
            };
            Shots = new Image[8]
            {
                Image.FromFile("../../../Resources/Images/shot-blue.png"),
                Image.FromFile("../../../Resources/Images/shot-red.png"),
                Image.FromFile("../../../Resources/Images/shot-yellow.png"),
                Image.FromFile("../../../Resources/Images/shot-grey.png"),
                Image.FromFile("../../../Resources/Images/shot-green.png"),
                Image.FromFile("../../../Resources/Images/shot-brown.png"),
                Image.FromFile("../../../Resources/Images/shot-violet.png"),
                Image.FromFile("../../../Resources/Images/shot-white.png")
            };
        }

        /// <summary>
        ///     Refresh the Panel.
        /// </summary>
        private void OnFrame()
        {
            Invoke(new MethodInvoker(() => Invalidate(true)));
        }

        /// <summary>
        ///     Draw beam.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="org"></param>
        /// <param name="dir"></param>
        private void OnBeamAttack(int owner, Vector2D org, Vector2D dir)
        {
            var animatedImage = new Bitmap("../../../Resources/Images/laser.gif");
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
                var viewSize = Size.Width;
                var playerX = controller.GetPlayerByID(controller.getPlayerID()).GetLocation().GetX();
                var playerY = controller.GetPlayerByID(controller.getPlayerID()).GetLocation().GetY();
                e.Graphics.TranslateTransform((float)(-playerX + viewSize / 2), (float)(-playerY + viewSize / 2));
                var world = controller.getWorld();


                // Draw the background
                e.Graphics.DrawImage(background, -world.size / 2, -world.size / 2, world.size, world.size);

                // Draw Powerups
                lock (Controller.powerupLock)
                {
                    foreach (var powerup in world.powerups.Values)
                    {
                        DrawObjectWithTransform(e, powerup, powerup.GetLocation().GetX(), powerup.GetLocation().GetY(),
                            0, PowerupDrawer);
                    }
                }

                // Draw projectile.
                lock (Controller.projectileLock)
                {
                    foreach (var shot in world.projectiles.Values)
                    {
                        DrawObjectWithTransform(e, shot, shot.GetLocation().GetX(), shot.GetLocation().GetY(),
                            shot.GetOrientation().ToAngle(), ShotDrawer);
                    }
                }


                // Draw walls
                foreach (var wall in world.walls.Values)
                {
                    if ((wall.getStartPoint() - wall.getEndPoint()).GetX() == 0)
                    {
                        if (wall.getStartPoint().GetY() < wall.getEndPoint().GetY())
                            for (var pixel = wall.getStartPoint().GetY();
                                pixel <= wall.getEndPoint().GetY();
                                pixel += 50)
                                DrawObjectWithTransform(e, wall, wall.getStartPoint().GetX(), pixel, 0, WallDrawer);
                        else
                            for (var pixel = wall.getEndPoint().GetY();
                                pixel <= wall.getStartPoint().GetY();
                                pixel += 50)
                                DrawObjectWithTransform(e, wall, wall.getStartPoint().GetX(), pixel, 0, WallDrawer);
                    }
                    else
                    {
                        if (wall.getStartPoint().GetX() < wall.getEndPoint().GetX())
                            for (var pixel = wall.getStartPoint().GetX();
                                pixel <= wall.getEndPoint().GetX();
                                pixel += 50)
                                DrawObjectWithTransform(e, wall, pixel, wall.getStartPoint().GetY(), 0, WallDrawer);
                        else
                            for (var pixel = wall.getEndPoint().GetX();
                                pixel <= wall.getStartPoint().GetX();
                                pixel += 50)
                                DrawObjectWithTransform(e, wall, pixel, wall.getStartPoint().GetY(), 0, WallDrawer);
                    }
                }

                // Draw beams;
                lock (Controller.beamLock)
                {
                    foreach (var beam in world.beams.Values)
                    {
                        DrawObjectWithTransform(e, beam, beam.GetOrigin().GetX(), beam.GetOrigin().GetY(),
                            beam.GetOrientation().ToAngle(), BeamDrawer);
                    }

                    // Remove completed beams.
                    foreach (var id in Beam.CompletedBeams)
                        world.beams.Remove(id);
                    Beam.CompletedBeams = new List<int>();
                }

                // Draw Tanks
                lock (Controller.tankLock)
                {
                    foreach (var tank in world.tanks.Values)
                    {
                        if (tank.GetHP() > 0)
                        {
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(),
                                tank.GetOrientation().ToAngle(), TankDrawer);
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(),
                                tank.GetAiming().ToAngle(), TurretDrawer);

                            // Draw the life bar
                            DrawObjectWithTransform(e, tank, tank.GetLocation().GetX(), tank.GetLocation().GetY(), 0,
                                ProfileDrawer);
                        }

                        if (tank.IsDead())
                            world.explosions[tank.GetID()] = new Explosion(tank.GetID());
                    }
                }

                // Draw explosions.
                foreach (var explosion in world.explosions.Values)
                {
                    DrawObjectWithTransform(e, explosion, world.tanks[explosion.GetID()].GetLocation().GetX(),
                        world.tanks[explosion.GetID()].GetLocation().GetY(), 0, ExplosionDrawer);
                }

                // Remove completed explosion.
                foreach (var ID in Explosion.CompletedExplosions)
                    world.explosions.Remove(ID);

                Explosion.CompletedExplosions = new List<int>();
                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);
            }
        }

        /// <summary>
        ///     Draw shot.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ShotDrawer(object o, PaintEventArgs e)
        {
            var projectile = o as Projectile;
            var shotImage = Shots[projectile.GetOwner() % Shots.Length];
            e.Graphics.DrawImage(shotImage, -15, -15, 30, 30);
        }

        /// <summary>
        ///     Draw powerup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(powerup, -15, -15, 30, 30);
        }

        /// <summary>
        ///     Draw turret.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            var tank = o as Tank;
            var turretImage = Turrets[tank.GetID() % Turrets.Length];
            e.Graphics.DrawImage(turretImage, -30, -30, 60, 60);
        }

        /// <summary>
        ///     Draw tank.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            var tank = o as Tank;
            var tankImage = Tanks[tank.GetID() % Tanks.Length];
            e.Graphics.DrawImage(tankImage, -30, -30, 60, 60);
        }

        /// <summary>
        ///     Draw wall.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(wall, -25, -25, 50, 50);
        }

        /// <summary>
        ///     Draw life bar, score, id, name of a tank.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProfileDrawer(object o, PaintEventArgs e)
        {
            var tank = o as Tank;
            // Draw life bar.
            e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(-30, 35, 60, 5));
            e.Graphics.FillRectangle(
                tank.GetHP() > 2 ? new SolidBrush(Color.Green) :
                tank.GetHP() > 1 ? new SolidBrush(Color.Yellow) : new SolidBrush(Color.Red),
                new Rectangle(-30, 35, tank.GetHP() * 20, 5));
            // Draw details.
            e.Graphics.DrawString(tank.GetName() + ": " + tank.GetScore(),
                new Font(new FontFamily("Microsoft YaHei"), 12), new SolidBrush(Color.Black), -35, 40);
        }

        private void BeamDrawer(object o, PaintEventArgs e)
        {
            var beam = o as Beam;
            var startX = (float)beam.GetStartPosition().GetX();
            var startY = (float)beam.GetStartPosition().GetY();
            var endX = (float)beam.GetEndPosition().GetX();
            var endY = (float)beam.GetEndPosition().GetY();
            var random = new Random();
            var progress = beam.GetProgress();
            var color = Color.FromArgb(182 + (int)(73 * progress), 220 + (int)(35 * progress), 255);
            e.Graphics.DrawLine(new Pen(color), startX, startY, endX, endY);
            for (var i = 0; i < 100; i++)
                e.Graphics.FillEllipse(new SolidBrush(color),
                    random.Next((int)startX - beam.currentFrame, (int)endX + beam.currentFrame),
                    random.Next((int)endY, (int)startY), random.Next(3, 5), random.Next(3, 5));
            if (beam.nextFrame() == -1)
                Beam.CompletedBeams.Add(beam.GetID());
        }

        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            var explosion = o as Explosion;
            var random = new Random();
            var progess = explosion.GetProgress();
            var currentMatrix = e.Graphics.Transform;
            var points = new Point[3];
            for (var i = 0; i < 3; i++)
                points[i] = new Point(random.Next((int)(-10 * (1 - progess)), (int)(10 * (1 - progess))),
                    random.Next((int)(-10 * (1 - progess)), 10));
            for (var i = 0; i < 10; i++)
            {
                e.Graphics.TranslateTransform(random.Next(-explosion.currentFrame * 2, explosion.currentFrame * 2),
                    random.Next(-explosion.currentFrame * 2, explosion.currentFrame * 2));
                e.Graphics.FillPolygon(
                    new SolidBrush(Color.FromArgb(255 - (int)(255 * progess), 67 - (int)(67 * progess), 0)), points);
                e.Graphics.Transform = currentMatrix;
            }

            if (explosion.nextFrame() == -1)
                Explosion.CompletedExplosions.Add(explosion.GetID());
        }
    }
}