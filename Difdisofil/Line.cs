using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Difdisofil
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// зміна яка вказує чи переміщався спліт-контейнер на вкладці Лінія
        /// </summary>
        internal bool splitLine = false;

        /// <summary>
        /// Маркер для картинки на вкладці Лінія
        /// </summary>
        internal MarkerClass markerLine = new MarkerClass();

        /// <summary>
        /// Змінна як керує порядком збереження точок Лінії
        /// </summary>
        private bool markNumLine = true;

        /// <summary>
        /// Змінна яка дозволяє додавати лінію
        /// </summary>
        private bool markDozvilLine = false;

        /// <summary>
        /// Тимчасова змінна для лінії
        /// </summary>
        private PointSort tempLine;

        /// <summary>
        /// Масив точок для побудови лінії на графіку
        /// </summary>
        internal float[,] pointsOfLine;
        /// <summary>
        /// Масив скорегованих точок для побудови лінії на графіку
        /// </summary>
        internal float[,] pointsOfLineCor;

        /// <summary>
        /// Доступ до точок для побудови лінії на графіку
        /// </summary>
        internal float[,] PointsOfLine
        {
            get { return pointsOfLineCor; }
            set
            {
                // присвоюємо дані, до яких буде доступ лише для зчитування
                pointsOfLine = value;
                // проводимо згладжування даних фільтром Гаусса
                //pointsOfLineCor = value;
                if (value != null)
                    pointsOfLineCor = GaussFilter(pointsOfLine, 5, 0.9, FG.concentration);
            }
        }

        /// <summary>
        /// Вибір типу для синтезу фільтра Гаусса
        /// </summary>
        enum FG
        {
            /// <summary>
            /// за висотою, де висота визначає висоту крайніх елементів фільтра відносно максимума
            /// </summary>
            height,
            /// <summary>
            /// за концентрацією енергії, де КЕ визначає ширину між крайніми елементами фільтра в яких сформована
            /// певна кількість енергії відносно всієї
            /// </summary>
            concentration
        }

        #region Обробка миші для 3-ї вкладки "Лінія"

        /// <summary>
        /// Міняємо курсор, якщо він переміщається в області зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_MouseMove(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLine);

            // Переміщення зображення, якщо є дозвіл
            if (testMoveImg && (e.Button == MouseButtons.Right))
            {
                splitContainerLine.Panel1.AutoScrollPosition =
                    MouseMove(splitContainerLine.Panel1.AutoScrollPosition);
            }

            // виводимо значення координат в статусному рядку
            if (PictureBoxLine.Image != null)
            {
                // координати пікселів
                toolStripStatusLabel1.Text = $"Координати: ({e.X}; " +
                    $"{ConvetWorldCoordinate(e.Location, PictureBoxLine).Y}) [пікселів]";
                // амплітуда сигналу
                toolStripStatusLabel2.Text = $"Амплітуда сигналу: " +
                    $"{ApmlitudaSignal(e.Location, PictureBoxLine).ToString("F2")} [відносних одиниць]";
                // відстань між маркерами
                if (!markNumLine)
                    toolStripStatusLabel3.Text = $"Відстань: " + DimentionLine(e.Location, 
                        markerLine.MarkerList[0]).ToString("F2") + " [пікселів]";
            }
        }

        /// <summary>
        /// Мініємо курсор, якщо мишка входить в область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_MouseEnter(object sender, EventArgs e)
        {
            // якщо входить в межі, то міняємо змінну
            testMoveImg = true;

            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLine);
        }

        /// <summary>
        /// Міняємо курсор, якщо він виходить за область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_MouseLeave(object sender, EventArgs e)
        {
            // якщо виходить в межі, то міняємо змінну
            testMoveImg = false;

            // виходим за межі, то міняємо курсор
            PictureBoxLine.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Відбувається коли нажимається кнопка миши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_MouseDown(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLine);

            // ліва кнопка миши
            if ((PictureBoxLine.Cursor == Cursors.Cross) &&
                (e.Button == MouseButtons.Left))
            {
                // проставлення похилої лінії (ctrl)
                // якщо true - то перший маркер, false - другий
                if (markNumLine)
                {
                    // очищення
                    markerLine.MarkerList.Clear();
                    // зберігаємо дані (тимчасового) розташування для першого маркера
                    markerLine.MarkerList.Add(e.Location);
                    // проставлення нихиленої лінії (перша точка)
                    tempLine.Add(e.Location);
                    // змінюємо на доступ до другого маркера
                    markNumLine = false;
                    // змінюємо дозвіл на додавання лінії
                    markDozvilLine = false;
                }
                else
                {
                    // перевірка точок на збіг
                    if (!markerLine.MarkerList[0].Equals(e.Location))
                    {
                        // проставлення нихиленої лінії (друга точка)
                        tempLine.Add(e.Location);
                        // очистка маркера
                        markerLine.MarkerList.Clear();
                        // змінюємо на доступ до першого маркера
                        markNumLine = true;
                        // змінюємо дозвіл на додавання лінії
                        markDozvilLine = true;
                    }
                }

                // дозвіл на відображення маркерів
                markerLine.Dozvil = true;
            }
            else if ((PictureBoxLine.Cursor == Cursors.HSplit) &&
                (e.Button == MouseButtons.Left))
            {
                // проставлення горизонтальної лінії (shift)
                tempLine.Add(new Point[2]
                {
                    new Point(0, e.Y),
                    new Point(PictureBoxLine.Image.Width - 1, e.Y)
                });
                // очистка маркера
                markerLine.MarkerList.Clear();
                // змінюємо на доступ до першого маркера
                markNumLine = true;
                // змінюємо дозвіл на додавання лінії
                markDozvilLine = true;
            }
            else if ((PictureBoxLine.Cursor == Cursors.VSplit) &&
                (e.Button == MouseButtons.Left))
            {
                // проставлення вертикальної лінії (alt)
                tempLine.Add(new Point[2]
                {
                    new Point(e.X, 0),
                    new Point(e.X, PictureBoxLine.Image.Height - 1)
                });
                // очистка маркера
                markerLine.MarkerList.Clear();
                // змінюємо на доступ до першого маркера
                markNumLine = true;
                // змінюємо дозвіл на додавання лінії
                markDozvilLine = true;
            }

            // додавання лінії
            if (markDozvilLine)
            {
                // перевірка ліній
                if (markerLine.LineDict.Count > 0)
                {
                    // в пам'яті наявно більше однієї лінії, перевіряємо чи наступна
                    // не збігається координатами із попередніми
                    foreach (PointSort i in markerLine.LineDict.Values)
                    {
                        if (!i.FirstP.Equals(tempLine.FirstP) &&
                            !i.SecondP.Equals(tempLine.SecondP))
                        {
                            // якщо немає подібних то дозволяємо додавати лінію
                            markDozvilLine = true;
                        }
                        else
                        {
                            // якщо хоч одна є, то забороняємо ставити і виходимо із циклу
                            markDozvilLine = false;
                            break;
                        }
                    }

                    // якщо є дозвіл на збереження лінії, то зберігаємо її
                    if (markDozvilLine)
                    {
                        markerLine.LineDict.Add(markerLine.LineDict.Last().Key + 1,
                            tempLine);
                        markerLine.LineBool.Add(markerLine.LineBool.Last().Key + 1, false);
                        // змінюємо дозвіл на додавання лінії
                        markDozvilLine = false;
                        // виводимо відстань лінії
                        toolStripStatusLabel3.Text = $"Відстань: " + DimentionLine(tempLine.FirstP,
                            tempLine.SecondP).ToString("F2") + " [пікселів]";
                    }
                }
                else
                {
                    // робоча область чиста, додаємо першу лінію
                    markerLine.LineDict.Add(0, tempLine);
                    markerLine.LineBool.Add(0, false);
                    // змінюємо дозвіл на додавання лінії
                    markDozvilLine = false;
                    // виводимо відстань лінії
                    toolStripStatusLabel3.Text = $"Відстань: " + DimentionLine(tempLine.FirstP,
                        tempLine.SecondP).ToString("F2") + " [пікселів]";
                }

                // дозвіл на відображення маркерів
                markerLine.Dozvil = true;

                // оновлюємо таблицю ліній
                if (img.imageCorect != null)
                    TableShowLine(markerLine, img.imageCorect, dataGridViewLine);
            }

            // оновлення області для відобрадення маркера
            PictureBoxLine.Refresh();

            // збереження координат мишки
            MouseSave(e, splitContainerLine.Panel1.AutoScrollPosition);
        }

        /// <summary>
        /// Відбувається коли відпускається кнопка миші
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_MouseUp(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLine);
        }

        #endregion

        /// <summary>
        /// Відображення маркерів на вкладці Лінія
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLine_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (markerLine.Dozvil)
            {
                // малюємо маркери (якщо один - незакінчена лінія)
                if (markerLine.MarkerList.Count == 1)
                    markerLine.DrawMark(markerLine.MarkerList, g);

                // малюємо лінії, якщо вони є
                if (markerLine.LineDict.Count > 0)
                    markerLine.DrawMark(markerLine.LineDict, g);
            }

            //Оновлюємо область відображення
            Update();
        }

        /// <summary>
        /// Відображення таблиці із даними проставдених ліній вкладки Лінія
        /// </summary>
        /// <param name="markerClass">Дані ліній</param>
        /// <param name="bitmap">Зображення яке аналізується</param>
        /// <param name="tableName">Назва таблиці в якій відображатимуться дані</param>
        private void TableShowLine(MarkerClass markerClass, Bitmap bitmap, DataGridView tableName)
        {
            // Чистимо таблицю
            tableName.Columns.Clear();
            tableName.Rows.Clear();

            // якщо таблиця пуста то виходимо
            if (markerClass.LineDict.Count == 0)
                return;

            // Задаємо розміри таблиці
            int Ny = markerClass.LineDict.Count;    // рядки
            
            // задамо формат відображення всіх комірок
            tableName.DefaultCellStyle.Format = "F2";

            // Додаємо колонки і їх назви
            tableName.Columns.Add("Column1", "X0");
            tableName.Columns.Add("Column2", "Y0");
            tableName.Columns.Add("Column3", "Xn");
            tableName.Columns.Add("Column4", "Yn");
            tableName.Columns.Add("Column5", "L");
            tableName.Columns.Add("Column6", "α");
            tableName.Columns.Add("Column7", "N");

            // заборона редагування
            for (int i = 0; i < tableName.Columns.Count; i++)
                tableName.Columns[i].ReadOnly = true;

            tableName.Columns.Add(new DataGridViewCheckBoxColumn());
            tableName.Columns[tableName.Columns.Count - 1].HeaderText = "Us(Ni)";
            tableName.Columns[tableName.Columns.Count - 1].ReadOnly = false;

            {
                int k = 0;  // лічильник

                // Додаємо підказаки, які відображатимуться при наведенні
                tableName.Columns[k++].ToolTipText = "Координата X\nпершої точки\nзліва на право";
                tableName.Columns[k++].ToolTipText = "Координата Y\nпершої точки\nзнизу до верху";
                tableName.Columns[k++].ToolTipText = "Координата X\nдругої точки\nзліва на право";
                tableName.Columns[k++].ToolTipText = "Координата Y\nдругої точки\nзнизу до верху";
                tableName.Columns[k++].ToolTipText = "Довжина лінії\n[пікселів]";
                tableName.Columns[k++].ToolTipText = "Кут налилу лінії\n[градусів]";
                tableName.Columns[k++].ToolTipText = "Кількість точок для графіка\n[пікселів]";
                tableName.Columns[k++].ToolTipText = "Відображення на графіку і\nвиведення даних в таблицю";
            }

            // задамо формат відображення всіх комірок
            tableName.DefaultCellStyle.Format = "F2";
            // корегування формату деяких стовбців
            for (int i = 0; i < 4; i++)
                tableName.Columns[i].DefaultCellStyle.Format = "F0";
            tableName.Columns[6].DefaultCellStyle.Format = "F0";

            // Добавляємо кількість рядків в таблицю
            tableName.Rows.Add(Ny);

            int j = 1;  // лічильник для рядків

            foreach (KeyValuePair<int, PointSort> i in markerClass.LineDict)
            {
                // підписуємо нумерацію рядків
                tableName.Rows[Ny - j].HeaderCell.Value = i.Key.ToString();

                int k = 0;  // лічильник для стовбців

                // заповнюємо комірки даними
                tableName.Rows[Ny - j].Cells[k++].Value = i.Value.FirstL.X;
                tableName.Rows[Ny - j].Cells[k++].Value = bitmap.Height - 1 - i.Value.FirstL.Y;
                tableName.Rows[Ny - j].Cells[k++].Value = i.Value.SecondL.X;
                tableName.Rows[Ny - j].Cells[k++].Value = bitmap.Height - 1 - i.Value.SecondL.Y;
                tableName.Rows[Ny - j].Cells[k++].Value =
                    DimentionLine(i.Value.FirstL, i.Value.SecondL);
                tableName.Rows[Ny - j].Cells[k++].Value =
                    AngleLine(i.Value);// + " °";
                tableName.Rows[Ny - j].Cells[k++].Value = i.Value.PointN;
                tableName.Rows[Ny - j].Cells[k++].Value = markerClass.LineBool[i.Key];

                j++;
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
        /// Відбувається при видаленні рядків на вкладці Лінія
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewLine_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // видаляємо із словника маркери які були виділені в таблиці
            for (int i = 0; i < dataGridViewLine.SelectedRows.Count; i++)
            {
                int numDel = 
                    int.Parse(dataGridViewLine.SelectedRows[i].HeaderCell.Value.ToString());
                markerLine.LineDict.Remove(numDel);
                markerLine.LineBool.Remove(numDel);
            }

            // сортування (перезаписуємо словник для нормального добавляння елементів в кінець)
            markerLine.LineDict = markerLine.LineDict.ToDictionary(x => x.Key, y => y.Value);
            markerLine.LineBool = markerLine.LineBool.ToDictionary(x => x.Key, y => y.Value);

            // змінна яка вказує чи видалився відмічений рядок
            bool temp = false;

            // перевіряємо чи лишилися відмічені, якщо ні то чистимо вкладку Графік і таблиця
            foreach (bool i in markerLine.LineBool.Values)
                temp |= i;

            if (!temp)
                GraphClear();

            // оновлюємо зображення
            PictureBoxLine.Refresh();
        }

        /// <summary>
        /// Зміна вертикального сплітконтейнера на горизонтальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HorizLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerLine.Orientation.Equals(Orientation.Vertical))
                splitContainerLine.Orientation = Orientation.Horizontal;
        }

        /// <summary>
        /// Зміна горизонтального сплітконтейнера на вертикальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerticLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerLine.Orientation.Equals(Orientation.Horizontal))
                splitContainerLine.Orientation = Orientation.Vertical;
        }

        /// <summary>
        /// Коли спілт-контейнер перестрає бути активним
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitContainerLine_Leave(object sender, EventArgs e)
        {
            // Зберігаємо дані
            splitLine = true;
            SaveLoadSettings(SLS.save, EF.splitContainerLine);
        }

        /// <summary>
        /// Розрахунок нахилу лінії в градусах
        /// </summary>
        /// <param name="points">Точки лінії</param>
        /// <returns></returns>
        private float AngleLine(PointSort points)
        {
            float dX = points.FirstP.X - points.SecondP.X,
                dY = points.FirstP.Y - points.SecondP.Y;

            if (dX != 0)
                return -(float)(Math.Atan(dY / dX) * 180.0 / Math.PI);  // похила і горизонтальна лінії
            else
                return 90f; // вертикальна лінія
        }

        /// <summary>
        /// Відбувається коли починається редагування даних
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewLine_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // якщо дані останньої комірки були змінені, то:
            if ((dataGridViewLine.CurrentCell != null) &&
                (e.ColumnIndex == dataGridViewLine.Columns.Count - 1))
            {
                // знімаємо галочки із інших комірок
                for (int i = 0; i < dataGridViewLine.RowCount; i++)
                {
                    if (i != e.RowIndex)
                        dataGridViewLine.Rows[i].Cells[e.ColumnIndex].Value = false;
                }
            }
        }

        /// <summary>
        /// Відбувається коли закінчується редагування даних
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewLine_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Міняємо курсор на вид очікування
            this.Cursor = Cursors.AppStarting;

            if ((dataGridViewLine.CurrentCell != null) &&
                (e.ColumnIndex == dataGridViewLine.Columns.Count - 1))
            {
                // ключ для редагування словника ліній
                int key = int.Parse(dataGridViewLine.Rows[e.RowIndex].HeaderCell.Value.ToString());
                // встановлене користувачем значення
                bool value = bool.Parse(dataGridViewLine.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                // корегуємо масив даних
                markerLine.CorrectLine(key, value);
                // витягаємо дані із зображення, відповідно до вибраного користувачем значення
                if (value && (img.imageCorect != null))
                {
                    PointsOfLine = null;
                    // отримуємо дані із зображення
                    //pointsOfLine = img.GetPixelFromLine(markerLine.LineDict[key], img.imageCorect);
                    PointsOfLine = img.GetPixelFromLine(markerLine.LineDict[key], img.imageCorect);
                    // будуємо таблицю
                    TableShowGraph(PointsOfLine, dataGridViewGraph);
                    // будуємо графік
                    pictureBoxGraph.Refresh();
                }
                else
                {
                    GraphClear();
                }
            }

            // Повертаємо курсор назад
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Згладжування даних фільтром Гаусса
        /// </summary>
        /// <param name="data">вхідні дані</param>
        /// <param name="N">ширина вікна згладжування > 1</param>
        /// <param name="hc">висота/кількість сконцентрованої енергії (0;1)</param>
        /// <param name="fg">Вибір типу синтезу фільтра висота/концентрація енергії</param>
        /// <returns></returns>
        private float[,] GaussFilter(float[,] data, int N, double hc, FG fg)
        {
            // вікно завжди більше нуля, а якщо дорівнює нулю, то згладжування не відбувається
            // якщо елементів в масиві менше ніж ширина вікна, то згладжування не вівдбувається
            if ((N <= 1) || (data.GetLength(1) <= N))
                return data;

            // вихідний результат
            float[,] rezult = data;

            // корегуємо коректність введених даних для висоти/КЕ
            if (hc <= 0.0)
                hc = double.Epsilon;
            else if (hc >= 1.0)
                hc = 1.0 - double.Epsilon;

            // радіус кружка (розсіювання КЕ) Гауссового фільтру
            double rGauss = (N - 1) / 2.0;

            // кільцеві значення (радіуси) елементів фільтру
            double[] t = new double[N];

            for (int i = 0; i < N; i++)
                t[i] = -rGauss + i;

            // фільтр
            double[] filter = new double[N];

            // сумма значень фільтра
            double sum = 0.0;

            switch (fg)
            {
                case FG.height:
                    {
                        for (int i = 0; i < N; i++)
                        {
                            filter[i] = Math.Pow(hc, Math.Pow(t[i] / rGauss, 2.0));
                            sum += filter[i];
                        }
                    }
                    goto default;
                case FG.concentration:
                    {
                        double b = -Math.Pow(rGauss, -2.0) * Math.Log(1.0 - hc);
                        for (int i = 0; i < N; i++)
                        {
                            filter[i] = Math.Exp(-b * rGauss * rGauss);
                            sum += filter[i];
                        }
                    }
                    goto default;
                default:
                    {
                        for (int i = 0; i < N; i++)
                            filter[i] /= sum;
                    }
                    break;
            }

            // відступ від краю в масиві - рівний радіусу фільтра
            int minD = (int)Math.Ceiling(0.5 * filter.Length) - 1,
                maxD = data.GetLength(1) - minD;

            // проводимо згладжування
            for (int i = minD; i < maxD - 1; i++)
            {
                // очищуємо змінну для суми
                sum = 0.0;

                // сумуємо перемножуючи на фільтр певний діапазон
                for (int j = 0; j < N; j++)
                {
                    sum += data[1, i + j - minD] * filter[j];
                }

                // присвоюємо нове згладжене значення
                rezult[1, i] = (float)sum;
            }

            return rezult;
        }

    }
}
