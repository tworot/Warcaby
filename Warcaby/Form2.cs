using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warcaby
{
    public partial class Form2 : Form
    {
        Form1 boardForm;
        GameRules tempGameRules;
        public Form2(Form1 boardForm)
        {
            //pass access to parent form
            this.boardForm = boardForm;

            //load object with current game rules
            tempGameRules = new GameRules();
            InitializeComponent();

            //move game rules from object to interface
            numericUpDown1.Value = tempGameRules.horizontalSize;
            numericUpDown1.Minimum = 5;
            numericUpDown1.Maximum = 30;
            numericUpDown2.Value = tempGameRules.verticalSize;
            numericUpDown2.Minimum = 5;
            numericUpDown2.Maximum = 30;
            numericUpDown3.Value = tempGameRules.numberOfLines;
            numericUpDown3.Minimum = 1;
            numericUpDown3.Maximum = (tempGameRules.verticalSize - 1) / 2;
            checkBox1.Checked = tempGameRules.pawnCapturesBackwards;
            checkBox2.Checked = tempGameRules.kingMovesDiagonally;
            checkBox3.Checked = tempGameRules.promotionDuringCapture;
            checkBox4.Checked = tempGameRules.mustCapture;
            this.Text = "Opcje gry";
            button2.Enabled = false;
        }

        private void SomethingChanged() {

            this.Text = "*Opcje gry";
            button2.Enabled = true;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            boardForm.optionsFormToNull();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            tempGameRules.horizontalSize = (int)numericUpDown1.Value;
            SomethingChanged();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            tempGameRules.verticalSize = (int)numericUpDown2.Value;
            SomethingChanged();
            int currentMax = (tempGameRules.verticalSize - 1) / 2;
            numericUpDown3.Maximum = currentMax;
            if (numericUpDown3.Value > currentMax)
            {
                numericUpDown3.Value = currentMax;
            }
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            tempGameRules.numberOfLines = (int)numericUpDown3.Value;
            SomethingChanged();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            tempGameRules.pawnCapturesBackwards = checkBox1.Checked;
            SomethingChanged();


        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            tempGameRules.kingMovesDiagonally = checkBox2.Checked;
            SomethingChanged();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            tempGameRules.promotionDuringCapture = checkBox3.Checked;
            SomethingChanged();

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            tempGameRules.mustCapture = checkBox4.Checked;
            SomethingChanged();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Text = "Opcje gry";
            button2.Enabled = false;
            tempGameRules.SaveRules();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Czy chcesz przerwać tą grę i rozpoczać kolejną?", "Nowa gra?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                tempGameRules.SaveRules();
                this.Close();
                boardForm.NewGame();
            }
        }
    }
}

