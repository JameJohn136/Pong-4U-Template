/*
 * Description:     A basic PONG simulator
 * Author:        James Johnson   
 * Date:            Feb 3rd 2023
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {
        #region global values

        //graphics objects for drawing
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush blueBrush = new SolidBrush(Color.Blue);
        SolidBrush redBrush = new SolidBrush(Color.Red);
        Font drawFont = new Font("Courier New", 10);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer wallBounceSound = new SoundPlayer(Properties.Resources.wallBounce1);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);

        //determines whether a key is being pressed or not
        Boolean wKeyDown, sKeyDown, upKeyDown, downKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;

        //ball values
        Boolean ballMoveRight = true;
        Boolean ballMoveDown = true;
        const int BALL_SPEED = 4;
        const int BALL_WIDTH = 20;
        const int BALL_HEIGHT = 20; 
        Rectangle ball;

        //player values
        const int PADDLE_SPEED = 4;
        const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            
        const int PADDLE_WIDTH = 10;
        const int PADDLE_HEIGHT = 40;
        Rectangle player1, player2;

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        int gameWinScore = 2;  // number of points needed to win game

        #endregion

        public Form1()
        {
            collisionSound.Stream = null;
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if a key is pressed and set is KeyDown value to true if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = true;
                    break;
                case Keys.S:
                    sKeyDown = true;
                    break;
                case Keys.Up:
                    upKeyDown = true;
                    break;
                case Keys.Down:
                    downKeyDown = true;
                    break;
                case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.Escape:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }
        
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = false;
                    break;
                case Keys.S:
                    sKeyDown = false;
                    break;
                case Keys.Up:
                    upKeyDown = false;
                    break;
                case Keys.Down:
                    downKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {
            if (newGameOk)
            {
                player1Score = player2Score = 0;
                newGameOk = false;
                startLabel.Visible = false;
                gameUpdateLoop.Start();
            }

            //player start positions
            player1 = new Rectangle(PADDLE_EDGE, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);
            player2 = new Rectangle(this.Width - PADDLE_EDGE - PADDLE_WIDTH, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);

            ball = new Rectangle(this.Width / 2 - BALL_WIDTH, this.Height / 2, BALL_WIDTH, BALL_HEIGHT);

            // Update Score Text
            UpdateScore();

        }

        private void UpdateScore()
        {
            player1ScoreLabel.Text = player1Score.ToString();
            plaery2ScoreLabel.Text = player2Score.ToString();
        }

        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {
            #region update ball position

            // Move Ball Side to Side
            if (ballMoveRight)
            {
                ball.X += BALL_SPEED;
            } else
            {
                ball.X -= BALL_SPEED;
            }

            // Move Ball up and down
            if (ballMoveDown)
            {
                ball.Y += BALL_SPEED;
            } else
            {
                ball.Y -= BALL_SPEED;
            }



            #endregion

            #region update paddle positions

            if (wKeyDown == true && player1.Y > 0)
            {
                player1.Y -= PADDLE_SPEED;
            }

            if (sKeyDown == true && player1.Y < this.Height - PADDLE_HEIGHT)
            {
                player1.Y += PADDLE_SPEED;
            }

            if (upKeyDown == true && player2.Y > 0)
            {
                player2.Y -= PADDLE_SPEED;
            }

            if (downKeyDown == true && player2.Y < this.Height - PADDLE_HEIGHT)
            {
                player2.Y += PADDLE_SPEED;
            }

            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y < 0) // if ball hits top line
            {
                collisionSound.Play();
                ballMoveDown = true;
            } else if (ball.Y >= this.Height - BALL_HEIGHT)
            {
                collisionSound.Play();
                ballMoveDown = false;
            }



            #endregion

            #region ball collision with paddles

            if (ball.IntersectsWith(player1) || ball.IntersectsWith(player2))
            {
                collisionSound.Play();
                
                if (ball.IntersectsWith(player1))
                {
                    ballMoveRight = true;
                } else
                {
                    ballMoveRight = false;
                }

                /*
                 * I tried to just reverse the ballMoveRight bool with
                 * ballMoveRight = !ballMoveRight;
                 * but it caused issues when the ball hit the side of the paddle
                 * as it would knock it back and forth
                 */
            }

            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < 0)  // ball hits left wall logic
            {
                // TODO
                scoreSound.Play();
                player2Score++;
                UpdateScore();


                if (player2Score >= gameWinScore)
                {
                    GameOver("Player 2");
                } else
                {
                    ballMoveRight = !ballMoveRight;
                    SetParameters();
                }

            }

            if (ball.X > this.Width)  // ball hits right wall logic
            {
                // TODO
                scoreSound.Play();
                player1Score++;
                UpdateScore();


                if (player1Score >= gameWinScore)
                {
                    GameOver("Player 1");
                }
                else
                {
                    ballMoveRight = !ballMoveRight;
                    SetParameters();
                }

            }


            #endregion

            //refresh the screen, which causes the Form1_Paint method to run
            this.Refresh();
        }
        
        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(string winner)
        {

            startLabel.Text = $"{winner} Wins!!!\n Play Again?\n Space to play again\n ESC to stop";
            gameUpdateLoop.Enabled = false;
            startLabel.Visible = true;
            newGameOk = true;
            Refresh();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(redBrush, player1);
            e.Graphics.FillRectangle(blueBrush, player2);

            if (ballMoveRight)
            {
                e.Graphics.FillRectangle(redBrush, ball);
            } else
            {
                e.Graphics.FillRectangle(blueBrush, ball);
            }

        }

    }
}
