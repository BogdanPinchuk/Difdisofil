using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Difdisofil
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// зміна яка вказує чи переміщався спліт-контейнер на вкладці Графік і таблиця
        /// </summary>
        internal bool splitGraph = false;

        /// <summary>
        /// Виведення даних графіка
        /// </summary>
        /// <param name="data">Дані для графіка</param>
        /// <param name="tableName">Назва таблиці в якій відображатимуться дані</param>
        private void TableShowGraph(float[,] data, DataGridView tableName)
        {
            // Чистимо таблицю
            tableName.Columns.Clear();
            tableName.Rows.Clear();

            // якщо таблиця пуста то виходимо
            if (data == null)
                return;

            // Задаємо розміри таблиці
            int Ny = data.GetLength(1),     // рядки
                Nx = 2;                     // колонки

            // задамо формат відображення всіх комірок
            tableName.DefaultCellStyle.Format = "F2";

            // Додаємо колонки/рядки і їх назви
            tableName.Columns.Add("Column1", "L(Ni)");
            tableName.Columns.Add("Column1", "Us(Ni)");

            // Додаємо підказаки, які відображатимуться при наведенні
            tableName.Columns[0].ToolTipText = "Координата точки L";
            tableName.Columns[1].ToolTipText = "Амплітуда сигналу Us";

            // заборона редагування
            for (int i = 0; i < tableName.Columns.Count; i++)
                tableName.Columns[i].ReadOnly = true;

            // задамо формат відображення всіх комірок
            tableName.DefaultCellStyle.Format = "F2";

            // Добавляємо кількість рядків в таблицю
            tableName.Rows.Add(Ny);

            // підписуємо назви рядків
            // заповнюємо таблицю
            for (int i = 0; i < Ny; i++)
            {
                tableName.Rows[i].HeaderCell.Value = i.ToString();
                for (int j = 0; j < Nx; j++)
                    tableName.Rows[i].Cells[j].Value = data[j, i];
            }

            // вирівнюємо комірки за розмірами внутрішніх даних
            //tableName.AutoResizeColumnHeadersHeight();
            tableName.AutoResizeColumns();
            tableName.AutoResizeRows();
            tableName.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            // знімаємо виділення таблиці
            tableName.ClearSelection();
        }

        /// <summary>
        /// Оновлення таблиць - вирівнювання по даних
        /// </summary>
        /// <param name="tableName">Назва таблиці в якій відображатимуться дані</param>
        private void UpdateTable(DataGridView tableName)
        {
            // вирівнюємо комірки за розмірами внутрішніх даних
            tableName.AutoResizeColumns();
            //tableName.AutoResizeColumnHeadersHeight();
            tableName.AutoResizeRows();
            tableName.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        /// <summary>
        /// Зміна вертикального сплітконтейнера на горизонтальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HorizGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerGraph.Orientation.Equals(Orientation.Vertical))
                splitContainerGraph.Orientation = Orientation.Horizontal;
        }

        /// <summary>
        /// Зміна горизонтального сплітконтейнера на вертикальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerticGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerGraph.Orientation.Equals(Orientation.Horizontal))
                splitContainerGraph.Orientation = Orientation.Vertical;
        }

        /// <summary>
        /// Коли спілт-контейнер перестрає бути активним
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitContainerGraph_Leave(object sender, EventArgs e)
        {
            // Зберігаємо дані
            splitGraph = true;
            SaveLoadSettings(SLS.save, EF.splitContainerGraph);
        }

        /// <summary>
        /// Зображення графіка на вкладці Графік і таблиця
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxGraph_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            if (PointsOfLine != null)
            {
                DrawGraph(PointsOfLine, e.ClipRectangle, g);
            }

            //Оновлюємо область відображення
            Update();
        }

        /// <summary>
        /// Побудова графіка
        /// </summary>
        /// <param name="data">Дані для графіка</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        private void DrawGraph(float[,] data, Rectangle rec, Graphics g)
        {
            // Задаємо область для графіка
            Rectangle plotArea = rec;
            
            // визначаємо межі
            int h = plotArea.Height,
                w = plotArea.Width;

            // Задаємо підпис осей і заголовку
            string yLabel = "Амплітуда сигналу Us",
                xLabel = "Координати точки L",
                sTitle = "Дифракційний розподіл освітленості";

            // Задаємо шрифт і висоту тексту
            string nameF = "Arial";
            Font labelF = new Font(nameF, 11, FontStyle.Regular),
                titleF = new Font(nameF, 13, FontStyle.Regular),
                gridF = new Font(nameF, 9, FontStyle.Regular);
            StringFormat sFormat = new StringFormat
            {
                Alignment = StringAlignment.Center
            };

            // Розраховуємо величину шрифта
            SizeF xlabelFS = g.MeasureString(yLabel, labelF),
                ylabelFS = g.MeasureString(xLabel, labelF),
                titleFS = g.MeasureString(sTitle, titleF);

            // Визначаємо розміри масивів
            int Ny = data.GetLength(0),
                Nx = data.GetLength(1);

            // Задаємо сітку
            int nY = 5,
                nX = 10;

            // Задаємо максимум і мінімум по осям графіка
            float minY = 0f, maxY = 1f,
                minX = data[0, 0],
                maxX = data[0, Nx - 1];

            // крок для сітки
            float dY = (maxY - minY) / (nY - 1),
                dX = (maxX - minX) / (nX - 1);

            // Розраховуємо величину шрифта для розмірностей
            float gridFSL, gridFSU ;

            // величина шрифта розмірності під графіком
            gridFSL = g.MeasureString(minX.ToString("F2"), gridF).Height;

            // половина розмірності X виходитиме за межі графіка
            gridFSU = g.MeasureString(minX.ToString("F2"), gridF).Width;

            // визначаємо максимальний розмір значень по Y
            for (int i = 0; i < nY; i++)
                gridFSU = Math.Max(gridFSU, 
                    g.MeasureString((minY + i * dY).ToString("F2"), gridF).Width);

            // Задаємо розміри області графіка
            plotArea.X = 25 + (int)(ylabelFS.Height + gridFSU);
            plotArea.Y = 15 + (int)(titleFS.Height + gridFSL);

            plotArea.Width = 20 + w - plotArea.Left - (int)(gridFSU * 2.0f);
            plotArea.Height = h - plotArea.Top - (5 + (int)(xlabelFS.Height +
                gridFSL * 2.0f));

            // коефіцієнти масштабу
            double kfY = plotArea.Height / (maxY - minY),
                kfX = plotArea.Width / (maxX - minX);

            // Перераховуємо точки відповідно до масштабу графіка
            PointF[] plot = new PointF[Nx];

            for (int i = 0; i < Nx; i++)
                plot[i] = new PointF(Obmez(plotArea.Left, plotArea.Right, 
                    (float)(plotArea.Left + kfX * data[0, i])),
                    Obmez(plotArea.Top, plotArea.Bottom, 
                    (float)(plotArea.Bottom - kfY * data[1, i])));
            

            // Підписуємо осі і заголовок
            g.DrawString(sTitle, titleF, new SolidBrush(Color.Black),
                new PointF(plotArea.Left + plotArea.Width / 2, 15), sFormat);
            g.DrawString(xLabel, labelF, new SolidBrush(Color.Black),
                new PointF(plotArea.Left + plotArea.Width / 2, h - 25), sFormat);

            // Зберігаємо параметри області
            GraphicsState gState = g.Save();
            g.TranslateTransform(10, plotArea.Top + plotArea.Height / 2);
            // Розвертаємо текст
            g.RotateTransform(-90);
            g.DrawString(yLabel, labelF, new SolidBrush(Color.Black), 0, 0, sFormat);
            // Відновлюємо попередньо збережені параметри області
            g.Restore(gState);

            // Малюємо область графіка (закрашуємо білим)
            g.FillRectangle(new SolidBrush(Color.White), plotArea);

            // Малюємо осі по крайнім лівим і нижнім центрам пікселів
            Pen aPen = new Pen(Color.Gray, 1f)
            {
                DashStyle = DashStyle.Dash,
                DashPattern = new float[] { 5.0f, 5.0f }
            };

            // Зображуємо сітку
            // Горизантальні лінії сітки
            for (int i = 1; i < nY; i++)
                g.DrawLine(aPen, plotArea.Left, (float)(plotArea.Top + i * dY * kfY), 
                    plotArea.Right, (float)(plotArea.Top + i * dY * kfY));
            // Вертикальні лінії сітки
            for (int i = 1; i < nX; i++)
                g.DrawLine(aPen, (float)(plotArea.Left + i * dX * kfX), plotArea.Bottom, 
                    (float)(plotArea.Left + i * dX * kfX), plotArea.Top);

            // Підписуємо значення сітки
            string formatG = "F2";

            // внизу
            sFormat.Alignment = StringAlignment.Center;

            // Вертикально
            for (int i = 0; i <= nX; i++)
                g.DrawString((i * dX).ToString(formatG), gridF, 
                    new SolidBrush(Color.Black),
                    new PointF((float)(plotArea.Left + i * dX * kfX),
                    plotArea.Bottom + 5), sFormat);

            // зліва
            sFormat.Alignment = StringAlignment.Far;

            // Горизантально
            for (int i = 1; i <= nY; i++)
                g.DrawString((i * dY).ToString(formatG), gridF, 
                    new SolidBrush(Color.Black),
                    new PointF(plotArea.Left - 5, (float)(plotArea.Bottom - 
                    i * dY * kfY - gridFSL * 0.5f)), sFormat);

            // Зображуємо сам графік
            aPen.DashStyle = DashStyle.Solid;
            aPen.Color = Color.Black;
            aPen.Width = 2f;

            // малюємо сам графік
            //g.DrawLines(aPen, plot);
            // малювати необхідно по лініях, в такому разі не відбуватиметься вихід за межі
            for (int i = 0; i < plot.Length - 1; i++)
                g.DrawLine(aPen, plot[i], plot[i + 1]);

            // Малюємо область графіка (обводимо межі) [для того щоб не закрашували межі інші елементи]
            g.DrawRectangle(new Pen(Color.Black), plotArea);

            // Вивантажуємо графіку
            aPen.Dispose();
        }

        /// <summary>
        /// Обмеження області, щоб графік не виходив за межі
        /// </summary>
        /// <param name="min">мінімальне значення</param>
        /// <param name="max">максимальне значення</param>
        /// <param name="Ui">значення яке аналізується для корегування</param>
        /// <returns></returns>
        private float Obmez(float min, float max, float Ui)
        {
            if (Ui < min)
                Ui = min;
            else if (Ui > max)
                Ui = max;

            return Ui;
        }

    }
}
