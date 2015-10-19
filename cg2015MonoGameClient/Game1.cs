using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using WebAPIAuthenticationClient;


namespace cg2015MonoGameClient
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D _txBackground;
        Texture2D _txCharacter;
        //Texture2D _txCollectable;
        Point _posCharacter = new Point(0,0);
        IHubProxy proxy;
        SpriteFont font;
        double Exitcount = 10;
        bool GameOver = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            // TODO: Add your initialization logic here
            HubConnection connection = new HubConnection("http://cgmonogameserver2015.azurewebsites.net/");
            proxy = connection.CreateHubProxy("MoveCharacterHub");
            Action<Point> MoveRecieved = MovedRecievedMessage;
            proxy.On("setPosition", MoveRecieved);
            var valid  = PlayerAuthentication.login("powell.paul@itsligo.ie", "itsPaul$").Result;
            connection.Start().Wait();
             

            base.Initialize();
        }

        private void MovedRecievedMessage(Point obj)
        {
            _posCharacter = obj;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _txBackground = Content.Load<Texture2D>("islandicon_wogc");
            _txCharacter = Content.Load<Texture2D>(@"Players\p2");
            font = Content.Load<SpriteFont>("MessageFont");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                proxy.Invoke("GetPoint");
            if (!(PlayerAuthentication.PlayerStatus == AUTHSTATUS.OK))
            {
                Exitcount -= gameTime.ElapsedGameTime.TotalSeconds;
                if (Exitcount < 1)
                    Exit();                    
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
                spriteBatch.Begin();
            if ((PlayerAuthentication.PlayerStatus == AUTHSTATUS.OK))
            {
                spriteBatch.Draw(_txBackground, Vector2.Zero, Color.White);
                spriteBatch.Draw(_txCharacter, new Rectangle(_posCharacter, new Point(100, 100)), Color.White);
            }
            else
                spriteBatch.DrawString(font, "Exiting in " + ((int)Exitcount).ToString() + " owing to "  + PlayerAuthentication.PlayerStatus.ToString(), new Vector2(10, 10), Color.White);                

            spriteBatch.End();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
