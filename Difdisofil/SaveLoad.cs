using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Prop = Difdisofil.Properties.Settings;

namespace Difdisofil
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// Збереження або загрузка налаштувань програми
        /// </summary>
        private enum SLS
        {
            /// <summary>
            /// збереження налаштувань
            /// </summary>
            save,
            /// <summary>
            /// завантаження налаштувань
            /// </summary>
            load
        }

        /// <summary>
        /// Який саме елемент програми зберігати
        /// </summary>
        private enum EF
        {
            /// <summary>
            /// настройки всіх налаштуваня
            /// </summary>
            all,
            /// <summary>
            /// настройки для форми
            /// </summary>
            FormMain,
            /// <summary>
            /// настройки для вкладки піксель
            /// </summary>
            splitContainerPixel,
            /// <summary>
            /// настройки для вкладки лінія
            /// </summary>
            splitContainerLine,
            /// <summary>
            /// настройки для вкладки графік і таблиця
            /// </summary>
            splitContainerGraph
        }

        /// <summary>
        /// Дії які виконуються при завантаженні форми
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            // загрузка налаштувань
            Prop.Default.Reload();
            SaveLoadSettings(SLS.load, EF.all);

            // корекція ширини спліт-контейнерів
            {
                int dim = 8;
                splitContainerPixel.SplitterWidth = dim;
                splitContainerLine.SplitterWidth = dim;
                splitContainerGraph.SplitterWidth = dim;
            }

            // Очищаємо статусний рядок
            ClearStatusDown();

            // Очищаємо всі вкладки
            ClearAlltoolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// Дії які виконуються при закритті форми
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            // збереження налаштувань
            SaveLoadSettings(SLS.save, EF.all);
            Prop.Default.Save();
            Prop.Default.Upgrade();
        }

        /// <summary>
        /// Загрузна налаштувань програми
        /// </summary>
        /// <param name="sls">Параметр за яким програма вибирає зберігати чи завантажити налаштування</param>
        private void SaveLoadSettings(SLS sls, EF ef)
        {
            switch (sls)
            {
                case SLS.save:
                    switch (ef)
                    {
                        case EF.all:
                            Allsave();
                            break;
                        case EF.FormMain:
                            FMsave();
                            break;
                        case EF.splitContainerPixel:
                            SCPsave();
                            break;
                        case EF.splitContainerLine:
                            SCLsave();
                            break;
                        case EF.splitContainerGraph:
                            SCGsave();
                            break;
                    }
                    break;
                case SLS.load:
                    switch (ef)
                    {
                        case EF.all:
                            Allload();
                            break;
                        case EF.FormMain:
                            FMload();
                            break;
                        case EF.splitContainerPixel:
                            SCPload();
                            break;
                        case EF.splitContainerLine:
                            SCLload();
                            break;
                        case EF.splitContainerGraph:
                            SCGload();
                            break;
                    }
                    break;
            }

            // All
            void Allsave()
            {
                FMsave();
                SCPsave();
                SCLsave();
                SCGsave();
            }
            void Allload()
            {
                FMload();
                SCPload();
                SCLload();
                SCGload();
            }

            // FormMain
            void FMsave()
            {
                Prop.Default.FMl = this.Location;
                Prop.Default.FMs = this.Size;
            }
            void FMload()
            {
                this.Location = Prop.Default.FMl;
                this.Size = Prop.Default.FMs;
            }

            // splitContainerPixel
            void SCPsave()
            {
                if (splitPixel)
                {
                    Prop.Default.sCPo = splitContainerPixel.Orientation;
                    Prop.Default.sCPsd = splitContainerPixel.SplitterDistance;
                    splitPixel = false;
                }
            }
            void SCPload()
            {
                splitContainerPixel.Orientation = Prop.Default.sCPo;
                splitContainerPixel.SplitterDistance = Prop.Default.sCPsd;
            }

            // splitContainerLine
            void SCLsave()
            {
                if (splitLine)
                {
                    Prop.Default.sCLo = splitContainerLine.Orientation;
                    Prop.Default.sCLsd = splitContainerLine.SplitterDistance;
                    splitLine = false;
                }
            }
            void SCLload()
            {
                splitContainerLine.Orientation = Prop.Default.sCLo;
                splitContainerLine.SplitterDistance = Prop.Default.sCLsd;
            }

            // splitContainerGraph
            void SCGsave()
            {
                if (splitGraph)
                {
                    Prop.Default.sCGo = splitContainerGraph.Orientation;
                    Prop.Default.sCGsd = splitContainerGraph.SplitterDistance;
                    splitGraph = false;
                }
            }
            void SCGload()
            {
                splitContainerGraph.Orientation = Prop.Default.sCGo;
                splitContainerGraph.SplitterDistance = Prop.Default.sCGsd;
            }
        }
    }
}
