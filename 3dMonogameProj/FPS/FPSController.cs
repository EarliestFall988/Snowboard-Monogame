using BEPUphysics;
using BEPUphysics.Character;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using System;
using System.Diagnostics;

namespace Core.FPS
{
    public class FPSController : ICamera
    {
        public Vector3 CameraRotation { get; set; }

        CharacterController cc;

        private Vector3 _positionStore = Vector3.Zero;

        private bool jumpPressed = false;

        public float slideSpeed = 20;

        public Vector3 Position
        {
            get => _positionStore;
            set
            {
                _positionStore = value + new Vector3(lastIterationValueX, lastIterationValueY, lastIterationValueZ);
            }
        }

        private Vector3 newPosition = Vector3.Zero;


        private Vector3 BodyOffset = new Vector3(0, 2, 0);

        float moveX, moveY, moveZ = 0;

        public Vector3 TopSpeed { get; set; } = new Vector3(7, 7, 7);
        public Vector3 RunSpeed { get; set; } = new Vector3(12, 12, 12);

        MouseState oldMouseState;

        private Microsoft.Xna.Framework.Game _game;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public float Acceleration { get; set; } = 20f;

        public Matrix View { get; protected set; }

        public Matrix Projection { get; protected set; }

        public float Sensitivity { get; set; } = 0.0018f;

        private Vector3 Direction => Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(CameraRotation.X) * Matrix.CreateRotationY(CameraRotation.Y));

        private BEPUutilities.Matrix WorldMatrix => BEPUutilities.Matrix.CreateWorldRH(ConvertTo(_positionStore), ConvertTo(Direction), ConvertTo(Vector3.Up));

        //public bool Enabled { get; set; }

        //public int UpdateOrder => 1;

        private OpenSimplexNoise noise = new OpenSimplexNoise();

        public bool HandheldCameraShakeEnabled { get; set; } = false;
        public float HandheldCameraShakeFrequency { get; set; } = 1;
        public float HandheldCameraShakeAmount { get; set; } = 1;

        private double iterator = 0;
        float lastIterationValueX = 0;
        float lastIterationValueY = 0;
        float lastIterationValueZ = 0;
        float lastRotationValue = 0;

        public bool enabled { get; set; } = true;
        private Space Physics;

        public Vector3 cameraOffset;

        float X = 0;
        float Y = 0;

        private bool snowboardPlaying = false;
        public SoundEffectInstance Woosh;
        public SoundEffectInstance Snowboard;

        private bool wasNotSliding = true;

