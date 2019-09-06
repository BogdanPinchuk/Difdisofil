using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Difdisofil
{
    /// <summary>
    /// Точка маркера
    /// </summary>
    internal class MarkerClass
    {
        /// <summary>
        /// Розмір маркера
        /// </summary>
        private static readonly float dimM = 3;
        /// <summary>
        /// Відстань рексту від маркеру
        /// </summary>
        private readonly int dimTM = 2;
        /// <summary>
        /// Розмір тексту
        /// </summary>
        private static readonly float dimF = 10f;

        /// <summary>
        /// Координати маркера
        /// </summary>
        internal Point Marker { get; set; }
        /// <summary>
        /// Координати списку маркерів
        /// </summary>
        internal List<Point> MarkerList { get; set; } 
            = new List<Point>(2);
        /// <summary>
        /// Координати словника маркерів
        /// </summary>
        internal Dictionary<int, Point> MarkerDict { get; set; } 
            = new Dictionary<int, Point>();
        /// <summary>
        /// Координати словника ліній
        /// </summary>
        internal Dictionary<int, PointSort> LineDict { get; set; } 
            = new Dictionary<int, PointSort>();
        /// <summary>
        /// Визначає чи вибрана дана лінія для відображення на графіку
        /// </summary>
        internal Dictionary<int, bool> LineBool { get; set; }
            = new Dictionary<int, bool>();

        /// <summary>
        /// Корегуємо вибрану лінію для відображення на графіку
        /// </summary>
        /// <param name="key">Ключ за яким міняється значення</param>
        /// <param name="value">Необхідне значення</param>
        internal void CorrectLine(int key, bool value)
        {
            foreach (int i in LineDict.Keys)
            {
                if (i != key)
                    LineBool[i] = false;
                else
                    LineBool[i] = value;
            }
        }

        /// <summary>
        /// Зміна властивостей маркера
        /// </summary>
        internal Pen Col_Think { get; set; } = new Pen(Color.Gold, dimM);
        /// <summary>
        /// Довжина лінії маркера
        /// </summary>
        internal int Dim_mark { get; set; } = 7;
        /// <summary>
        /// Наявність маркера і дозвіл його відображення
        /// </summary>
        internal bool Dozvil { get; set; } = false;
        /// <summary>
        /// Вирівнювання тексту
        /// </summary>
        internal StringFormat StrFormat { get; set; } = new StringFormat()
        {
            Alignment = StringAlignment.Near
        };
        /// <summary>
        /// Колір тексту
        /// </summary>
        internal SolidBrush StrBrush { get; set; } = new SolidBrush(Color.Gold);
        /// <summary>
        /// Шрифт тексту
        /// </summary>
        internal Font StrFont { get; set; } = new Font("Arial", dimF, FontStyle.Regular);

        #region Малювання маркерів

        /// <summary>
        /// Малювання маркера на формі
        /// </summary>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawMark(Graphics g)
        {
            // Точки частин маркера "+"
            Point u = new Point(Marker.X, Marker.Y - Dim_mark),
                d = new Point(Marker.X, Marker.Y + Dim_mark + 1),
                l = new Point(Marker.X - Dim_mark, Marker.Y),
                r = new Point(Marker.X + Dim_mark + 1, Marker.Y);

            // Малюємо маркер
            g.DrawLine(Col_Think, u, d);
            g.DrawLine(Col_Think, l, r);
        }

        /// <summary>
        /// З'єднуємо два маркери лінією
        /// </summary>
        /// <param name="ps">Координати двох ліній</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawLine(PointSort ps, Graphics g)
        {
            // малюємо лінію
            g.DrawLine(Col_Think, ps.FirstP, ps.SecondP);
        }

        /// <summary>
        /// Малювання маркера на формі
        /// </summary>
        /// <param name="marker">Координати маркера</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawMark(Point m, Graphics g)
        {
            // Заносимо дані розташування маркера
            Marker = m;
            // Малюємо маркер
            DrawMark(g);
        }

        /// <summary>
        /// Відображення всіх маркерів в списку
        /// </summary>
        /// <param name="markList">Список маркерів</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawMark(List<Point> markList, Graphics g)
        {
            // перебираємо список і малюємо кожен маркер
            foreach (Point i in markList)
            {
                // малюємо маркер
                DrawMark(i, g);
            }
        }

        /// <summary>
        /// Відображення всіх маркерів в словнику
        /// </summary>
        /// <param name="markDic">Словник маркерів</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawMark(Dictionary<int, Point> markDic, Graphics g)
        {
            // перебираємо словник і малюємо кожен маркер
            foreach (KeyValuePair<int, Point> i in markDic)
            {
                // малюємо маркер
                DrawMark(i.Value, g);
                // підписуємо його номер
                DrawStr(i, g);
            }
        }

        /// <summary>
        /// Відображення всіх ліній в словнику
        /// </summary>
        /// <param name="markDic">Словник ліній</param>
        /// <param name="g">Графічна область елемента на якому малюємо</param>
        internal void DrawMark(Dictionary<int, PointSort> markDic, Graphics g)
        {
            // перебираємо словник і малюємо кожен маркер
            foreach (KeyValuePair<int, PointSort> i in markDic)
            {
                // малюємо лінію
                DrawLine(i.Value, g);
                // малюємо перший і другий маркери
                DrawMark(i.Value.FirstP, g);
                DrawMark(i.Value.SecondP, g);
                // підписуємо номер лінії біля другого маркера
                KeyValuePair<int, Point> m =
                    new KeyValuePair<int, Point>(i.Key, i.Value.SecondP);
                DrawStr(m, g);
            }
        }

        /// <summary>
        /// Підпис маркерів (нумерацією)
        /// </summary>
        /// <param name="g"></param>
        private void DrawStr(KeyValuePair<int, Point> m, Graphics g)
        {
            // підписуємо відповідний маркер
            g.DrawString(m.Key.ToString(), StrFont, StrBrush,
                new Point(m.Value.X + dimTM, m.Value.Y + dimTM), StrFormat);
        }

        #endregion

    }
}
