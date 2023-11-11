using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BEPUphysics;

using Core.FPS;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Entities;
using System.Threading;
using System.Diagnostics;
using Core.Helpers;
using BEPUphysics.BroadPhaseEntries;
using Core.Utils;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Core
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Thread PhysicsThread;

        FPSController controller;

        Space space = new Space();

        Model PlaygroundModel;
        Model MapModel;

        Model CubeModel;

        Texture2D reticleTexture;

        List<SoundEffect> effects = new List<SoundEffect>();

        //Crate[] crates;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Mouse.SetCursor(MouseCursor.FromTexture2D(Content.Load<Texture2D>("transparent-01"), 0, 0));
            PlaygroundModel = Content.Load<Model>("ground");
            CubeModel = Content.Load<Model>("cube");

            MapModel = Content.Load<Model>("first map");

            controller = new FPSController(this, new Vector3(0, 65, -360), new Vector3(-0.1f, 179.06393f, 0), space);
            CreateStaticWorld();

            effects.Add(Content.Load<SoundEffect>("612663__nox_sound__snowboard_slide_loop_mono_03"));
            effects.Add(Content.Load<SoundEffect>("444640__audiopapkin__swoosh-27"));
            effects.Add(Content.Load<SoundEffect>("disco-club-by-winniethemoog-from-filmmusic-io"));
            effects.Add(Content.Load<SoundEffect>("244942__spoonbender__wind-through-trees-3b"));

            var x = effects[2].CreateInstance();
            x.IsLooped = true;
            x.Volume = 0.125f;
            x.Play();

            var y = effects[3].CreateInstance();
            y.IsLooped = true;
            y.Volume = 0.125f;
            y.Play();

            var snowboard = effects[0].CreateInstance();
            controller.Snowboard = snowboard;
            controller.Woosh = effects[1].CreateInstance();

            #region debug

            //crates = new Crate[] {
            //new Crate(this, CrateType.DarkCross, Matrix.Identity),
            //new Crate(this, CrateType.Slats, Matrix.CreateTranslation(4, 0, 5)),
            //new Crate(this, CrateType.Cross, Matrix.CreateTranslation(-8, 0, 3)),
            //new Crate(this, CrateType.DarkCross, Matrix.CreateRotationY(MathHelper.PiOver4) * Matrix.CreateTranslation(1, 0, 7)),
            //new Crate(this, CrateType.Slats, Matrix.CreateTranslation(3, 0, -3)),
            //new Crate(this, CrateType.Cross, Matrix.CreateRotationY(3) * Matrix.CreateTranslation(3, 2, -3))
            //};


            #endregion


            //Box ground = new Box(BEPUutilities.Vector3.Zero, 30, 1, 30);
            //space.Add(ground);

            space.Add(new Box(new BEPUutilities.Vector3(0, 4, 0), 1, 1, 1, 1));
            space.Add(new Box(new BEPUutilities.Vector3(0, 8, 0), 1, 1, 1, 1));
            space.Add(new Box(new BEPUutilities.Vector3(0, 12, 0), 1, 1, 1, 1));

            foreach (Entity e in space.Entities)
            {
                if (e is Box box)
                {
                    BEPUutilities.Matrix scaling = BEPUutilities.Matrix.CreateScale(box.Width, box.Height, box.Length);
                    EntityModel model = new EntityModel(e, CubeModel, controller, scaling, this);
                    Components.Add(model);
                    e.Tag = model;
                }
            }



            reticleTexture = Content.Load<Texture2D>("reticle-01");
            Components.Add(new Reticle(this, reticleTexture));


            SetupPhysics();
            controller.SetEnabled(true);
        }

        private void CreateStaticWorld()
        {
            //BEPUutilities.Vector3[] vertices;
            //int[] indices;
            //MeshHandler.GetVerticesAndIndicesFromModel(PlaygroundModel, out vertices, out indices);
            //var mesh = new StaticMesh(vertices, indices, new BEPUutilities.AffineTransform(new BEPUutilities.Vector3(0, -40, 0)));
            //space.Add(mesh);
            //Components.Add(new StaticModel(PlaygroundModel, mesh.WorldTransform.Matrix, controller, this));

            BEPUutilities.Vector3[] vertices1;
            int[] indices1;
            MeshHandler.GetVerticesAndIndicesFromModel(MapModel, out vertices1, out indices1);
            var mesh1 = new StaticMesh(vertices1, indices1, new BEPUutilities.AffineTransform(new BEPUutilities.Vector3(0, -40, 0)));
            space.Add(mesh1);
            Components.Add(new StaticModel(MapModel, mesh1.WorldTransform.Matrix, controller, this));
        }


        private void SetupPhysics()
        {
            space.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -20, 0);

            PhysicsThread = new Thread(PhysicsUpdate); //monogame does not have a dedicated fixed update thread...so we have to make one
            PhysicsThread.IsBackground = true;
            PhysicsThread.Start();
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            controller.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        /// <summary>
        /// Update the physics engine (on a different thread, becuase monogame does not have a good enough fixed timestep)
        /// </summary>
        void PhysicsUpdate()
        {
            double dt;
            double time;
            double previousTime = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;

            while (true)
            {
                time = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
                dt = time - previousTime;
                previousTime = time;

                space.Update((float)dt);
                Thread.Sleep(0); // force the thread to yield
            }
        }
    }
}