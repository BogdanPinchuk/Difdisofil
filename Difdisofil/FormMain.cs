using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

using mc = Difdisofil.TempImage.MyColor;
using System.Diagnostics;

namespace Difdisofil
{
    /// <summary>
    /// Головне діалогове вікно
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// Змінна завантаженого зображення
        /// </summary>
        internal TempImage img;

        /// <summary>
        /// Дозвід на переміщення зображення
        /// </summary>
        internal bool testMoveImg = false;

        /// <summary>
        /// Позиція курсора від початку нажимання кнопки миші
        /// </summary>
        internal Point posCursor;

        /// <summary>
        /// Позиція зображення відносно курсора від початку нажимання кнопки миші
        /// </summary>
        internal Point posImg;

        /// <summary>
        /// Маркер для картинки на вкладці Зображення
        /// </summary>
        internal MarkerClass markerZobr = new MarkerClass();

        /// <summary>
        /// Змінна як керує порядком проставлення маркерів вкладки Зображення
        /// </summary>
        private bool markNumZob = true;

        /// <summary>
        /// Змінна яка дозволяє ставити маркер
        /// </summary>
        private bool markDozvilZobr = true;

        /// <summary>
        /// Тимчасова змінна для внесення даних точок виділення області
        /// </summary>
        private PointOb markObrez;

        /// <summary>
        /// Перевірка чи вибрана компонента
        /// </summary>
        private bool colorVibor = false;

        /// <summary>
        /// Вибрана кольорова компонента
        /// </summary>
        private mc colorComp = mc.def;

        /// <summary>
        /// Ініціалізація форми
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Завантаження зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Запускаєм діалогове вікно в якому вибираємо файл зображення
            if (openFileDialogMain.ShowDialog() == DialogResult.OK)
            {
                // Пробуємо завантажити
                try
                {
                    using (Bitmap tempIm = new Bitmap(openFileDialogMain.FileName))
                    {
                        // Міняємо курсор на вид очікування
                        this.Cursor = Cursors.AppStarting;

                        // Чистимо робочу область
                        ClearAlltoolStripMenuItem_Click(sender, e);

                        // Присвоюємо зовнішній змінній картинку і "відпускаємо" файл
                        img.ImageSave = (Bitmap)tempIm.Clone();

                        // Відображаємо картинку
                        PictureBoxLoad.Image = img.ImageSave;

                        // Визначаємо стандартні розміри обрізки зображення
                        Obrezka.CalcOblast(img.ImageSave);

                        // Аналізуємо зображення
                        #region
                        //Тестування обробки зображення
                        img.GetInfoFromImage2(img.ImageSave);
                        #endregion

                        // Змінаємо галочку із кольорових компонент
                        CheckMenuColor(mc.def);

                        // Повертаємо курсор назад
                        this.Cursor = Cursors.Default;

                        // Забороняємо малювати маркер
                        markerZobr.Dozvil = false;
                        markerPix.Dozvil = false;
                        markerLine.Dozvil = false;

                        // Очищаємо статусний рядок
                        ClearStatusDown();
                    }
                }
                catch (Exception)
                {
                    // Видаємо помилку у випадку невірного формату зображення
                    MessageBox.Show("Format of this file is unknown!", "Error!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Файл незавантажений");
            }
        }

