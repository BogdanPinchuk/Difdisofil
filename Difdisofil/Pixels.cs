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
        /// зміна яка вказує чи переміщався спліт-контейнер на вкладці Піксель
        /// </summary>
        internal bool splitPixel = false;

        /// <summary>
        /// Маркер для картинки на вкладці Піксель
        /// </summary>
        internal MarkerClass markerPix = new MarkerClass();

        /// <summary>
        /// Змінна яка дозволяє ставити маркер на вкладці Піксель (щоб не було копій)
        /// </summary>
        private bool markDozvilPix = true;

        #region Обробка миші для 2-ї вкладки "Піксель"

        /// <summary>
        /// Коли мишка переміщається над елементом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_MouseMove(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxPixel);

            // Переміщення зображення, якщо є дозвіл
            if (testMoveImg && (e.Button == MouseButtons.Right))
            {
                splitContainerPixel.Panel1.AutoScrollPosition =
                    MouseMove(splitContainerPixel.Panel1.AutoScrollPosition);
            }

            // виводимо значення координат в статусному рядку
            if (PictureBoxPixel.Image != null)
            {
                // координати пікселів
                toolStripStatusLabel1.Text = $"Координати: ({e.X}; " +
                    $"{ConvetWorldCoordinate(e.Location, PictureBoxPixel).Y}) [пікселів]";
                // амплітуда сигналу
                toolStripStatusLabel2.Text = $"Амплітуда сигналу: " +
                    $"{ApmlitudaSignal(e.Location, PictureBoxPixel).ToString("F2")} [відносних одиниць]";
            }
        }

        /// <summary>
        /// Коли мишка входить в область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_MouseEnter(object sender, EventArgs e)
        {
            // якщо входить в межі, то міняємо змінну
            testMoveImg = true;

            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxPixel);
        }

        /// <summary>
        /// Коли мишка покидає область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_MouseLeave(object sender, EventArgs e)
        {
            // якщо виходить в межі, то міняємо змінну
            testMoveImg = false;

            // виходим за межі, то міняємо курсор
            PictureBoxPixel.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Відбувається коли нажимається кнопка миши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_MouseDown(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxPixel);

            // ліва кнопка миши
            if ((PictureBoxPixel.Cursor == Cursors.Cross) &&
                (e.Button == MouseButtons.Left))
            {
                // перевірка точок
                if (markerPix.MarkerDict.Count > 0)
                {
                    // в пам'яті наявно більше одного маркера, перевіряємо чи наступний 
                    // не збігається координатами із попередніми
                    foreach (Point i in markerPix.MarkerDict.Values)
                    {
                        if (!i.Equals(e.Location))
                        {
                            // якщо немає подібних то дозволяємо ставити маркер
                            markDozvilPix = true;
                        }
                        else
                        {
                            // якщо хоч один є забороняємо ставити і виходимо із циклу
                            markDozvilPix = false;
                            break;
                        }
                    }
                    
                    // якщо є дозвіл на збереження маркера, то зберігаємо його
                    if (markDozvilPix)
                    {
                        // додавання елемента
                        markerPix.MarkerDict.Add(markerPix.MarkerDict.Last().Key + 1, e.Location);
                        // забороняємо зберігати маркер
                        markDozvilPix = false;
                    }
                }
                else
                {
                    // робоча область чиста, ставимо переший маркер
                    markerPix.MarkerDict.Add(0, e.Location);
                    // забороняємо зберігати маркер
                    markDozvilPix = false;
                    // дозволяємо малювати маркери
                    markerPix.Dozvil = true;
                }

                // оновлення області для відобрадення маркера
                PictureBoxPixel.Refresh();

                // оновлюємо таблицю маркерів
                if (img.imageCorect != null)
                    TableShowPix(markerPix, img.imageCorect, dataGridViewPixel);
            }

            // збереження координат мишки
            MouseSave(e, splitContainerPixel.Panel1.AutoScrollPosition);
        }

        /// <summary>
        /// Відбувається коли відпускається кнопка миші
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_MouseUp(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxPixel);
        }

        #endregion

        /// <summary>
        /// Відображення маркерів на вкладці Піксель
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxPixel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (markerPix.Dozvil)
            {
                //Малюємо маркери
                markerPix.DrawMark(markerPix.MarkerDict, g);
            }

            //Оновлюємо область відображення
            Update();
        }

        /// <summary>
        /// Відображення таблиці із даними проставдених маркерів вкладки Піксель
        /// </summary>
        /// <param name="markerClass">Дані маркерів</param>
        /// <param name="bitmap">Зображення яке аналізується</param>
        /// <param name="tableName">Назва таблиці в якій відображатимуться дані</param>
        private void TableShowPix(MarkerClass markerClass, Bitmap bitmap, DataGridView tableName)
        {
            // Чистимо таблицю
            tableName.Columns.Clear();
            tableName.Rows.Clear();

            // якщо таблиця пуста то виходимо
            if (markerClass.MarkerDict.Count == 0)
                return;

            // Задаємо розміри таблиці
            int Ny = markerClass.MarkerDict.Count;    // рядки

            // Змінна для збереження значення амплітуд сигналів через небезпечний код
            float[] Us = new float[Ny];

            // Додаємо колонки і їх назви
            tableName.Columns.Add("Column1", "X");
            tableName.Columns.Add("Column2", "Y");
            tableName.Columns.Add("Column3", "Us");

            // Додаємо підказаки, які відображатимуться при наведенні
            tableName.Columns[0].ToolTipText = "Координата X\nзліва на право";
            tableName.Columns[1].ToolTipText = "Координата Y\nзнизу до верху";
            tableName.Columns[2].ToolTipText = "Амплітуда сигналу Us";

            // додаємо налаштування для забороги зміни значень
            for (int i = 0; i < tableName.Columns.Count; i++)
                tableName.Columns[i].ReadOnly = true;

            // задамо формат відображення всіх комірок
            tableName.DefaultCellStyle.Format = "F2";
            // корегування формату деяких стовбців
            tableName.Columns[0].DefaultCellStyle.Format = "F0";
            tableName.Columns[1].DefaultCellStyle.Format = "F0";

            // Добавляємо кількість рядків в таблицю
            tableName.Rows.Add(Ny);

            // Отримуємо значення амплітуд сигналів
            Us = img.GetPixelFromImage(markerClass.MarkerDict, bitmap);

            #region Ще один варінт перерахунку
            /* перевырити швидкість
            // Заповнюємо значеннями
            for (int i = 0; i < Ny; i++)
            {
                // підписуємо нумерацію рядків
                tableName.Rows[i].HeaderCell.Value = (Ny - i).ToString();

                // заповнюємо комірки даними
                tableName.Rows[i].Cells[0].Value = markerPix.MarkerList[Ny - i - 1].X;
                tableName.Rows[i].Cells[1].Value = bitmap.Height - 1 - markerPix.MarkerList[Ny - i - 1].Y;
                tableName.Rows[i].Cells[2].Value = Us[Ny - i - 1];
                //tableName.Rows[i].Cells[2].Value = bitmap.GetPixel(markerPix.markerMany[Ny - i - 1].X,
                //    markerPix.markerMany[Ny - i - 1].Y).G / 255f;
            }
            */
            #endregion

            int j = 1;  // лічильник для рядків

            foreach (KeyValuePair<int, Point> i in markerClass.MarkerDict)
            {
                // підписуємо нумерацію рядків
                tableName.Rows[Ny - j].HeaderCell.Value = i.Key.ToString();

                int k = 0;  // лічильник для стовбців

                // заповнюємо комірки даними
                tableName.Rows[Ny - j].Cells[k++].Value = i.Value.X;
                tableName.Rows[Ny - j].Cells[k++].Value = bitmap.Height - 1 - i.Value.Y;
                tableName.Rows[Ny - j].Cells[k++].Value = Us[j - 1];

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
        /// Відбувається при видаленні рядків на вкладці Піксель
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridViewPixel_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // видаляємо із словника маркери які були виділені в таблиці
            for (int i = 0; i < dataGridViewPixel.SelectedRows.Count; i++)
                markerPix.MarkerDict.Remove(int.Parse(dataGridViewPixel.
                    SelectedRows[i].HeaderCell.Value.ToString()));

            // сортування (перезаписуємо словник для нормального добавляння елементів в кінець)
            markerPix.MarkerDict = markerPix.MarkerDict.ToDictionary(x => x.Key, y => y.Value);

            // оновлюємо зображення
            PictureBoxPixel.Refresh();
        }

        /// <summary>
        /// Зміна вертикального сплітконтейнера на горизонтальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HorizPixelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerPixel.Orientation.Equals(Orientation.Vertical))
                splitContainerPixel.Orientation = Orientation.Horizontal;
        }

        /// <summary>
        /// Зміна горизонтального сплітконтейнера на вертикальний
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerticPixelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // міняємо орієнитацію
            if (splitContainerPixel.Orientation.Equals(Orientation.Horizontal))
                splitContainerPixel.Orientation = Orientation.Vertical;
        }

        /// <summary>
        /// Коли спілт-контейнер перестрає бути активним
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitContainerPixel_Leave(object sender, EventArgs e)
        {
            // Зберігаємо дані
            splitPixel = true;
            SaveLoadSettings(SLS.save, EF.splitContainerPixel);
        }

    }
}
