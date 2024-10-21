using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crosswords
{
    public partial class Form1 : Form
    {
        CrosswordTable crossword;
        //для перехода между состояниями приложения
        enum State
        {
            Main, //главное, где мы задаем шаблон
            Result //смотрим на найденное решение, в нем нельзя изменять шаблон
        }

        State formState = State.Main;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pathBox.Text = GetWordListPath();
            labelInfo.Text = "Нажимайте на клетки, чтобы добавить в шаблон. Минимальная длина слов - 3. При готовности нажмите на кнопку \"Заполнить\"";
            InitializeBoard();
            crossword = new CrosswordTable(board.ColumnCount, board.RowCount);
        }

        private string GetWordListPath()
        {
            // Get the base directory where your application is running
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Define the relative path to your file
            string relativeFilePath = @"..\..\mit.edu_~ecprice_wordlist.10000.txt"; // Adjust this path as needed

            // Combine the base directory path with the relative path to create the full file path
            string fullFilePath = Path.Combine(baseDirectory, relativeFilePath);

            // Now you can use 'fullFilePath' to access the file
            if (File.Exists(fullFilePath))
            {
                return fullFilePath;
                // Perform file operations here
            }
            else
            {
                return "";
            }
        }

        //первичная настройка таблицы
        private void InitializeBoard()
        {
            board.BackgroundColor = Color.Black;
            board.DefaultCellStyle.BackColor = Color.Black;

            for (int i = 0; i < board.ColumnCount; i++)
            {
                board.Rows.Add();
            }

            foreach (DataGridViewRow row in board.Rows)
            {
                row.Height = board.Height / board.Rows.Count;
            }

            foreach(DataGridViewColumn column in board.Columns)
            {
                column.Width = board.Width / board.Columns.Count;
            }

            for (int i = 0; i < board.RowCount; i++)
            {
                for (int j = 0; j < board.ColumnCount; j++)
                {
                    board[i, j].ReadOnly = true;
                }
            }
        }

        private void FormatCall(int i, int j)
        {
            DataGridViewCell c = board[i, j];
            c.Style.BackColor = Color.White;
            c.Style.SelectionBackColor = Color.Wheat;
        }

        private void board_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (formState != State.Main)
            {
                return;
            }
            DataGridViewCell c = board[e.ColumnIndex, e.RowIndex];

            if(crossword.ChangePattern(c.ColumnIndex, c.RowIndex))
            {
                c.Style.BackColor = Color.White;
            }
            else
            {
                c.Style.BackColor = Color.Black;
            }

            // Приостановить рендеринг ячеек
            board.SuspendLayout();

            // Завершить рендеринг ячеек
            board.ResumeLayout();
        }

        //попытаться найти решение заданному шаблону
        private void tryButton_Click(object sender, EventArgs e)
        {
            if (formState == State.Result)
            {
                return;
            }
            if (crossword.CheckValidity())
            {
                crossword.FindWordPlaces();
                foreach (var word in crossword.segments)
                {
                    if(word.dir == Directions.Across)
                    {
                        for(int i = word.startCellX; i < word.startCellX + word.length; i++)
                        {
                            board[i, word.startCellY].Style.BackColor = Color.Green;
                        }
                    }
                    if(word.dir == Directions.Down)
                    {
                        for (int i = word.startCellY; i < word.startCellY + word.length; i++)
                        {
                            board[word.startCellX, i].Style.BackColor = Color.Green;
                        }
                    }
                }
                board.Invalidate();
                board.Update();

                string[] wordlist = GetWordList();
                if (crossword.SolveCrossword(wordlist))
                {
                    MessageBox.Show("Решение найдено.");
                    SyncCrosswordBoard();
                    formState = State.Result;
                    labelInfo.Text = "Нажмите на кнопку \"Очистить\", чтобы создать шаблон заново.";
                }
                else
                {
                    MessageBox.Show("Не можем найти решение вашему шаблону");
                    foreach (var word in crossword.segments)
                    {
                        if (word.dir == Directions.Across)
                        {
                            for (int i = word.startCellX; i < word.startCellX + word.length; i++)
                            {
                                board[i, word.startCellY].Style.BackColor = Color.White;
                            }
                        }
                        if (word.dir == Directions.Down)
                        {
                            for (int i = word.startCellY; i < word.startCellY + word.length; i++)
                            {
                                board[word.startCellX, i].Style.BackColor = Color.White;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("incorrect");
            }
        }

        //получить массив слов через путь файла, указанный в pathBox
        private string[] GetWordList()
        {
            // Replace 'your_file_path' with the actual path to your file.
            string filePath = pathBox.Text;

            List<string> filteredWords = new List<string>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Trim the line to remove leading and trailing spaces.
                        line = line.Trim();

                        if (line.Length > 2)
                        {
                            filteredWords.Add(line);
                        }
                    }
                }

                // Convert the List<string> to a string[].
                string[] resultArray = filteredWords.ToArray();

                // Print the filtered words or use them as needed.
                foreach (string word in resultArray)
                {
                    Console.WriteLine(word);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("something wrong with the file");
            }
            return filteredWords.ToArray();
        }

        private void SyncCrosswordBoard()
        {
            for (int i = 0; i < board.ColumnCount; i++)
            {
                for (int j = 0; j < board.RowCount; j++)
                {
                    char c = crossword.GetContent(i, j);
                    if (c != '\0')
                    {
                        board[i, j].Value = c;
                        board[i, j].Style.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        //очистить шаблон и таблицу
        private void clearButton_Click(object sender, EventArgs e)
        {
            formState = State.Main;
            for (int i = 0; i < board.ColumnCount; i++)
            {
                for (int j = 0; j < board.RowCount; j++)
                {
                    board[i, j].Style.BackColor = Color.Black;
                    board[i, j].Value = "";
                }
            }
            crossword = new CrosswordTable(board.ColumnCount, board.RowCount);
            board.Invalidate();
            board.Update();
            labelInfo.Text = "Нажимайте на клетки, чтобы добавить в шаблон. Минимальная длина слов - 3. При готовности нажмите на кнопку \"Заполнить\"";
        }
    }
}
