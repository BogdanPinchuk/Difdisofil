using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace Difdisofil
{
    /// <summary>
    /// Контейнер в якому знаходитиметься базове зображення
    /// </summary>
    internal struct TempImage
    {
        /// <summary>
        /// Набір кольорових компонент
        /// </summary>
        internal enum MyColor
        {
            /// <summary>
            /// Додаткова компонента для знання галочки, default
            /// </summary>
            def,
            /// <summary>
            /// R - червона, RGB
            /// </summary>
            red,
            /// <summary>
            /// G - зелена, RGB
            /// </summary>
            green,
            /// <summary>
            /// B - синя, RGB
            /// </summary>
            blue,
            /// <summary>
            /// C - блакитний, CMYK
            /// </summary>
            cyan,
            /// <summary>
            /// M - пурпурний, CMYK
            /// </summary>
            magenta,
            /// <summary>
            /// Y - жовтий, CMYK
            /// </summary>
            yellow,
            /// <summary>
            /// K - чорний, CMYK
            /// </summary>
            black,
            /// <summary>
            /// Min - мінімальне, RGB
            /// </summary>
            min,
            /// <summary>
            /// Med - медіана, RGB
            /// </summary>
            med,
            /// <summary>
            /// Max - максимальна, RGB
            /// </summary>
            max,
            /// <summary>
            /// HM - середнє гармонійне "-1"
            /// </summary>
            hm,
            /// <summary>
            /// GM - середнє геометричне "0"
            /// </summary>
            gm,
            /// <summary>
            /// AM - середня арифметичне "1"
            /// </summary>
            am,
            /// <summary>
            /// RMS - середнє квадратичне "2"
            /// </summary>
            rms,
            /// <summary>
            /// CM - середнє кубічне (степеневе) "3"
            /// </summary>
            cm
        }

        /// <summary>
        /// Завантажене зображення
        /// </summary>
        private Bitmap imageSave;

        /// <summary>
        /// Обрізане зображення
        /// </summary>
        private Bitmap imageCut;

        /// <summary>
        /// Зображення над яким проводяться математичні перетворення
        /// </summary>
        internal Bitmap imageCorect;

        /// <summary>
        /// Розміри зображення
        /// </summary>
        private Size sizeImage;

        /// <summary>
        /// Дані кольорів головного зображення
        /// </summary>
        private byte[][,] imgPixels;

        /// <summary>
        /// Дані кольорів обрізаного зображення
        /// </summary>
        private byte[][,] imgPixels2;

        /// <summary>
        /// Змінна для визначення часу виконання
        /// </summary>
        //Stopwatch times;

        /// <summary>
        /// Доступ до базового зображення
        /// </summary>
        internal Bitmap ImageSave
        {
            get { return imageSave; }
            set
            {
                imageSave = value;
                // зберігаємо дані про розміри зображення
                SizeImage = imageSave.Size;
            }
        }

        /// <summary>
        /// Обрізана частина зображення
        /// </summary>
        internal Bitmap ImageCut
        {
            get { return imageCut; }
            set { imageCut = value; }
        }

        /// <summary>
        /// Розміри картинки
        /// </summary>
        internal Size SizeImage
        {
            get { return sizeImage; }
            set { sizeImage = value; }
        }

        /// <summary>
        /// Доступ до кольору картинки
        /// </summary>
        internal byte[][,] ImgPixels
        {
            get { return imgPixels; }
            set { imgPixels = value; }
        }

        #region Отримання даних із зображення

        /// <summary>
        /// Отримання кольорів пікселів, наївний метод
        /// </summary>
        /// <param name="bitmap">Зображення із якого отримуватиемться RGB кольори</param>
        internal void GetInfoFromImage0(Bitmap bitmap)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            int w = bitmap.Width,
                h = bitmap.Height;

            // Створюємо/перезаписуємо екземпляр для даних кольорів
            imgPixels = new byte[4][,];
            for (int i = 0; i < 4; i++)
                imgPixels[i] = new byte[w, h];

            // Отримуємо дані кольорів від зображення
            for (int i = 0; i < h; i++) // ширина зображення
            {
                for (int j = 0; j < w; j++)  // висота зображення
                {
                    imgPixels[0][j, i] = bitmap.GetPixel(j, i).A;   // компонента прозорості
                    imgPixels[1][j, i] = bitmap.GetPixel(j, i).R;   // компонента червона
                    imgPixels[2][j, i] = bitmap.GetPixel(j, i).G;   // компонента зелена
                    imgPixels[3][j, i] = bitmap.GetPixel(j, i).B;   // компонента синня
                }
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");

            //imgPixels0 = imgPixels;
        }

        /// <summary>
        /// Отримання кольорів пікселів, прямий робота із Bitmap
        /// </summary>
        /// <param name="bitmap">Зображення із якого отримуватиемться RGB кольори</param>
        internal unsafe void GetInfoFromImage1(Bitmap bitmap)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            int w = bitmap.Width,
                h = bitmap.Height;

            // Створюємо/перезаписуємо екземпляр для даних кольорів
            imgPixels = new byte[4][,];
            for (int i = 0; i < 4; i++)
                imgPixels[i] = new byte[w, h];

            BitmapData BD = bitmap.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            
            try
            {
                byte* curpos;
                
                for (int i = 0; i < h; i++) // висота зображення
                {
                    curpos = ((byte*)BD.Scan0) + i * BD.Stride;

                    for (int j = 0; j < w; j++)  // ширина зображення
                    {
                        imgPixels[3][j, i] = *(curpos++);   // компонента синня
                        imgPixels[2][j, i] = *(curpos++);   // компонента зелена
                        imgPixels[1][j, i] = *(curpos++);   // компонента червона
                        imgPixels[0][j, i] = *(curpos++);   // компонента прозорості
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(BD);
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");

            //imgPixels1 = imgPixels;
        }

        /// <summary>
        /// Отримання кольорів пікселів, з використанням вказівників
        /// </summary>
        /// <param name="bitmap">Зображення із якого отримуватиемться RGB кольори</param>
        internal unsafe void GetInfoFromImage2(Bitmap bitmap)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            int w = bitmap.Width,
                h = bitmap.Height;

            // Створюємо/перезаписуємо екземпляр для даних кольорів
            imgPixels = new byte[4][,];
            for (int i = 0; i < 4; i++)
                imgPixels[i] = new byte[w, h];

            BitmapData BD = bitmap.LockBits(new Rectangle(0, 0, w, h), 
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (byte* a = imgPixels[0], r = imgPixels[1],
                    g = imgPixels[2], b = imgPixels[3])
                {
                    byte* _a = a, _r = r, _g = g, _b = b;

                    for (int i = 0; i < h; i++) // висота зображення
                    {
                        curpos = ((byte*)BD.Scan0) + i * BD.Stride;

                        for (int j = 0; j < w; j++)  // ширина зображення
                        {
                            *_b = *(curpos++);  _b += h;    // компонента синня
                            *_g = *(curpos++);  _g += h;    // компонента зелена
                            *_r = *(curpos++);  _r += h;    // компонента червона
                            *_a = *(curpos++);  _a += h;    // компонента прозорості
                        }

                        {
                            int d = w * h - 1;
                            _b -= d; _g -= d; _r -= d; _a -= d;
                        }
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(BD);
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");

            //imgPixels2 = imgPixels;
        }

        /// <summary>
        /// Отримання кольорів пікселів, з використанням вказівників
        /// </summary>
        /// <param name="bitmap">Зображення із якого отримуватиемться RGB кольори</param>
        internal unsafe void GetInfoFromImage2(Bitmap bitmap, PointOb oblast)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            Obrezka.Oblast = oblast;

            int w = Obrezka.Width,
                h = Obrezka.Height;

            // Створюємо/перезаписуємо екземпляр для даних кольорів
            imgPixels2 = new byte[4][,];
            for (int i = 0; i < 4; i++)
                imgPixels2[i] = new byte[w, h];

            BitmapData BD = bitmap.LockBits(new Rectangle(Obrezka.Oblast.L, Obrezka.Oblast.U, 
                w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (byte* a = imgPixels2[0], r = imgPixels2[1],
                    g = imgPixels2[2], b = imgPixels2[3])
                {
                    byte* _a = a, _r = r, _g = g, _b = b;

                    for (int i = 0; i < h; i++) // висота зображення
                    {
                        curpos = ((byte*)BD.Scan0) + i * BD.Stride;

                        for (int j = 0; j < w; j++)  // ширина зображення
                        {
                            *_b = *(curpos++); _b += h;    // компонента синня
                            *_g = *(curpos++); _g += h;    // компонента зелена
                            *_r = *(curpos++); _r += h;    // компонента червона
                            *_a = *(curpos++); _a += h;    // компонента прозорості
                        }

                        {
                            int d = w * h - 1;
                            _b -= d; _g -= d; _r -= d; _a -= d;
                        }
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(BD);
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");

            //imgPixels2 = imgPixels;
        }

        /// <summary>
        ///  Розрахунок значення масиву точок амплітуд сигналів
        /// </summary>
        /// <param name="markers">масив координат маркерів</param>
        /// <param name="bitmap">картинка яка аналізується</param>
        /// <returns></returns>
        internal unsafe float[] GetPixelFromImage(Dictionary<int, Point> markers, Bitmap bitmap)
        {
            // кількість елементів в списку
            int num = markers.Count;

            // розміри зображення
            int w = bitmap.Width,
                h = bitmap.Height;

            // амплітуди сигналів
            float[] amp = new float[num];

            BitmapData BD = bitmap.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (float* a = amp)
                {
                    float* _a = a;
                    #region
                    /*
                    for (int i = 0; i < num; i++)
                    {
                        curpos = ((byte*)BD.Scan0 + markers[i].Y * BD.Stride) + 
                            markers[i].X * 4;

                        *_a = *curpos / 255f; _a++;
                    }
                    */
                    #endregion
                    foreach (KeyValuePair<int, Point> i in markers)
                    {
                        curpos = (byte*)BD.Scan0 + i.Value.Y * BD.Stride +
                            i.Value.X * 4;

                        *_a = *curpos / 255f; _a++;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(BD);
            }

            return amp;
        }

        /// <summary>
        /// Розрахунок значення масиву точок лінії амплітуд сигналів
        /// </summary>
        /// <param name="line">координати лінії</param>
        /// <param name="bitmap">картинка яка аналізується</param>
        /// <returns></returns>
        internal unsafe float[,] GetPixelFromLine(PointSort line, Bitmap bitmap)
        {
            // кількість елементів в списку
            int num = line.PointN;

            // розміри зображення
            int w = bitmap.Width,
                h = bitmap.Height;

            // амплітуди сигналів
            float[,] amp = new float[2, num];

            // знаходимо відстані які займає по двум осям лінія
            float dLX = line.SecondL.X - line.FirstL.X,
                dLY = line.SecondL.Y - line.FirstL.Y;

            // знаходимо кроки дискретизації лінії
            float dX = dLX / (num - 1),
                dY = dLY / (num - 1);

            // тимчасова координата
            Point tempPoint = line.FirstL;

            BitmapData BD = bitmap.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (float* _amp = amp)
                {
                    float* _num = _amp, 
                        _val = _amp + num;

                    for (int i = 0; i < num; i++)
                    {
                        // задаємо точку в якій визначатимемо амплітуду сигналу
                        tempPoint = new Point(
                            line.FirstL.X + (int)Math.Round(i * dX, MidpointRounding.AwayFromZero), 
                            line.FirstL.Y + (int)Math.Round(i * dY, MidpointRounding.AwayFromZero));

                        curpos = ((byte*)BD.Scan0) + tempPoint.Y * BD.Stride +
                            tempPoint.X * 4;

                        *_num = i;  _num++;
                        *_val = *curpos / 255f; _val++;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(BD);
            }

            return amp;
        }

        #endregion

        #region Збереження даних в зображенні

        /// <summary>
        /// Створює сіре зображення із RGB матриці, в залежності від вибраної кольорової компоненти
        /// </summary>
        /// <param name="mc">Кольорова компонента для сірого зображення</param>
        internal unsafe void SetInfoFromImage(MyColor mc, PointOb oblast)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            Obrezka.Oblast = oblast;

            // Створення зображення яке відображатиметься після корекції
            imageCorect = new Bitmap(imageSave, Obrezka.Width, Obrezka.Height);

            int w = imageCorect.Width,
                h = imageCorect.Height;

            //BitmapData BD = imageCorect.LockBits(new Rectangle(0, 0, w, h), 
            //    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData BD = imageCorect.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (byte* a = imgPixels2[0], r = imgPixels2[1],
                    g = imgPixels2[2], b = imgPixels2[3])
                {
                    byte* _a = a, _r = r,  _g = g, _b = b;
                    byte _t = 0;
                    byte[] _tm;

                    for (int i = 0; i < h; i++) // висота зображення
                    {
                        curpos = ((byte*)BD.Scan0) + i * BD.Stride;

                        for (int j = 0; j < w; j++)  // ширина зображення
                        {
                            // створення списка із 3-х компонент
                            _tm = new byte[3] { *_r, *_g, *_b };

                            switch (mc)
                            {
                                #region Визначення необхідної компоненти і занесення її в зображення
                                case MyColor.red:
                                    _t = _tm[0];
                                    goto default;
                                case MyColor.green:
                                    _t = _tm[1];
                                    goto default;
                                case MyColor.blue:
                                    _t = _tm[2];
                                    goto default;
                                case MyColor.cyan:
                                    _t = (byte)(255 - _tm[0]);
                                    goto default;
                                case MyColor.magenta:
                                    _t = (byte)(255 - _tm[1]);
                                    goto default;
                                case MyColor.yellow:
                                    _t = (byte)(255 - _tm[2]);
                                    goto default;
                                case MyColor.black:
                                    _t = (byte)(255 - _tm.Min());
                                    goto default;
                                case MyColor.min:
                                    _t = _tm.Min();
                                    goto default;
                                case MyColor.med:
                                    _tm.OrderBy(t => t);
                                    _t = _tm[1];
                                    goto default;
                                case MyColor.max:
                                    _t = _tm.Max();
                                    goto default;
                                case MyColor.hm:
                                    if (_tm[0] * _tm[1] * _tm[2] != 0)
                                        _t = (byte)(3 * Math.Round((float)((_tm[0] * _tm[1] * _tm[2]) /
                                            (_tm[1] * _tm[2] + _tm[0] * _tm[2] + _tm[0] * _tm[1])),
                                            MidpointRounding.AwayFromZero));
                                    else
                                        _t = 0;
                                    goto default;
                                case MyColor.gm:
                                    _t = (byte)Math.Round(Math.Pow(_tm[0] * _tm[1] * _tm[2],
                                        1.0 / 3.0), MidpointRounding.AwayFromZero);
                                    goto default;
                                case MyColor.am:
                                    _t = (byte)Math.Round((_tm[0] + _tm[1] + _tm[2]) / 3.0, 
                                        MidpointRounding.AwayFromZero);
                                    goto default;
                                case MyColor.rms:
                                    _t = (byte)Math.Round(Math.Sqrt((_tm[0] * _tm[0] +
                                        _tm[1] * _tm[1] + _tm[2] * _tm[2]) / 3.0),
                                        MidpointRounding.AwayFromZero);
                                    goto default;
                                case MyColor.cm:
                                    _t = (byte)Math.Round(Math.Pow((_tm[0] * _tm[0] * _tm[0] +
                                        _tm[1] * _tm[1] * _tm[1] + _tm[2] * _tm[2] * _tm[2]) / 3.0,
                                        1.0 / 3.0), MidpointRounding.AwayFromZero);
                                    goto default;
                                default:
                                    *(curpos++) = _t;
                                    *(curpos++) = _t;
                                    *(curpos++) = _t;
                                    *(curpos++) = *_a;
                                    break;
                                    #endregion
                            }
                            _b += h; _g += h; _r += h; _a += h;
                        }

                        {
                            int d = w * h - 1;
                            _b -= d; _g -= d; _r -= d; _a -= d;
                        }
                    }
                }
            }
            finally
            {
                imageCorect.UnlockBits(BD);
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");
        }

        /// <summary>
        /// Корегує зображення у відповідності до вибраної області
        /// </summary>
        /// <param name="oblast"></param>
        internal unsafe void SetInfoFromImage(PointOb oblast)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();
            Obrezka.Oblast = oblast;

            // Створення зображення яке відображатиметься після корекції
            imageCut = new Bitmap(imageSave);

            int w = imageCut.Width,
                h = imageCut.Height;

            //BitmapData BD = imageCut.LockBits(new Rectangle(0, 0, w, h),
            //    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData BD = imageCut.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;

                fixed (byte* a = imgPixels[0], r = imgPixels[1],
                    g = imgPixels[2], b = imgPixels[3])
                {
                    byte* _a = a, _r = r, _g = g, _b = b;

                    for (int i = 0; i < h; i++) // висота зображення
                    {
                        curpos = ((byte*)BD.Scan0) + i * BD.Stride;

                        for (int j = 0; j < w; j++)  // ширина зображення
                        {
                            *(curpos++) = *_b;
                            *(curpos++) = *_g;
                            *(curpos++) = *_r;

                            #region
                            if ((i >= Obrezka.Oblast.U) &&
                                (i <= Obrezka.Oblast.D) &&
                                (j >= Obrezka.Oblast.L) &&
                                (j <= Obrezka.Oblast.R))
                            {
                                *(curpos++) = *_a;
                            }
                            else
                            {
                                *(curpos++) = (byte)Math.Round(*_a / 2.0,
                                    MidpointRounding.AwayFromZero);
                            }
                            #endregion

                            _b += h; _g += h; _r += h; _a += h;
                        }

                        {
                            int d = w * h - 1;
                            _b -= d; _g -= d; _r -= d; _a -= d;
                        }
                    }
                }
            }
            finally
            {
                imageCut.UnlockBits(BD);
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");
        }

        /// <summary>
        /// Виділяє лише необхідну компоненту кольору
        /// </summary>
        /// <param name="mc">Кольорова компонента для сірого зображення</param>
        internal void SetColorImg(MyColor mc)
        {
            //// Засікаємо час
            //times = new Stopwatch();
            //times.Restart();

            // Створення зображення яке відображатиметься після корекції
            imageCorect = new Bitmap(imageSave);

            int w = SizeImage.Width,
                h = SizeImage.Height;

            // фон зображення, компонет які закрашуються
            List<int> mList = new List<int>(3);
            int board = 0,// = default(int),
                alpha = 255;

            // Присвоюємо нові дані для зображення
            for (int i = 0; i < h; i++) // ширина зображення
            {
                for (int j = 0; j < w; j++)  // висота зображення
                {
                    // створення списка із 3-х компонент
                    for (int k = 0; k < 3; k++)
                        mList.Add(imgPixels[k + 1][j, i]);

                    switch (mc)
                    {
                        #region Визначення необхідної компоненти і занесення її в зображення
                        case MyColor.red:
                            board = mList[0];
                            goto default;
                        case MyColor.green:
                            board = mList[1];
                            goto default;
                        case MyColor.blue:
                            board = mList[2];
                            goto default;
                        case MyColor.cyan:
                            board = 255 - mList[0];
                            goto default;
                        case MyColor.magenta:
                            board = 255 - mList[1];
                            goto default;
                        case MyColor.yellow:
                            board = 255 - mList[2];
                            goto default;
                        case MyColor.black:
                            board = 255 - mList.Min();
                            goto default;
                        case MyColor.min:
                            board = mList.Min();
                            goto default;
                        case MyColor.med:
                            mList.Sort();
                            board = mList[1];
                            goto default;
                        case MyColor.max:
                            board = mList.Max();
                            goto default;
                        case MyColor.hm:
                            if (mList[0] * mList[1] * mList[2] != 0)
                                board = 3 * (int)Math.Round((float)((mList[0] * mList[1] * mList[2]) / 
                                    (mList[1] * mList[2] + mList[0] * mList[2] + mList[0] * mList[1])),
                                    MidpointRounding.AwayFromZero);
                            else
                                board = 0;
                            goto default;
                        case MyColor.gm:
                            board = (int)Math.Round(Math.Pow(mList[0] * mList[1] * mList[2], 
                                1.0 / 3.0), MidpointRounding.AwayFromZero);
                            goto default;
                        case MyColor.am:
                            board = (int)Math.Round(mList.Sum() / 3.0, MidpointRounding.AwayFromZero);
                            goto default;
                        case MyColor.rms:
                            board = (int)Math.Round(Math.Sqrt((mList[0] * mList[0] +
                                mList[1] * mList[1] + mList[2] * mList[2]) / 3.0), 
                                MidpointRounding.AwayFromZero);
                            goto default;
                        case MyColor.cm:
                            board = (int)Math.Round(Math.Pow((mList[0] * mList[0] * mList[0] +
                                mList[1] * mList[1] * mList[1] + mList[2] * mList[2] * mList[2]) / 3.0,
                                1.0 / 3.0), MidpointRounding.AwayFromZero);
                            goto default;
                        default:
                            if (board > 255) board = 255;
                            else if (board < 0) board = 0;
                            imageCorect.SetPixel(j, i, Color.FromArgb(alpha, board, board, board));
                            break;
                            #endregion
                    }

                    // очистка списка
                    mList.Clear();
                }
            }

            //// Зупиняємо час
            //times.Stop();
            //MessageBox.Show("Базовий метод: " + times.ElapsedMilliseconds.ToString() + " мс");
        }

        #endregion

        /// <summary>
        /// Копіювання масиву данних
        /// </summary>
        internal void CloneColor()
        {
            imgPixels2 = imgPixels;
            //imageCut = imageSave;
        }

    }
}
