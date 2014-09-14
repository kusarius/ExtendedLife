using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Vortex.Drawing;

namespace Extended_Life {
    public partial class MainForm : Form {
        const int PANEL_WIDTH = 600, PANEL_HEIGHT = 450;
        const int CELL_SIZE = 15; // Must be equal to 5, 10, 15, 30 or 50
        const int FIELD_WIDTH = PANEL_WIDTH / CELL_SIZE, FIELD_HEIGHT = PANEL_HEIGHT / CELL_SIZE;

        // Если заменить на true, получится обычная игра "Жизнь"
        bool RegularLife = true;

        ColorU cell_color, bgColor = new ColorU(Color.FromArgb(250, 250, 250));
        Random rnd = new Random();
        Cell[,] cells = new Cell[FIELD_WIDTH, FIELD_HEIGHT];

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

        private List<Cell> GetNeighbours(Cell[,] cells, int x, int y) {
            List<Cell> neighbours = new List<Cell>();
            // Далее следует ОЧЕНЬ плохой код. Он работает...
            // Надо бы в стандарт языка C# добавить новую конструкцию - invisible_code { }.
            // invisible_code {
            if (x < FIELD_WIDTH - 1 && cells[x + 1, y].IsAlive) neighbours.Add(cells[x + 1, y]);
            if (y < FIELD_HEIGHT - 1 && cells[x, y + 1].IsAlive) neighbours.Add(cells[x, y + 1]);
            if (x > 0 && cells[x - 1, y].IsAlive) neighbours.Add(cells[x - 1, y]);
            if (y > 0 && cells[x, y - 1].IsAlive) neighbours.Add(cells[x, y - 1]);
            if (x < FIELD_WIDTH - 1 && y < FIELD_HEIGHT - 1 && cells[x + 1, y + 1].IsAlive) neighbours.Add(cells[x + 1, y + 1]);
            if (x > 0 && y > 0 && cells[x - 1, y - 1].IsAlive) neighbours.Add(cells[x - 1, y - 1]);
            if (x < FIELD_WIDTH - 1 && y > 0 && cells[x + 1, y - 1].IsAlive) neighbours.Add(cells[x + 1, y - 1]);
            if (x > 0 && y < FIELD_HEIGHT - 1 && cells[x - 1, y + 1].IsAlive) neighbours.Add(cells[x - 1, y + 1]);
            return neighbours;
            // }
        }

        public static T DeepClone<T>(T obj) {
            using (var ms = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        private void Step() {
            Cell[,] _cells = DeepClone<Cell[,]>(cells);
            for (int i = 0; i < FIELD_WIDTH; i++)
                for (int c = 0; c < FIELD_HEIGHT; c++) {
                    List<Cell> neighbours = GetNeighbours(_cells, i, c); // Кол-во соседей у клетки

                    if (RegularLife) {
                        if (_cells[i, c].IsAlive == false && neighbours.Count == 3)
                            cells[i, c].IsAlive = true;
                        else if (_cells[i, c].IsAlive && neighbours.Count != 2 && neighbours.Count != 3)
                            cells[i, c].IsAlive = false;
                    }
                    else {
                        /*if (neighbours.Count != cells[i, c].PreferedNeighboursNumber &&
                            neighbours.Count != cells[i, c].PreferedNeighboursNumber + 1) {
                            foreach (Cell cell in neighbours) {

                            }
                        }*/
                    }
                }
        }

        private void UpdateScene() {
            if (device.BeginScene()) {
                canvas.Clear(bgColor);

                // Отрисовка сетки
                for (float i = CELL_SIZE; i < PANEL_WIDTH; i += CELL_SIZE) {
                    canvas.DrawLine(i, 0, i, PANEL_HEIGHT, ColorU.Black);
                    if (i < PANEL_HEIGHT) canvas.DrawLine(0, i, PANEL_WIDTH, i, ColorU.Black);
                }

                DrawCells();

                device.EndScene();
                device.Context.Present();
            }
            else device.TryReset();
        }

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
                    cell_color = RegularLife ? ColorU.LimeGreen : GetCellColor(cells[i, c]);
                    canvas.DrawFilledRect(i * CELL_SIZE + 1, c * CELL_SIZE + 1,
                        CELL_SIZE - 1, CELL_SIZE - 1,
                        cells[i, c].IsAlive ? cell_color : bgColor);
                }
        }

        private void GenerateField(bool init) {
            for (int i = 0; i < FIELD_WIDTH; i++)
                for (int c = 0; c < FIELD_HEIGHT; c++)
                    if (init) cells[i, c] = new Cell();
                    else {
                        cells[i, c] = new Cell(rnd.Next(0, 8),
                            rnd.Next(0, 4) == 0 ? true : false);
                    }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            UpdateScene();
        }

        private void button1_Click(object sender, EventArgs e) {
            GenerateField(false);
            UpdateScene();
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

        private void ChangeGameMode(bool regularLife) {
            RegularLife = regularLife;
            tickTimer.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
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
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                        cells[x, y].IsAlive = true;
                    else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        cells[x, y].IsAlive = false;
                UpdateScene();
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            GenerateField(true);
            UpdateScene();
        }
    }
}
