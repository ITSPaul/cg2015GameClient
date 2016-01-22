using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using WebAPIAuthenticationClient;
using InputEngineNS;
using System.Threading.Tasks;

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
        HubConnection connection;
        //Texture2D _txCollectable;
        Point _posCharacter = new Point(0,0);
        IHubProxy proxy;
        SpriteFont font;
        double Exitcount = 10;
        bool GameOver = false;
        //List<string> TopScores = new List<string>();
        private bool _scoreboard;
        List<GameScoreObject> scores = new List<GameScoreObject>();
        List<string> chatMessages = new List<string>();

        // Set up a Viewport for the chat
        Viewport _chatvport;
        Viewport originalvport;
        IHubProxy chatproxy;
        bool chatMode = false;
        string line = string.Empty;

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
            new InputEngine(this);
            setupChatViewPort();
            // TODO: Add your initialization logic here
            //HubConnection connection = new HubConnection("http://cgmonogameserver2015.azurewebsites.net/");
            connection = new HubConnection("http://localhost:50574/");
            proxy = connection.CreateHubProxy("MoveCharacterHub");
            chatproxy = connection.CreateHubProxy("ChatHub");
            Action<Point> MoveRecieved = MovedRecievedMessage;
            proxy.On("setPosition", MoveRecieved);
            // Check Player Authentication constructor for endpoint setting
            // set to local host at the moment
            Task<bool> t = PlayerAuthentication.login("powell.paul@itsligo.ie", "itsPaul$1");
            t.Wait();
            Action<string, string> ChatRecieved = ChatRecievedMessage;
            chatproxy.On("heyThere", ChatRecieved);
            chatMessages.Add("Chat-->");
            connection.Start().Wait();
            base.Initialize();
        }

        private void setupChatViewPort()
        {
            originalvport = GraphicsDevice.Viewport;
            _chatvport = originalvport;
            _chatvport.Height = 200;
            _chatvport.X = 0;
            _chatvport.Y = originalvport.Height - _chatvport.Height;
        }

        private void ChatRecievedMessage(string from, string message)
        {

            chatMessages.Add(string.Concat(from, ":", message));
            chatMode = true;
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
            // Turn on chat mode
            if (InputEngine.IsKeyPressed(Keys.F1))
            {
                // line represent the current line to be captured
                line = string.Empty;
                chatMode = !chatMode;
            }
            // if chatting then do not update game window
            if (chatMode)
            {
                if(InputEngine.IsKeyPressed(Keys.Enter))
                    {
                    // replace connection id with name of logged in player
                    chatproxy.Invoke("SendMess", new object[] { connection.ConnectionId, line });
                    line = string.Empty;
                    //chatMessages.Add(line);
                    }
                else
                {
                    //if (InputEngine.PressedKeys.Length > 0)
                        
                                if(InputEngine.currentKey != Keys.None)
                                    line += InputEngine.lookupKeys[InputEngine.currentKey];
                }
            }
            // update game window
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    proxy.Invoke("GetPoint");
                if (!(PlayerAuthentication.PlayerStatus == AUTHSTATUS.OK))
                {
                    Exitcount -= gameTime.ElapsedGameTime.TotalSeconds;
                    if (Exitcount < 1)
                        Exit();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    scores = PlayerAuthentication.getScores(5, "Battle Call");
                    if (scores != null)
                    {
                        _scoreboard = true;

                    }
                }
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
            // Set the Viewport to the original
            GraphicsDevice.Viewport = originalvport;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (chatMode)
            {
                // Set the Viewport to the chat port, show any lines of text recorded
                GraphicsDevice.Viewport = _chatvport;
                spriteBatch.Begin();
                Vector2 pos = Vector2.Zero;
                foreach (string chatline in chatMessages)
                {
                    spriteBatch.DrawString(font, chatline, pos, Color.White);
                    pos += new Vector2(0, font.MeasureString(chatline).Y);

                }
                // write current line
                if (line.Length > 0)
                    spriteBatch.DrawString(font, line, pos, Color.White);
                spriteBatch.End();
            }
            // Set the Viewport to the original and show the game play
            GraphicsDevice.Viewport = originalvport;
            spriteBatch.Begin();
            if ((PlayerAuthentication.PlayerStatus == AUTHSTATUS.OK))
            {
                spriteBatch.Draw(_txBackground, Vector2.Zero, Color.White);
                spriteBatch.Draw(_txCharacter, new Rectangle(_posCharacter, new Point(100, 100)), Color.White);
                if(_scoreboard)
                {
                    Vector2 position = new Vector2(400, 200);
                    foreach (var item in scores)
                    {
                        spriteBatch.DrawString(font, item.GamerTag + ":" + item.score,position,Color.White);
                        position += new Vector2(0, 40);
                    }
                }

            }
            else
                spriteBatch.DrawString(font, "Exiting in " + ((int)Exitcount).ToString() + " owing to "  + PlayerAuthentication.PlayerStatus.ToString(), new Vector2(10, 10), Color.White);
            spriteBatch.End();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
