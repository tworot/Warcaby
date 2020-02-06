using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warcaby
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form1 = new Form1();
            Application.Run(form1);
        }
    }

    public class Pawn : Panel
    {
        public int horizontal, vertical;
        public Boolean isWhite, isKing;

        public Pawn(int horizontal, int vertical, Boolean isWhite, Boolean isKing, Panel[,] boardPanels, Size size, Action<object, EventArgs> pawnClick)
        {
            this.horizontal = horizontal; this.vertical = vertical; this.isWhite = isWhite; this.isKing = isKing;
            this.RestOfInit(boardPanels, size, pawnClick);
        }

        public Pawn(RawPawn rawPawn, Panel[,] boardPanels, Size size, Action<object, EventArgs> pawnClick)
        {
            this.horizontal = rawPawn.horizontal; this.vertical = rawPawn.vertical;
            this.isWhite = rawPawn.isWhite; this.isKing = rawPawn.isKing;
            this.RestOfInit(boardPanels, size, pawnClick);
        }

        private void RestOfInit(Panel[,] boardPanels, Size size, Action<object, EventArgs> pawnClick)
        {
            this.Parent = boardPanels[vertical, horizontal];
            this.Size = size;
            //this.ResetPawnGraphics();
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Click += new EventHandler(pawnClick);
            this.BringToFront();
        }

        public void Activate()
        {
            Parent.BackColor = Color.DarkRed;
        }
        public void Deactivate()
        {
            Parent.BackColor = Color.DarkGray;
        }
        public void ResetPawnGraphics()
        {
            /* if (this.isKing)
                 if (this.isWhite) this.BackgroundImage = Image.FromFile("images/bialadama.png");
                 else this.BackgroundImage = Image.FromFile("images/czarnadama.png");
             else if (this.isWhite) this.BackgroundImage = Image.FromFile("images/bialypion.png");
             else this.BackgroundImage = Image.FromFile("images/czarnypion.png"); */

            if (this.isKing)
                if (this.isWhite) this.BackgroundImage = Properties.Resources.whiteKing;
                else this.BackgroundImage = Properties.Resources.blackKing;
            else if (this.isWhite) this.BackgroundImage = Properties.Resources.whitePawn;
            else this.BackgroundImage = Properties.Resources.blackPawn;
            
        }
    }
    
    public class Target : Panel {
        public int horizontal, vertical;
        public Pawn capturedPawn;

        public Target(int horizontal, int vertical, Pawn capturedPawn = null) {
            this.horizontal = horizontal; this.vertical = vertical; this.capturedPawn = capturedPawn;
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
    }

    public class Move {
        public Pawn pawn; public int horizontal, vertical;
        public Boolean isCapture; public Pawn capturedPawn;

        public Move(Pawn pawn, int horizontal, int vertical, Pawn capturedPawn = null) {
            this.pawn = pawn; this.horizontal = horizontal; this.vertical = vertical; this.capturedPawn = capturedPawn;
            this.isCapture = (capturedPawn != null); 
        }
    }
    [Serializable]
    public class GameRules
    {
        public Boolean pawnCapturesBackwards, kingMovesDiagonally, promotionDuringCapture, mustCapture;
        public int horizontalSize, verticalSize, numberOfLines;

        public GameRules(Boolean pawnCapturesBackwards = true, Boolean kingMovesDiagonally = true, Boolean promotionDuringCapture = true, Boolean mustCapture = true, int horizontalSize = 8, int verticalSize = 8, int numberOfLines = 3) {
            this.pawnCapturesBackwards = pawnCapturesBackwards;
            this.kingMovesDiagonally = kingMovesDiagonally;
            this.promotionDuringCapture = promotionDuringCapture;
            this.mustCapture = mustCapture;
            this.horizontalSize = horizontalSize;
            this.verticalSize = verticalSize;
            this.numberOfLines = numberOfLines;
        }

        public GameRules() {
            this.pawnCapturesBackwards = (Boolean)Properties.Settings.Default["pawnCapturesBackwards"];
            this.kingMovesDiagonally = (Boolean)Properties.Settings.Default["kingMovesDiagonally"];
            this.promotionDuringCapture = (Boolean)Properties.Settings.Default["promotionDuringCapture"];
            this.mustCapture = (Boolean)Properties.Settings.Default["mustCapture"];
            this.horizontalSize = (int)Properties.Settings.Default["horizontalSize"];
            this.verticalSize = (int)Properties.Settings.Default["verticalSize"];
            this.numberOfLines = (int)Properties.Settings.Default["numberOfLines"];
        }

        public void SaveRules() {
            Properties.Settings.Default["pawnCapturesBackwards"] = this.pawnCapturesBackwards;
            Properties.Settings.Default["kingMovesDiagonally"] = this.kingMovesDiagonally;
            Properties.Settings.Default["promotionDuringCapture"] = this.promotionDuringCapture;
            Properties.Settings.Default["mustCapture"] = this.mustCapture;
            Properties.Settings.Default["horizontalSize"] = this.horizontalSize;
            Properties.Settings.Default["verticalSize"] = this.verticalSize;
            Properties.Settings.Default["numberOfLines"] = this.numberOfLines;
            Properties.Settings.Default.Save();
        }
    }

    [Serializable]
    public class GameState {
        public Boolean isWhiteTurn;
        public Boolean isMultiCapture;
        public GameRules gameRules;
        public List<RawPawn> rawPawnList;
        public int activePawnIndex;
    }

    [Serializable]
    public class RawPawn
    {
        public int horizontal, vertical;
        public Boolean isWhite, isKing;

        public RawPawn(int horizontal, int vertical, Boolean isWhite, Boolean isKing)
        {
            this.horizontal = horizontal; this.vertical = vertical; this.isWhite = isWhite; this.isKing = isKing;
        }
        public RawPawn(Pawn pawn)
        {
            this.horizontal = pawn.horizontal; this.vertical = pawn.vertical;
            this.isWhite = pawn.isWhite; this.isKing = pawn.isKing;
        }

    }

}
