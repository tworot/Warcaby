using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warcaby
{
    public partial class Form1 : Form
    {
        private int tileSize;
        private Color clr1 = Color.DarkGray;
        private Color clr2 = Color.White;
        private List<Pawn> pawnList = new List<Pawn>();
        private List<Target> targetList = new List<Target>();
        private List<Move> moveList = new List<Move>();
        private Boolean isWhiteTurn = false;
        private Pawn activePawn = null;
        private Boolean isCaptureAvailable = false;
        private Boolean isMultiCapture = false;
        private GameRules currentGameRules = new GameRules();
        private Form2 optionsForm = null;

        public Panel[,] boardPanels = null;
        public Form1()
        {
            InitializeComponent();
            // InitializeBoard();
            NewGame();
        }

        public void NewGame() {

            currentGameRules = new GameRules();

            //resetting game-state parameters
            activePawn = null;
            isWhiteTurn = false;
            isCaptureAvailable = false;
            isMultiCapture = false;

            //populating board with pawns

            moveList.Clear();
            foreach (Target t in targetList) t.Dispose();
            targetList.Clear();

            if (boardPanels != null)
            {
                //made that way so panels from board wouldn't flicker when new game is created

                //create new board with new pawns
                List<Pawn> newPawns = new List<Pawn>();
                Panel[,] newPanels = InitializeBoard();
                InitializePawns(newPanels, newPawns);

                foreach (Pawn p in newPawns) p.ResetPawnGraphics();
                for (int i = 0; i < boardPanels.GetLength(0); i++) for (int j = 0; j < boardPanels.GetLength(1); j++)
                    {
                        boardPanels[i, j].Dispose();
                    }
                for (int i = 0; i < pawnList.Count; i++)
                {
                    pawnList[i].Dispose();
                }
                pawnList.Clear();
                boardPanels = newPanels;
                pawnList = newPawns;
            }
            else
            {
                this.boardPanels = InitializeBoard();
                InitializePawns(boardPanels, pawnList);
                foreach (Pawn p in pawnList) p.ResetPawnGraphics();
            }
            InitializeTurn();
        }

        public void InitializePawns(Panel[,] panels, List<Pawn> list)
        {
            Size size = new Size(this.tileSize, this.tileSize);
            for (int i = 0; i < currentGameRules.numberOfLines; i++)
                for (int j = i % 2; j < currentGameRules.horizontalSize; j += 2)
                    list.Add(new Pawn(j, i, true, false, panels, size, pawnClick));
            for (int i = currentGameRules.verticalSize - currentGameRules.numberOfLines; i < currentGameRules.verticalSize; i++)
                for (int j = i % 2; j < currentGameRules.horizontalSize; j += 2)
                    list.Add(new Pawn(j, i, false, false, panels, size, pawnClick));
        }

        public void SaveGame(String path)
        {
            GameState gs = new GameState();
            gs.rawPawnList = new List<RawPawn>();
            gs.gameRules = currentGameRules;
            gs.activePawnIndex = -1;
            gs.isWhiteTurn = isWhiteTurn;
            gs.isMultiCapture = isMultiCapture;

            for (int i = 0; i < pawnList.Count; i++)
            {
                gs.rawPawnList.Add(new RawPawn(pawnList[i]));
                if (pawnList[i] == activePawn) gs.activePawnIndex = i;
            }
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, gs);
                stream.Close();
            }
            catch {
                MessageBox.Show("Nie udało się zapisać gry!", "Błąd zapisu");
            }
        }

        public void LoadGame(String path)
        {
            GameState gs = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                gs = (GameState)formatter.Deserialize(stream);
                stream.Close();
            }
            catch {
                MessageBox.Show("Nie udało się załadować gry!", "Błąd wczytywania");
            }
            if (gs != null)
            {
                currentGameRules = gs.gameRules;

                //resetting game-state parameters
                activePawn = null;
                isWhiteTurn = gs.isWhiteTurn;
                isCaptureAvailable = false;
                isMultiCapture = gs.isMultiCapture;

                //populating board with pawns

                moveList.Clear();
                foreach (Target t in targetList) t.Dispose();
                targetList.Clear();
                //made that way so panels from board wouldn't flicker when new game is created

                //create new board with new pawns
                List<Pawn> newPawns = new List<Pawn>();
                Panel[,] newPanels = InitializeBoard();

                Size size = new Size(this.tileSize, this.tileSize);
                for (int i = 0; i < gs.rawPawnList.Count; i++)
                {
                    newPawns.Add(new Pawn(gs.rawPawnList[i], newPanels, size, pawnClick));
                    if (i == gs.activePawnIndex) activePawn = newPawns[i];
                }

                foreach (Pawn p in newPawns) p.ResetPawnGraphics();
                if (boardPanels != null)
                {
                    for (int i = 0; i < boardPanels.GetLength(0); i++) for (int j = 0; j < boardPanels.GetLength(1); j++)
                        {
                            boardPanels[i, j].Dispose();
                        }
                    for (int i = 0; i < pawnList.Count; i++)
                    {
                        pawnList[i].Dispose();
                    }
                    pawnList.Clear();

                }
                boardPanels = newPanels;
                pawnList = newPawns;
                SwitchLabel();
                StartMoveState(isMultiCapture);
            }
        }








        public void pawnClick(object sender, EventArgs e)
        {
            Pawn pawn = (Pawn)sender;
            if (!isMultiCapture || pawn == activePawn)
                wakeUpPawn(pawn);
        }

        public void wakeUpPawn(Pawn pawn) {
            if (isWhiteTurn == pawn.isWhite)
            {
                if (activePawn != null) activePawn.Deactivate();
                foreach (Target target in targetList)
                {
                    target.Dispose();
                }
                targetList.Clear();
                activePawn = pawn;
                activePawn.Activate();
                foreach (Move move in moveList)
                {
                    if (activePawn.vertical == move.pawn.vertical && activePawn.horizontal == move.pawn.horizontal)
                    {
                        Target target = new Target(move.horizontal, move.vertical, move.capturedPawn);
                        target.Parent = boardPanels[move.vertical, move.horizontal];
                        target.BackColor = Color.DarkGreen;
                        target.Size = new Size(tileSize, tileSize);
                        target.Click += new EventHandler(targetClick);
                        targetList.Add(target);
                    }
                }
            }
        }

        private void PromotionCheck() {
            if (activePawn.isWhite && activePawn.vertical == currentGameRules.verticalSize - 1)
            {
                activePawn.isKing = true;
                activePawn.ResetPawnGraphics();
            }
            if (!activePawn.isWhite && activePawn.vertical == 0)
            {
                activePawn.isKing = true;
                activePawn.ResetPawnGraphics();
            }
        }

        public void targetClick(object sender, EventArgs e) {
            Target target = (Target)sender;
            activePawn.Deactivate();
            activePawn.vertical = target.vertical;
            activePawn.horizontal = target.horizontal;
            bool hasCaptured = false;
            if (target.capturedPawn != null)
            {
                pawnList.RemoveAll(op => op.vertical == target.capturedPawn.vertical && op.horizontal == target.capturedPawn.horizontal);
                target.capturedPawn.Dispose();
                hasCaptured = true;
            }
            activePawn.Parent = boardPanels[target.vertical, target.horizontal];

            foreach (Target target2 in targetList)
            {
                target2.Dispose();
            }
            targetList.Clear();
            isCaptureAvailable = false;
            if (hasCaptured) StartMoveState();
            else
            {
                PromotionCheck();
                InitializeTurn();
            }
        }

        public void InitializeTurn()
        {
            isWhiteTurn = !isWhiteTurn;
            if (activePawn != null) activePawn.Deactivate();
            activePawn = null;
            SwitchLabel();
            StartMoveState(false);
        }
        
        public void StartMoveState(bool multiCapture = true)
        {
            isCaptureAvailable = false;
            moveList.Clear();

            if (multiCapture)
            {
                CheckCaptures(activePawn);
                if (isCaptureAvailable)
                {
                    if (currentGameRules.promotionDuringCapture) PromotionCheck();
                    wakeUpPawn(activePawn);
                    isMultiCapture = true;
                }
                else
                {
                    isMultiCapture = false;
                    PromotionCheck();
                    InitializeTurn();

                }
            }
            else
            {
                foreach (Pawn anyPawn in pawnList)
                {
                    if (anyPawn.isWhite == isWhiteTurn)
                    {
                        if (!isCaptureAvailable || !currentGameRules.mustCapture) CheckMoves(anyPawn);
                        CheckCaptures(anyPawn);
                    }
                }
                if (moveList.Count == 0)
                {
                    if (isWhiteTurn) MessageBox.Show("Wygrywa gracz czarny!", "Koniec gry");
                    else MessageBox.Show("Wygrywa gracz biały!", "Koniec gry");
                }
            }
        }

        public void CheckCaptures(Pawn sourcePawn) {
            if (sourcePawn.isKing)
                if (currentGameRules.kingMovesDiagonally)
                {
                    CheckDiagonalCapture(sourcePawn, 1, 1);
                    CheckDiagonalCapture(sourcePawn, 1, -1);
                    CheckDiagonalCapture(sourcePawn, -1, 1);
                    CheckDiagonalCapture(sourcePawn, -1, -1);
                }
                else
                {
                    if (sourcePawn.vertical > 1)
                    {
                        if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, -1);
                        if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, -1);
                    }
                    if (sourcePawn.vertical < currentGameRules.verticalSize - 2)
                    {
                        if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, 1);
                        if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, 1);
                    }
                }
            else if (currentGameRules.pawnCapturesBackwards)
            {
                if (sourcePawn.vertical > 1)
                {
                    if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, -1);
                    if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, -1);
                }
                if (sourcePawn.vertical < currentGameRules.verticalSize - 2)
                {
                    if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, 1);
                    if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, 1);
                }
            }
            else {
                if (sourcePawn.isWhite) if (sourcePawn.vertical < currentGameRules.verticalSize - 2)
                    {
                        if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, 1);
                        if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, 1);
                    }
                    else { }
                else if (sourcePawn.vertical > 1)
                {
                    if (sourcePawn.horizontal > 1) CheckRegularCapture(sourcePawn, -1, -1);
                    if (sourcePawn.horizontal < currentGameRules.horizontalSize - 2) CheckRegularCapture(sourcePawn, 1, -1);
                }
            }
        }

        public void CheckRegularCapture(Pawn sourcePawn, int directionHorizontal, int directionVertical) {
            Pawn capturablePawn = pawnList.Find(anyPawn => 
                (anyPawn.horizontal == sourcePawn.horizontal + directionHorizontal &&
                anyPawn.vertical == sourcePawn.vertical + directionVertical &&
                anyPawn.isWhite != sourcePawn.isWhite));
            directionHorizontal *= 2;
            directionVertical *= 2;
            if (capturablePawn != null && !pawnList.Exists(anyPawn => (
                anyPawn.vertical == sourcePawn.vertical + directionVertical &&
                anyPawn.horizontal == sourcePawn.horizontal + directionHorizontal))){
                if (!isCaptureAvailable ) { 
                    isCaptureAvailable = true; 
                    if (currentGameRules.mustCapture) moveList.Clear(); }
                moveList.Add(new Move(sourcePawn, sourcePawn.horizontal + directionHorizontal, sourcePawn.vertical + directionVertical, capturablePawn));
            }
        }

        public void CheckMoves(Pawn p) {
            if (p.isKing)
            {
                if (currentGameRules.kingMovesDiagonally)
                {
                    CheckDiagonalMove(p, 1, 1);
                    CheckDiagonalMove(p, 1, -1);
                    CheckDiagonalMove(p, -1, 1);
                    CheckDiagonalMove(p, -1, -1);
                }
                else {
                    if (p.vertical > 0) {
                        if (p.horizontal > 0) CheckRegularMove(p, -1, -1);
                        if (p.horizontal < currentGameRules.horizontalSize - 1) CheckRegularMove(p, 1, -1);
                    }
                    if (p.vertical < currentGameRules.verticalSize - 1) {
                        if (p.horizontal > 0) CheckRegularMove(p, -1, 1);
                        if (p.horizontal < currentGameRules.horizontalSize - 1) CheckRegularMove(p, 1, 1);
            }}}
            else {
                int direction = p.isWhite ? 1 : -1;
                if (p.horizontal > 0) CheckRegularMove(p, -1, direction);
                if (p.horizontal < currentGameRules.horizontalSize-1) CheckRegularMove(p, 1, direction);
            }
        }

        public void CheckRegularMove(Pawn p, int directionHorizontal, int directionVertical) {
            if (!pawnList.Exists(op => (op.vertical == p.vertical + directionVertical && op.horizontal == p.horizontal + directionHorizontal))) {
                moveList.Add(new Move(p, p.horizontal + directionHorizontal, p.vertical + directionVertical));
            }}
        
        public void CheckDiagonalMove(Pawn p, int directionHorizontal, int directionVertical)
        {
            int diagonalHorizontal = p.vertical + directionHorizontal, diagonalVertical = p.horizontal + directionVertical; bool keepRunning = true;

            while (diagonalHorizontal < currentGameRules.verticalSize && diagonalHorizontal >= 0 && diagonalVertical < currentGameRules.horizontalSize && diagonalVertical >= 0 && keepRunning) {
                if (pawnList.Exists(op => (op.vertical == diagonalHorizontal && op.horizontal == diagonalVertical)))
                {
                    keepRunning = false;
                }
                else {
                    moveList.Add(new Move(p, diagonalVertical, diagonalHorizontal));
                }
                diagonalHorizontal += directionHorizontal; diagonalVertical += directionVertical;
            }
        }

        public void CheckDiagonalCapture(Pawn p, int dirY, int dirX)
        {
            int tempY = p.vertical + dirY, tempX = p.horizontal + dirX; bool noFirstContact = true, noSecondContact = true;
            Pawn pawnToCapture = null;
            while (tempY < currentGameRules.verticalSize && tempY >= 0 && tempX < currentGameRules.horizontalSize && tempX >= 0 && noSecondContact)
            {
                Pawn tempPawn = pawnList.Find(op => (op.vertical == tempY && op.horizontal == tempX ));
                if (tempPawn != null) 
                    if(noFirstContact && tempPawn.isWhite != p.isWhite)
                    {
                        pawnToCapture = tempPawn;
                        noFirstContact = false;
                    }
                    else noSecondContact = false;
                else if(!noFirstContact)
                {
                    if (!isCaptureAvailable) { 
                        isCaptureAvailable = true; 
                        if (currentGameRules.mustCapture) moveList.Clear(); }
                    moveList.Add(new Move(p, tempX, tempY, pawnToCapture));
                }
                tempY += dirY; tempX += dirX;
            }
        }

        public void SwitchLabel()
        {
            if (isWhiteTurn)
            {
                label1.Text = "Gracz biały";
                label1.ForeColor = Color.Black;
                this.BackColor = Color.White;
            }
            else
            {
                label1.Text = "Gracza czarny";
                label1.ForeColor = Color.White;
                this.BackColor = Color.Black;

            }
        }

        private Panel[,] InitializeBoard()
        {
            Panel[,] boardPanels2 = new Panel[currentGameRules.verticalSize, currentGameRules.horizontalSize];
            tileSize = Math.Min(panel1.Width / currentGameRules.horizontalSize, panel1.Height / currentGameRules.verticalSize);
            int paddingHorizontal = (panel1.Width - (tileSize * currentGameRules.horizontalSize))/2;
            int paddingVertical = (panel1.Height - (tileSize * currentGameRules.verticalSize))/2;

            for (var n = 0; n < currentGameRules.verticalSize; n++)
            {
                for (var m = 0; m < currentGameRules.horizontalSize; m++)
                {
                    var newPanel = new Panel
                    {
                        Size = new Size(tileSize, tileSize),
                        Location = new Point(
                            tileSize * m + paddingHorizontal, 
                            tileSize * (currentGameRules.verticalSize -n -1) + paddingVertical
                    )};

                    // add to Form's Controls so that they show up
                    Controls.Add(newPanel);
                    newPanel.Parent = this.panel1;
                    newPanel.SendToBack();
                    // add to our 2d array of panels for future use
                    boardPanels2[n, m] = newPanel;

                    // color the backgrounds
                    if (n % 2 == 0)
                        newPanel.BackColor = m % 2 != 0 ? clr2 : clr1;
                    else
                        newPanel.BackColor = m % 2 != 0 ? clr1 : clr2;
                }
            }
            return boardPanels2;
        }

        private void RefreshBoard()
        {
            tileSize = Math.Min(panel1.Width / currentGameRules.horizontalSize, panel1.Height / currentGameRules.verticalSize);
            int paddingHorizontal = (panel1.Width - (tileSize * currentGameRules.horizontalSize)) / 2;
            int paddingVertical = (panel1.Height - (tileSize * currentGameRules.verticalSize)) / 2;

            for (var n = 0; n < currentGameRules.verticalSize; n++)
            {
                for (var m = 0; m < currentGameRules.horizontalSize; m++)
                {
                    boardPanels[n, m].Size = new Size(tileSize, tileSize);
                    boardPanels[n, m].Location = new Point(
                            tileSize * m + paddingHorizontal,
                            tileSize * (currentGameRules.verticalSize - n - 1) + paddingVertical
                    );
                }
            }
            foreach (Pawn p in pawnList) p.Size = new Size(tileSize, tileSize);
            foreach (Target t in targetList) t.Size = new Size(tileSize, tileSize);
        }

        private void panel1_Resize(object sender, EventArgs e) { RefreshBoard(); }

        private void button1_Click(object sender, EventArgs e) { 
            if (MessageBox.Show("Czy chcesz przerwać tą grę i rozpoczać kolejną?", "Nowa gra", MessageBoxButtons.YesNo) == DialogResult.Yes) NewGame(); }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (MessageBox.Show("Czy chcesz wyjść z gry?", "Wyjście", MessageBoxButtons.YesNo) == DialogResult.No) e.Cancel = true; }

        public void optionsFormToNull() { optionsForm = null; }

        private void button2_Click(object sender, EventArgs e)
        {
            if (optionsForm == null)
            {
                optionsForm = new Form2(this);
                optionsForm.ShowDialog();
            }
            else optionsForm.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Zapisz grę";
            saveFileDialog.DefaultExt = "wrcb";
            saveFileDialog.Filter = "Zapisy gry (*.wrcb)|*.wrcb|Wszystkie pliki (*.*)|*.*";
            saveFileDialog.CheckPathExists = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveGame(saveFileDialog.FileName);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Czy chcesz opuścić bieżącą grę?", "Wczytaj grę", MessageBoxButtons.YesNo) == DialogResult.Yes) { 
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Title = "Wczytaj grę";
                openFileDialog.DefaultExt = "wrcb";
                openFileDialog.Filter = "Zapisy gry (*.wrcb)|*.wrcb|Wszystkie pliki (*.*)|*.*";
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadGame(openFileDialog.FileName);
                }
            }
        }
    }
}