        public FPSController(Microsoft.Xna.Framework.Game game, Vector3 position, Vector3 cameraRotation, Space physicsSpace)
        {
            this._game = game;
            this.Position = position;
            CameraRotation = cameraRotation;
            X = cameraRotation.X;
            Y = cameraRotation.Y;

            cc = new CharacterController();
            cc.Body.Position = new BEPUutilities.Vector3(position.X, position.Y, position.Z);

            Physics = physicsSpace;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, game.GraphicsDevice.Viewport.AspectRatio, 0.05f, 1000f);
            Mouse.SetPosition(0, game.Window.ClientBounds.Height);
        }

        public void SetEnabled(bool isEnabled)
        {

            enabled = isEnabled;
            if (enabled)
            {
                if (Physics != null)
                {
                    Physics.Add(cc);
                    _positionStore = new Vector3(cc.Body.Position.X, cc.Body.Position.Y, cc.Body.Position.Z) + BodyOffset;
                }
            }
            else
            {
                if (Physics != null)
                    Physics.Remove(cc);
            }
        }

        public void Update(GameTime gameTime)
        {
            var newMouseState = Mouse.GetState();

            var vector = HandleTranslation(gameTime);
            CameraShake(gameTime);
            var playerRotation = HandlePlayerRotation(newMouseState);


            cc.HorizontalMotionConstraint.MovementDirection = new BEPUutilities.Vector2(vector.X, vector.Y);

            if (cc.Body.Position.Y < -100)
            {
                cc.Body.Position = new BEPUutilities.Vector3(0, 65, -360);
            }

            if (!Snowboard.IsLooped)
                Snowboard.IsLooped = true;

            if (cc.Body.Position.Z < 0 && cc.Body.Position.Z > -355)
                cc.ViewDirection = -BEPUutilities.Vector3.Forward;
            else
                cc.ViewDirection = WorldMatrix.Forward;

            Debug.WriteLine(cc.Body.Position);

            _positionStore = new Vector3(cc.Body.Position.X, cc.Body.Position.Y, cc.Body.Position.Z) + BodyOffset;
            View = playerRotation;


            // Reset mouse state 
            Mouse.SetPosition(_game.Window.ClientBounds.Width / 2, _game.Window.ClientBounds.Height / 2);

            oldMouseState = Mouse.GetState();
        }

        private double SqrMagnitude(Vector3 a)
        {
            return Math.Pow(a.X, 2) + Math.Pow(a.Y, 2) + Math.Pow(a.Z, 2);
        }

        private BEPUutilities.Vector3 ConvertTo(Vector3 a)
        {
            return new BEPUutilities.Vector3(a.X, a.Y, a.Z);
        }

        private double SqrMagnitude(Vector2 a)
        {
            return Math.Pow(a.X, 2) + Math.Pow(a.Y, 2);
        }

        public Vector2 HandleTranslation(GameTime time)
        {
            var deltaTime = (float)time.ElapsedGameTime.TotalSeconds;

            var keyboard = Keyboard.GetState();

            // Get the direction the player is currently facing
            var facing = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(CameraRotation.Y));


            Vector2 moveDirection = Vector2.Zero;

            bool moveXAxis = false;


            if (cc.Body.Position.Z < 0 && cc.Body.Position.Z > -355)
            {

                if (wasNotSliding)
                {
                    Woosh.Play();
                    Woosh.Volume = 0.5f;

                    if (Math.Abs(cc.Body.LinearVelocity.Y) > 0.5f)
                    {
                        Snowboard.Stop();
                        snowboardPlaying = false;
                    }
                    else
                    {
                        Snowboard.Play();
                        snowboardPlaying = true;
                    }
                    wasNotSliding = false;
                }

                cc.StandingSpeed = slideSpeed;

                moveX += Acceleration;
                moveXAxis = true;
            }
            else
            {
                if (snowboardPlaying)
                {
                    Snowboard.Stop();
                    snowboardPlaying = false;
                }

                wasNotSliding = true;

                cc.StandingSpeed = TopSpeed.X;

                //// Forward and backward movement
                if (keyboard.IsKeyDown(Keys.W) && !keyboard.IsKeyDown(Keys.S))
                {
                    moveX += Acceleration;
                    moveXAxis = true;
                }

                if (keyboard.IsKeyDown(Keys.S) && !keyboard.IsKeyDown(Keys.A))
                {
                    moveX -= Acceleration;
                    moveXAxis = true;
                }
            }


            if (!moveXAxis)
            {
                moveX = MathHelper.Lerp(moveX, 0, deltaTime * 10);
            }

            bool moveZAxis = false;

            // Strifing movement
            if (keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.D))
            {
                if (cc.Body.Position.Z < 0 && cc.Body.Position.Z > -355)
                {
                    moveZ += Acceleration / 4;
                }
                else
                {
                    moveZ += Acceleration;
                }
                moveZAxis = true;
            }
            if (keyboard.IsKeyDown(Keys.D) && !keyboard.IsKeyDown(Keys.A))
            {

                if (cc.Body.Position.Z < 0 && cc.Body.Position.Z > -355)
                {
                    moveZ -= Acceleration / 4;
                }
                else
                {
                    moveZ -= Acceleration;
                }

                moveZAxis = true;
            }

            if (!moveZAxis)
            {
                moveZ = MathHelper.Lerp(moveZ, 0, deltaTime * 10);
            }


            moveX = MathHelper.Clamp(moveX, -TopSpeed.X, TopSpeed.X);
            moveZ = MathHelper.Clamp(moveZ, -TopSpeed.Z / 2, TopSpeed.Z / 2);


            moveDirection = new Vector2(-moveZ, moveX);

            moveDirection = moveDirection * deltaTime;

            if (SqrMagnitude(moveDirection) > 0.0125)
            {
                moveDirection.Normalize();
            }
            else
            {
                moveDirection = Vector2.Zero;
            }

            if (!jumpPressed && keyboard.IsKeyDown(Keys.Space))
            {
                jumpPressed = true;

            }

            if (jumpPressed && keyboard.IsKeyUp(Keys.Space))
            {
                jumpPressed = false;
                cc.Jump();
            }



            //if (keyboard.IsKeyDown(Keys.LeftShift))
            //{
            //    cc.StandingSpeed = slideSpeed;
            //}
            //else
            //{
            //    cc.StandingSpeed = TopSpeed.X;
            //}

            //if(keyboard.IsKeyDown(Keys.LeftControl))
            //{
            //    cc.StanceManager.CrouchingHeight = 0.75f;
            //    cc.StanceManager.DesiredStance = Stance.Crouching;
            //}
            //else
            //{
            //    cc.StanceManager.DesiredStance = Stance.Standing;
            //}

            return moveDirection;
        }

        private Matrix HandlePlayerRotation(MouseState newMouseState)
        {

            // Adjust horizontal angle
            Y += Sensitivity * (oldMouseState.X - newMouseState.X);

            // Adjust vertical angle 
            X += Sensitivity * (oldMouseState.Y - newMouseState.Y);

            CameraRotation = new Vector3(X, Y, 0);

            //Debug.WriteLine(X + " " + Y);

            // create the view matrix
            return Matrix.CreateLookAt(Position, Position + Direction, Vector3.Up);
        }

        private void CameraShake(GameTime time)
        {
            float x = (float)noise.Evaluate(iterator, iterator) * HandheldCameraShakeAmount * 0.25f;
            float y = (float)noise.Evaluate(-iterator, -iterator) * HandheldCameraShakeAmount * 0.25f;
            float z = (float)noise.Evaluate(iterator, -iterator) * HandheldCameraShakeAmount * 0.25f;
            float rotation = (float)noise.Evaluate(iterator, -iterator) * HandheldCameraShakeAmount * 0.1f;

            _positionStore = new Vector3(_positionStore.X - lastIterationValueX + x, _positionStore.Y - lastIterationValueY + y, _positionStore.Z - lastIterationValueZ + z);
            //_rotationStore = _rotationStore - lastRotationValue + rotation; <- need to revisit the rotation attribute later

            lastIterationValueX = x;
            lastIterationValueY = y;
            lastIterationValueZ = z;
            lastRotationValue = rotation;

            iterator += HandheldCameraShakeFrequency * 0.10 * time.ElapsedGameTime.TotalSeconds;
        }
    }
}