        /// <summary>
        /// Запус форми для введення параметрів зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormParam formParam = new FormParam();
            //formParam.Show();           // запуск форми з доступом до головної
            formParam.ShowDialog();     // запус форми з блокуванням головної
        }

        #region Обробка миші для 1-ї вкладки "Зображення"

        /// <summary>
        /// Відображення маркерів на вкладці Зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (markerZobr.Dozvil)
            {
                //Малюємо маркери
                markerZobr.DrawMark(markerZobr.MarkerList, g);
            }

            //Оновлюємо область відображення
            Update();
        }

        /// <summary>
        /// Міняємо курсор, якщо він переміщається в області зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_MouseMove(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLoad);

            // Переміщення зображення, якщо є дозвіл
            if (testMoveImg && (e.Button == MouseButtons.Right))
            {
                tabPage1.AutoScrollPosition = MouseMove(tabPage1.AutoScrollPosition);
            }

            // виводимо значення координат в статусному рядку
            if (PictureBoxLoad.Image != null)
            {
                toolStripStatusLabel1.Text = $"Координати: ({e.X}; "+
                    $"{ConvetWorldCoordinate(e.Location, PictureBoxLoad).Y}) [пікселів]";
            }
        }

        /// <summary>
        /// Мініємо курсор, якщо мишка входить в область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_MouseEnter(object sender, EventArgs e)
        {
            // якщо входить в межі, то міняємо змінну
            testMoveImg = true;

            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLoad);

        }

        /// <summary>
        /// Міняємо курсор, якщо він виходить за область елемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_MouseLeave(object sender, EventArgs e)
        {
            // якщо виходить в межі, то міняємо змінну
            testMoveImg = false;

            // виходим за межі, то міняємо курсор
            PictureBoxLoad.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Відбувається коли нажимається кнопка миши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_MouseDown(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLoad);

            // ліва кнопка миши
            if ((PictureBoxLoad.Cursor == Cursors.Cross) &&
                (e.Button == MouseButtons.Left))
            {
                // якщо є дозвіл на ставлення маркерів ставимо перший, а потім другий
                if (markDozvilZobr)
                {
                    // якщо true - то перший маркер, false - другий
                    if (markNumZob)
                    {
                        // зберігаємо дані розташування для першого маркера
                        markObrez.LU = e.Location;
                        // додаємо маркер для його відображення
                        markerZobr.MarkerList.Add(markObrez.LU);
                        // змінюємо на доступ до другого маркера
                        markNumZob = !markNumZob;
                    }
                    else
                    {
                        // перевірка точок
                        if ((markObrez.LU.X != e.Location.X) &&
                            (markObrez.LU.Y != e.Location.Y))
                        {
                            // зберігаємо дані розташування для другого маркера
                            markObrez.RD = e.Location;

                            // додаємо маркер для його відображення
                            markerZobr.MarkerList.Add(markObrez.RD);

                            // Присвоюємо розташування загальній змінній обрізаної області
                            Obrezka.Oblast = markObrez;

                            #region Відображення обрізаної області
                            if (img.ImageSave != null)
                            {
                                // Обрізаємо зображення
                                img.GetInfoFromImage2(img.ImageSave, Obrezka.Oblast);

                                if (colorVibor)
                                {
                                    // Основлюємо зображення
                                    PixelClear();
                                    LineClear();

                                    // оновлюємо малюнки
                                    GrayColor(colorComp);
                                }

                                // корегуємо титульне зображення
                                img.SetInfoFromImage(Obrezka.Oblast);

                                // Оновлюємо зображення
                                PictureBoxLoad.Image = img.ImageCut;
                            }
                            #endregion

                            // змінюємо на доступ до першого маркера
                            markNumZob = !markNumZob;

                            // змінюємо дозвіл на ставлення маркерів
                            markDozvilZobr = false;
                        }
                    }

                    // дозвіл на відображення маркерів
                    markerZobr.Dozvil = true;

                    // оновлення області для відобрадення маркера
                    PictureBoxLoad.Refresh();
                }
            }

            // збереження координат мишки
            MouseSave(e, tabPage1.AutoScrollPosition);
        }

        /// <summary>
        /// Відбувається коли відпускається кнопка миші
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxLoad_MouseUp(object sender, MouseEventArgs e)
        {
            // Керуємо відображенням курсора миші
            MouseForm(PictureBoxLoad);
        }

        #endregion
        
        #region Обробка натискань клавіатури

        /// <summary>
        /// Відбувається при натисканні клавіші клавіатури
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlMain_KeyDown(object sender, KeyEventArgs e)
        {
            TabControlMain_KeyUp(sender, e);
        }

        /// <summary>
        /// Відбувається при відпусканні клавіші клавіатури
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlMain_KeyUp(object sender, KeyEventArgs e)
        {
            // тимчасова змінна для PictureBox
            PictureBox picB = PictureBoxLoad;

            // Ctrl
            switch (tabControlMain.SelectedIndex)
            {
                case 0:
                    picB = PictureBoxLoad;
                    goto default;
                case 1:
                    picB = PictureBoxPixel;
                    goto default;
                case 2:
                    picB = PictureBoxLine;
                    goto default;
                default:
                    MouseForm(picB);
                    break;
            }
        }
        #endregion

        #region Компоненти кольорів для відображення

        /// <summary>
        /// Розрахунок і відображення сірої компоненти на зображенні
        /// </summary>
        /// <param name="componenta"></param>
        private void GrayColor(mc componenta)
        {
            if (img.ImageSave != null)
            {
                // Міняємо курсор на вид очікування
                this.Cursor = Cursors.AppStarting;

                if (markerZobr.MarkerList.Count < 2)
                {
                    img.CloneColor();
                    Obrezka.CalcOblast(img.ImageSave);
                }

                // Реалізуємо необхідну компонету
                img.SetInfoFromImage(componenta, Obrezka.Oblast);

                // Ставимо галочку
                CheckMenuColor(componenta);

                // відображаємо зображення 2/3 вкладці
                PictureBoxPixel.Image = img.imageCorect;
                PictureBoxLine.Image = img.imageCorect;

                // оновлюємо таблиці
                if (img.imageCorect != null)
                {
                    TableShowPix(markerPix, img.imageCorect, dataGridViewPixel);
                    TableShowLine(markerLine, img.imageCorect, dataGridViewLine);
                    TableShowGraph(PointsOfLine, dataGridViewGraph);
                    pictureBoxGraph.Refresh();
                }

                // Повертаємо курсор назад
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Відображення галочкою в меню вибраної компоненти
        /// </summary>
        /// <param name="componenta"></param>
        private void CheckMenuColor(mc componenta)
        {
            // змінамємо всі галочки
            #region
            redToolStripMenuItem.Checked = false;
            greenToolStripMenuItem.Checked = false;
            blueToolStripMenuItem.Checked = false;

            cyanToolStripMenuItem.Checked = false;
            magentaToolStripMenuItem.Checked = false;
            yellowToolStripMenuItem.Checked = false;
            blackToolStripMenuItem.Checked = false;

            minToolStripMenuItem.Checked = false;
            medToolStripMenuItem.Checked = false;
            maxToolStripMenuItem.Checked = false;

            hmToolStripMenuItem.Checked = false;
            gmToolStripMenuItem.Checked = false;
            amToolStripMenuItem.Checked = false;
            rmsToolStripMenuItem.Checked = false;
            cmToolStripMenuItem.Checked = false;
            #endregion

            // чи вибрана якась із компонент
            colorVibor = true;

            // зберігаємо в пам'яті, яку компоненту ми вибрали
            colorComp = componenta;

            switch (componenta)
            {
                #region
                case mc.red:
                    redToolStripMenuItem.Checked = true;
                    break;
                case mc.green:
                    greenToolStripMenuItem.Checked = true;
                    break;
                case mc.blue:
                    blueToolStripMenuItem.Checked = true;
                    break;
                case mc.cyan:
                    cyanToolStripMenuItem.Checked = true;
                    break;
                case mc.magenta:
                    magentaToolStripMenuItem.Checked = true;
                    break;
                case mc.yellow:
                    yellowToolStripMenuItem.Checked = true;
                    break;
                case mc.black:
                    blackToolStripMenuItem.Checked = true;
                    break;
                case mc.min:
                    minToolStripMenuItem.Checked = true;
                    break;
                case mc.med:
                    medToolStripMenuItem.Checked = true;
                    break;
                case mc.max:
                    maxToolStripMenuItem.Checked = true;
                    break;
                case mc.hm:
                    hmToolStripMenuItem.Checked = true;
                    break;
                case mc.gm:
                    gmToolStripMenuItem.Checked = true;
                    break;
                case mc.am:
                    amToolStripMenuItem.Checked = true;
                    break;
                case mc.rms:
                    rmsToolStripMenuItem.Checked = true;
                    break;
                case mc.cm:
                    cmToolStripMenuItem.Checked = true;
                    break;
                default:
                    colorVibor = false;
                    break;
                    #endregion
            }
        }

        /// <summary>
        /// Червона комонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.red);
        }

        /// <summary>
        /// Зелена компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.green);
        }

        /// <summary>
        /// Синня компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.blue);
        }

        /// <summary>
        /// Блакитна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CyanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.cyan);
        }

        /// <summary>
        /// Пурпурна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MagentaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.magenta);
        }

        /// <summary>
        /// Жовта компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YellowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.yellow);
        }

        /// <summary>
        /// Чорна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.black);
        }

        /// <summary>
        /// Мінімальна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.min);
        }

        /// <summary>
        /// Медіанна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.med);
        }

        /// <summary>
        /// Максимальна компонента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.max);
        }

        /// <summary>
        /// Середнє гармонійне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.hm);
        }

        /// <summary>
        /// Середнє геометричне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.gm);
        }

        /// <summary>
        /// Середнє арифметичне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.am);
        }

        /// <summary>
        /// Середнє квадратичне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.rms);
        }

        /// <summary>
        /// Середнє степеневе (кубічне)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Реалізуємо необхідну компонету
            GrayColor(mc.cm);
        }

        #endregion

        #region Управління відображенням курсора
        /// <summary>
        /// Зиіна форми вказівника мишки в залежності від того де вона знаходиться
        /// </summary>
        /// <param name="picB">Область зображенння над якою знаходиться мишка</param>
        private void MouseForm(PictureBox picB)
        {
            if (MouseButtons == MouseButtons.Right)
            {
                // віддаємо перевагу нажаттю правої кнопки миші
                picB.Cursor = Cursors.SizeAll;
            }
            else if (ModifierKeys == Keys.Control)
            {
                // якщо зажатий ctrl, то міняємо курсор
                picB.Cursor = Cursors.Cross;
            }
            else if ((ModifierKeys == Keys.Shift) && 
                (picB.Name == PictureBoxLine.Name))
            {
                // якщо зажатий shift, то міняємо курсор
                picB.Cursor = Cursors.HSplit;
            }
            else if ((ModifierKeys == Keys.Alt) &&
                (picB.Name == PictureBoxLine.Name))
            {
                // якщо зажатий alt, то міняємо курсор
                picB.Cursor = Cursors.VSplit;
            }
            else
            {
                // нічого не нажато, то вигляд - стандартний
                picB.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Збереження позиції курсора
        /// </summary>
        /// <param name="e">Параметри мишки</param>
        /// <param name="pos">Позиція автоскролів</param>
        private void MouseSave(MouseEventArgs e, Point pos)
        {
            // права кнопка миши
            if (e.Button == MouseButtons.Right)
            {
                // Зберігаємо позицію місця курсора миші
                posCursor = MousePosition;

                // Зберігаємо позицію повзунків
                posImg.X = -pos.X;
                posImg.Y = -pos.Y;
            }
        }

        /// <summary>
        /// Переміщення зображення при нажатій правій клавіші мишки
        /// </summary>
        /// <param name="e">Параметри мишки</param>
        /// <param name="pos">Позиція автоскролів</param>
        private new Point MouseMove(Point pos)
        {
            // Різниця переміщення курсора
            Point deltaCur = new Point
            {
                // визначаємо різницю переменіщення
                X = MousePosition.X - posCursor.X,
                Y = MousePosition.Y - posCursor.Y
            };

            pos = new Point(posImg.X - deltaCur.X, posImg.Y - deltaCur.Y);

            return pos;
        }
        #endregion

        #region Очищення робочої області
        /// <summary>
        /// Очищення всіх вкладок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAlltoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Зображення
            ImageToolStripMenuItem_Click(sender, e);

            // оновлюємо зображення
            if (colorVibor)
                GrayColor(colorComp);
        }

        /// <summary>
        /// Очищення вкладки Зображення
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageClear();
        }

        /// <summary>
        /// Очищення вкладки Піксель
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PixelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PixelClear();
        }

        /// <summary>
        /// Очищення вкладки Лінія
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LineClear();
        }

        /// <summary>
        /// Очищення вкладки Графік і таблиця
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphClear();
        }

        /// <summary>
        /// Очищення вкладки Зображення
        /// </summary>
        private void ImageClear()
        {
            // Забороняємо малювати маркер
            markerZobr.Dozvil = false;

            // змінюємо дозвіл на ставлення маркерів
            markDozvilZobr = true;

            // чистимо список маркерів
            markerZobr.MarkerList.Clear();

            // якщо картинка наявна в пам'яті, то відображаємо
            if (img.ImageSave != null)
            {
                // Міняємо курсор на вид очікування
                this.Cursor = Cursors.AppStarting;

                // Відображаємо картинку
                PictureBoxLoad.Image = img.ImageSave;

                // корегуємо область видимості
                Obrezka.CalcOblast(img.ImageSave);

                // оновлюємо малюнки
                //GrayColor(colorComp);

                // Повертаємо курсор назад
                this.Cursor = Cursors.Default;
            }

            // оновлення області для відобрадення маркера
            PictureBoxLoad.Refresh();

            // Необхідно почистити макрери на інших вкладках
            PixelClear();
            LineClear();
        }

        /// <summary>
        /// Очищення вкладки Піксель
        /// </summary>
        private void PixelClear()
        {
            // Забороняємо малювати маркер
            markerPix.Dozvil = false;

            // чистимо список маркерів
            markerPix.MarkerDict.Clear();

            // чистимо статусний рядок
            ClearStatusDown();

            if (img.imageCorect != null)
            {
                // Міняємо курсор на вид очікування
                this.Cursor = Cursors.AppStarting;

                // Відображаємо картинку
                PictureBoxPixel.Image = img.imageCorect;

                // оновлюємо таблицю маркерів
                TableShowPix(markerPix, img.imageCorect, dataGridViewPixel);

                // Повертаємо курсор назад
                this.Cursor = Cursors.Default;
            }

            // оновлення області для відобрадення маркера
            PictureBoxPixel.Refresh();
        }

        /// <summary>
        /// Очищення вкладки Лінія
        /// </summary>
        private void LineClear()
        {
            // Забороняємо малювати лінії
            markerLine.Dozvil = false;

            // повертаємося до першого маркера
            markNumLine = true;

            // чистимо список ліній і маркера
            markerLine.LineDict.Clear();
            markerLine.LineBool.Clear();
            markerLine.MarkerList.Clear();

            // чистимо статусний рядок
            ClearStatusDown();

            if (img.imageCorect != null)
            {
                // Міняємо курсор на вид очікування
                this.Cursor = Cursors.AppStarting;

                // Відображаємо картинку
                PictureBoxLine.Image = img.imageCorect;

                // оновлюємо таблицю маркерів
                TableShowLine(markerLine, img.imageCorect, dataGridViewLine);

                // Повертаємо курсор назад
                this.Cursor = Cursors.Default;
            }

            // чистимо графік і таблицю
            GraphClear();

            // оновлення області для відобрадення маркера
            PictureBoxLine.Refresh();
        }

        /// <summary>
        /// Очищення вкладки Графік і таблиця
        /// </summary>
        private void GraphClear()
        {
            // чистимо дані
            PointsOfLine = null;

            if (img.imageCorect != null)
            {
                // Міняємо курсор на вид очікування
                this.Cursor = Cursors.AppStarting;

                // оновлюємо таблицю
                TableShowGraph(PointsOfLine, dataGridViewGraph);
                pictureBoxGraph.Refresh();

                // Повертаємо курсор назад
                this.Cursor = Cursors.Default;
            }
            
        }

        /// <summary>
        /// Очищення стутусного рядка внизу
        /// </summary>
        private void ClearStatusDown()
        {
            toolStripStatusLabel1.Text = default(string);
            toolStripStatusLabel2.Text = default(string);
            toolStripStatusLabel3.Text = default(string);
        }

        #endregion

        /// <summary>
        /// Конвертація із комп'ютерних (зверху вниз) у світові координати (знизу вверх)
        /// </summary>
        /// <param name="compCoord">Координати розташування</param>
        /// <param name="picB">Елемент зображення на якому вимірюються координати</param>
        /// <returns></returns>
        private Point ConvetWorldCoordinate(Point compCoord, PictureBox picB)
        {
            // Віднімаємо від розміру картинки значення розташування мишки
            return new Point(compCoord.X, picB.Image.Height - compCoord.Y - 1);
        }

        /// <summary>
        /// Розрахунок амплітуди сигналу
        /// </summary>
        /// <param name="compCoord"></param>
        /// <param name="picB"></param>
        /// <returns></returns>
        private float ApmlitudaSignal(Point compCoord, PictureBox picB)
        {
            return ((Bitmap)picB.Image).GetPixel(compCoord.X, compCoord.Y).G / 255f;
        }

        /// <summary>
        /// Розрахунок відстані між двома маркерами, для лінії
        /// </summary>
        /// <param name="mouse">Розташування мишки/розташування другого маркера</param>
        /// <param name="save">Збережене розташування першого маркера</param>
        /// <returns></returns>
        private float DimentionLine(Point mouse, Point save)
        {
            return (float)Math.Sqrt(Math.Pow(mouse.X - save.X, 2.0) +
                        Math.Pow(mouse.Y - save.Y, 2.0));
        }

        /// <summary>
        /// Відбувається при зміні вкладок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // якщо вкладки переключилися, то чистимо статусний рядок
            ClearStatusDown();

            // корегуємо налаштування
            switch (tabControlMain.SelectedIndex)
            {
                case 0:
                    goto default;
                case 1:
                    SaveLoadSettings(SLS.load, EF.splitContainerPixel);
                    UpdateTable(dataGridViewPixel);
                    break;
                case 2:
                    SaveLoadSettings(SLS.load, EF.splitContainerLine);
                    UpdateTable(dataGridViewLine);
                    break;
                case 3:
                    SaveLoadSettings(SLS.load, EF.splitContainerGraph);
                    UpdateTable(dataGridViewGraph);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Інформація про програму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Назва: {AccessibilityObject.Name}\n" +
                $"Версія: {ProductVersion}\n\n" +
                "Програму розробив:\n\nПінчук Богдан Юрійович\n" +
                "pinchuk.brus@gmail.com\n\n" +
                "для кафедри Оптичних та оптико-електронних приладів\n" +
                "та для КПС ПБ \"Арсенал\"", 
                "Інформація про програму Difdisofil",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
