using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Vortex.Drawing;

namespace Extended_Life {
    public partial class MainForm : Form {
        // ТУДУ: Собрать движок Vortex2D под .NET 4.0

        const int CELL_SIZE = 5; // Должен быть равен 5, 10, 15, 30 или 50
        const int PANEL_WIDTH = 700, PANEL_HEIGHT = 500; // Ширина и высота панели должна быть на 2 пикселя больше указанной здесь
        const int FIELD_WIDTH = PANEL_WIDTH / CELL_SIZE, FIELD_HEIGHT = PANEL_HEIGHT / CELL_SIZE;

        // Режим игры
        bool RegularLife = true;
        int countOfSteps = 0;

        ColorU cell_color, bgColor = new ColorU(Color.FromArgb(250, 250, 250));
        Random rnd = new Random();
        Cell[,] cells = new Cell[FIELD_WIDTH, FIELD_HEIGHT];
        Cell[,] cells_clone;

        // Для отрисовки используется движок Vortex2D
        SingleContextDevice device;
        Canvas2D canvas;

        public MainForm() {
            InitializeComponent();

            device = new SingleContextDevice(panel1.Handle);
            device.Context.VerticalSync = false;
            canvas = device.Context.Canvas;

            GenerateField(true);
            toolTip.SetToolTip(trackBar1, "Update speed. Value - " + trackBar1.Value + " ms.");
        }

        private Cell[] GetNeighbours(Cell[,] cells, int x, int y, ref int alive) {
            Cell[] neighbours = new Cell[8];

            if (x < FIELD_WIDTH - 1) neighbours[0] = cells[x + 1, y];
            if (y < FIELD_HEIGHT - 1) neighbours[1] = cells[x, y + 1];
            if (x > 0) neighbours[2] = cells[x - 1, y];
            if (y > 0) neighbours[3] = cells[x, y - 1];
            if (x < FIELD_WIDTH - 1 && y < FIELD_HEIGHT - 1) neighbours[4] = cells[x + 1, y + 1];
            if (x > 0 && y > 0) neighbours[5] = cells[x - 1, y - 1];
            if (x < FIELD_WIDTH - 1 && y > 0) neighbours[6] = cells[x + 1, y - 1];
            if (x > 0 && y < FIELD_HEIGHT - 1) neighbours[7] = cells[x - 1, y + 1];

            alive = 0;
            for (int i = 0; i < 8; ++i)
                alive += (neighbours[i].IsAlive ? 1 : 0);

            return neighbours;
        }

        // Один шаг игры
        private void Step() {
            cells_clone = (Cell[,])cells.Clone();
            int nalive = 0; // Число активных соседей
            for (int i = 0; i < FIELD_WIDTH; i++)
                for (int c = 0; c < FIELD_HEIGHT; c++) {
                    Cell[] neighbours = GetNeighbours(cells_clone, i, c, ref nalive);

                    if (RegularLife) {
                        if (cells_clone[i, c].IsAlive == false && nalive == 3)
                            cells[i, c].IsAlive = true;
                        else if (cells_clone[i, c].IsAlive && nalive != 2 && nalive != 3)
                            cells[i, c].IsAlive = false;
                    }
                    else {
                        // ...
                    }
                }
            ++countOfSteps;
            stepCountLabel.Text = countOfSteps.ToString();
        }

        // Обновление изображения
        private void UpdateScene() {
            if (device.BeginScene()) {
                canvas.Clear(bgColor);
    
                // Отрисовка сетки
                for (float i = CELL_SIZE; i < PANEL_WIDTH; i += CELL_SIZE) {
                    canvas.DrawLine(i, 0, i, PANEL_HEIGHT, ColorU.Gray);
                    if (i < PANEL_HEIGHT) canvas.DrawLine(0, i, PANEL_WIDTH, i, ColorU.Gray);
                }

                DrawCells();

                device.EndScene();
                device.Context.Present();
            }
            else device.TryReset();
        }

        // Возвращает цвет ячейки в зависимости от её любимого кол-ва соседей
        private ColorU GetCellColor(Cell c) {
            switch (c.PreferedNeighboursNumber) {
                case 0: return new ColorU(Color.DarkBlue);
                case 1: return ColorU.Blue;
                case 2: return ColorU.DeepSkyBlue;
                case 3: return ColorU.Green;
                case 4: return ColorU.LimeGreen;
                case 5: return ColorU.Gold;
                case 6: return ColorU.Orange;
                case 7: return ColorU.Red;
                default: return bgColor;
            }
        }

        private void DrawCells() {
            for (int i = 0; i < FIELD_WIDTH; i++)
                for (int c = 0; c < FIELD_HEIGHT; c++) {
                    cell_color = RegularLife ? ColorU.Blue : GetCellColor(cells[i, c]);
                    canvas.DrawFilledRect(i * CELL_SIZE + 1, c * CELL_SIZE + 1,
                        CELL_SIZE - 1, CELL_SIZE - 1,
                        cells[i, c].IsAlive ? cell_color : bgColor);
                }
        }

        private void GenerateField(bool init) {
            if (init) cells = new Cell[FIELD_WIDTH, FIELD_HEIGHT];
            else {
                for (int i = 0; i < FIELD_WIDTH; i++)
                    for (int c = 0; c < FIELD_HEIGHT; c++)
                        cells[i, c] = new Cell(rnd.Next(0, 8), rnd.Next(0, 4) == 0 ? true : false);
            }        
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            UpdateScene();
        }

        private void button1_Click(object sender, EventArgs e) {
            GenerateField(false);
            UpdateScene();
            countOfSteps = 0;
            stepCountLabel.Text = countOfSteps.ToString();
        }

        private void button2_Click(object sender, EventArgs e) {
            tickTimer.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
        }

        // Таймер по тику обновляет поле
        private void tickTimer_Tick(object sender, EventArgs e) {
            Step();
            UpdateScene();
        }

        private void button3_Click(object sender, EventArgs e) {
            tickTimer.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            tickTimer.Interval = trackBar1.Value;

            // Обновляем подсказку
            toolTip.SetToolTip(trackBar1, "Update speed. Value - " + trackBar1.Value + " ms.");
        }

        // Меняет режим игры
        private void ChangeGameMode(bool regularLife) {
            RegularLife = regularLife;
            tickTimer.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
            countOfSteps = 0;
            stepCountLabel.Text = countOfSteps.ToString();
            GenerateField(true);
            UpdateScene();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            ChangeGameMode(true);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            ChangeGameMode(false);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e) {
            if (RegularLife) {
                int x = e.X / CELL_SIZE, y = e.Y / CELL_SIZE;
                if (x >= 0 && y >= 0 && x < FIELD_WIDTH && y < FIELD_HEIGHT)
                    if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                        cells[x, y].IsAlive = true;
                        UpdateScene();
                    }
                    else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                        cells[x, y].IsAlive = false;
                        UpdateScene();
                    }
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            GenerateField(true);
            UpdateScene();
            countOfSteps = 0;
            stepCountLabel.Text = countOfSteps.ToString();
        }
    }
}
