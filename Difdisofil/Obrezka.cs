using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Difdisofil
{
    /// <summary>
    /// Тип для точок виділеної області
    /// </summary>
    internal struct PointOb
    {
        /// <summary>
        /// Ліва крайня точка області
        /// </summary>
        private int l;
        /// <summary>
        /// Права крайня точка області
        /// </summary>
        private int r;
        /// <summary>
        /// Верхня крайня точка області
        /// </summary>
        private int u;
        /// <summary>
        /// Нижня крайня точка області
        /// </summary>
        private int d;
        /// <summary>
        /// Ліва верхня крайня точка області
        /// </summary>
        private Point lu;
        /// <summary>
        /// Права нижня крайня точка області
        /// </summary>
        private Point rd;

        /// <summary>
        /// Ліва крайня точка області
        /// </summary>
        internal int L
        {
            get { return l; }
            set { lu.X = l = value; }
        }
        /// <summary>
        /// Права крайня точка області
        /// </summary>
        internal int R
        {
            get { return r; }
            set { rd.X = r = value; }
        }
        /// <summary>
        /// Верхня крайня точка області
        /// </summary>
        internal int U
        {
            get { return u; }
            set { lu.Y = u = value; }
        }
        /// <summary>
        /// Нижня крайня точка області
        /// </summary>
        internal int D
        {
            get { return d; }
            set { rd.Y = d = value; }
        }

        /// <summary>
        /// Ліва верхня крайня точка області
        /// </summary>
        internal Point LU
        {
            get { return lu; }
            set
            {
                lu = value;
                l = value.X;
                u = value.Y;
            }
        }

        /// <summary>
        /// Права нижня крайня точка області
        /// </summary>
        internal Point RD
        {
            get { return rd; }
            set
            {
                rd = value;
                r = value.X;
                d = value.Y;
            }
        }
    }

    /// <summary>
    /// Дані про розміщення обрізаної області
    /// </summary>
    internal static class Obrezka
    {
        /// <summary>
        /// Координати виділеної області
        /// </summary>
        private static PointOb oblast;

        /// <summary>
        /// Виведення ширини області
        /// </summary>
        internal static int Width { get; private set; }
        /// <summary>
        /// Виведення висоти області
        /// </summary>
        internal static int Height { get; private set; }

        /// <summary>
        /// Координати виділеної області
        /// </summary>
        internal static PointOb Oblast
        {
            get { return oblast; }
            set
            {
                oblast.L = Math.Min(value.L, value.R);
                oblast.R = Math.Max(value.L, value.R);
                oblast.U = Math.Min(value.U, value.D);
                oblast.D = Math.Max(value.U, value.D);
                Width = Math.Abs(value.R - value.L);
                Height = Math.Abs(value.D - value.U);
            }
        }

        /// <summary>
        /// Розрахунок області по зображенню
        /// </summary>
        /// <param name="bitmap"></param>
        internal static void CalcOblast(Bitmap bitmap)
        {
            // знаходимо ширину і висоту
            Width = bitmap.Width;
            Height = bitmap.Height;

            // знаходимо і розраховуємо точки
            oblast.L = oblast.U = 0;
            oblast.R = Width;
            oblast.D = Height;
        }

    }

    /// <summary>
    /// Тип для ліній (отсортировані дві точки)
    /// </summary>
    internal struct PointSort
    {
        /// <summary>
        /// Список із 2-х точок, які потім сортуватимуться
        /// </summary>
        private List<Point> pointList;

        /// <summary>
        /// Перша точка (без позначення нумерації лінії)
        /// </summary>
        internal Point FirstP { get; private set; }
        /// <summary>
        /// Друга точка (біля якої позначатимуться номер лінії)
        /// </summary>
        internal Point SecondP { get; private set; }
        /// <summary>
        /// Перша точка для графіка (зліва)
        /// </summary>
        internal Point FirstL { get; private set; }
        /// <summary>
        /// Друга точка для графіку (справа)
        /// </summary>
        internal Point SecondL { get; private set; }

        /// <summary>
        /// Кількість точок (пікселів) в прямій
        /// </summary>
        internal int PointN { get; private set; }

        /// <summary>
        /// Почергове додавання точки, для її аналізу і сортування
        /// </summary>
        /// <param name="point">одна точка</param>
        internal void Add(Point point)
        {
            if ((pointList == null) || 
                (pointList.Count == 0))
            {
                // ініціалізуємо змінну і додаємо першу точку
                pointList = new List<Point>(2) { point };
            }
            else if (pointList.Count == 1)
            {
                // додаємо другу точку
                pointList.Add(point);
                // сотруємо і вносимо дані для їх зчитування
                Sort(pointList);
            }
        }

        /// <summary>
        /// Довадання 2-х точок зразу, для їх аналізу і сортування
        /// </summary>
        /// <param name="points">дві точки</param>
        internal void Add(Point[] points)
        {
            // очищення списку
            if (pointList != null)
                pointList.Clear();
            // додавання точок і їх аналіз
            Add(points[0]);
            Add(points[1]);
        }

        /// <summary>
        /// Сортування точок
        /// </summary>
        /// <param name="pointList"></param>
        private void Sort(List<Point> pointList)
        {
            // не допускаємо двох однакових точок
            if (pointList[0].Equals(pointList[1]))
            {
                pointList.Clear();
                return;
            }
            else if (pointList[0].X == pointList[1].X)   // коли лінія - вертикальна
            {
                // якщо p[0] більше p[1], то вона буде другою точкою
                if (pointList[0].Y > pointList[1].Y)
                {
                    SecondP = SecondL = pointList[1];
                    FirstP = FirstL = pointList[0];
                }
                else
                {
                    FirstP = FirstL = pointList[1];
                    SecondP = SecondL = pointList[0];
                }
            }
            else if (pointList[0].Y == pointList[1].Y)  // коли лінія - горизонтальна
            {
                // якщо p[0] більше p[1], то вона буде другою точкою
                if (pointList[0].X > pointList[1].X)
                {
                    SecondP = FirstL = pointList[1];
                    FirstP = SecondL = pointList[0];
                }
                else
                {
                    FirstP = SecondL = pointList[1];
                    SecondP = FirstL = pointList[0];
                }
            }
            else
            {
                // аналізуємо нахил прямої (аналіз по вертикалі)
                if (pointList[0].Y > pointList[1].Y)
                {
                    FirstP =  pointList[1];
                    SecondP = pointList[0];
                }
                else
                {
                    SecondP = pointList[1];
                    FirstP = pointList[0];
                }
                // аналізуємо нахил прямої (аналіз по горизонталі)
                if (pointList[0].X > pointList[1].X)
                {
                    FirstL = pointList[1];
                    SecondL = pointList[0];
                }
                else
                {
                    SecondL = pointList[1];
                    FirstL = pointList[0];
                }
            }

            // чистимо змінну
            pointList.Clear();

            // визначаємо кількість точок по яких будується лінія
            PointN = Math.Max(Math.Abs(FirstP.X - SecondP.X),
                Math.Abs(FirstP.Y - SecondP.Y)) + 1;
        }
    }
}
