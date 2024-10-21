using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;

namespace Crosswords
{
    public class CrosswordTable
    {
        private bool[,] inPattern;
        private char[,] charContainer;
        private int n, m; //n - columns, m - rows
        public List<CrosswordSegment> segments;

        public CrosswordTable(int n, int m)
        {
            this.n = n;
            this.m = m;
            inPattern = new bool[n, m];
            charContainer = new char[n, m];
        }

        public bool ChangePattern(int i, int j)
        {
            //выводы:
            //true - данная клетка теперь в шаблоне
            //false - данная клетка убрана из шаблона
            if(inPattern[i, j])
            {
                inPattern[i, j] = false;
                return false;
            }
            else
            {
                inPattern[i, j] = true;
                return true;
            }
        }

        //проверка на структурную правильность задания шаблона
        public bool CheckValidity()
        {
            //проверка на то, что хотя бы одна клетка есть в шаблоне, и проверка размера
            bool madePattern = false;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (inPattern[i, j])
                    {
                        madePattern = true;
                        break;
                    }
                }
                if (madePattern)
                    break;
            }

            if (!madePattern)
            {
                return false;
            }

            //проверка на наличие групп клеток 2х2 в шаблоне
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < m - 1; j++)
                {
                    if (inPattern[i, j] && inPattern[i + 1, j] && inPattern[i, j + 1] && inPattern[i + 1, j + 1])
                        return false;
                }
            }

            //поиска групп клеток в шаблоне с высотой/шириной меньше 3
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (inPattern[i, j])
                    {
                        // irr
                        if (j + 2 < m && inPattern[i, j + 1] && inPattern[i, j + 2])
                            continue;
                        // rir
                        if (j - 1 >= 0 && j + 1 < m && inPattern[i, j - 1] && inPattern[i, j + 1])
                            continue;
                        // rri
                        if (j - 2 >= 0 && inPattern[i, j - 1] && inPattern[i, j - 2])
                            continue;
                        // icc
                        if (i + 2 < n && inPattern[i + 1, j] && inPattern[i + 2, j])
                            continue;
                        // cic
                        if (i - 1 >= 0 && i + 1 < n && inPattern[i - 1, j] && inPattern[i + 1, j])
                            continue;
                        // cci
                        if (i - 2 >= 0 && inPattern[i - 1, j] && inPattern[i - 2, j])
                            continue;
                        return false;
                    }
                }
            }

            return true;
        }

        //поиск отрезков. Если найдены, вернуть true
        public bool FindWordPlaces()
        {
            bool[,] scoped = new bool[n, m];
            segments = new List<CrosswordSegment>();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (!inPattern[i, j] || scoped[i, j])
                        continue;

                    //check across
                    int l = 0;
                    //проверка клеток слева, может начало отрезка там
                    int k = i;
                    while (k - 1 >= 0 && inPattern[k - 1, j])
                    {
                        k--;
                    }

                    for (int v = k; v < n; v++)
                    {
                        if (!inPattern[v, j])
                            break;
                        scoped[v, j] = true;
                        l++;
                    }
                    if(l > 2)
                    {
                        CrosswordSegment word = new CrosswordSegment(k, j, Directions.Across, l);
                        segments.Add(word);
                    }

                    //check down
                    l = 0;
                    //used to identify the highest cell of down facing segment
                    k = j;
                    //for it check both current and one below being in pattern
                    if (j < m - 1)
                    {
                        if (inPattern[i, j] && inPattern[i, j + 1])
                        {
                            //из-за логики кода некоторые клетки сверху могли быть пропущены
                            //возможно среди клеток свыше находится начало отрезка
                            while (k >= 0)
                            {
                                if (k - 1 >= 0 && inPattern[i, k - 1])
                                    k--;
                                else
                                {
                                    break;
                                }
                            }
                            for (int v = k; v < m; v++)
                            {
                                if (!inPattern[i, v])
                                    break;
                                scoped[i, v] = true;
                                l++;
                            }
                        }
                    }
                    if (l > 2)
                    {
                        CrosswordSegment word = new CrosswordSegment(i, k, Directions.Down, l);
                        segments.Add(word);
                    }
                }
            }

            return segments.Count > 0;
        }

        //попробовать заполнить отрезок словом
        public bool TryFillSegment(CrosswordSegment segment, string word)
        {
            if(segment.length != word.Length)
                return false;

            // c, r
            if (segment.dir == Directions.Across)
            {
                //check if there are no already filled cells
                for(int i = segment.startCellX; i < segment.startCellX + segment.length; i++) 
                {
                    //if there are unmatching characters then return false
                    if (charContainer[i, segment.startCellY] == '\0'
                        || charContainer[i, segment.startCellY] == word[i - segment.startCellX])
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                //fill
                for (int i = segment.startCellX; i < segment.startCellX + segment.length; i++)
                {
                    charContainer[i, segment.startCellY] = word[i - segment.startCellX];
                }
            }

            if (segment.dir == Directions.Down)
            {
                //check if there are no already filled cells
                for (int j = segment.startCellY; j < segment.startCellY + segment.length; j++)
                {
                    if (charContainer[segment.startCellX, j] == '\0'
                        || charContainer[segment.startCellX, j] == word[j - segment.startCellY])
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                //fill
                for (int j = segment.startCellY; j < segment.startCellY + segment.length; j++)
                {
                    charContainer[segment.startCellX, j] = word[j - segment.startCellY];
                }
            }

            return true;
        }

        //метод для решения кроссворда, работает при допущении,
        //что выполнена проверка на структурную корректность шаблона
        public bool SolveCrossword(string[] wordList)
        {
            List<CrosswordSegment> unsolvedSegments = new List<CrosswordSegment>(segments);

            var hashtable = SetWordDictionary(wordList);

            if (!Solve(unsolvedSegments, hashtable, 0))
            {
                // Если не удалось найти решение, вернуть false, но перед этим опустошить charContainer
                charContainer = new char[n, m];
                return false;
            }

            return true;
        }

        private bool Solve(List<CrosswordSegment> unsolvedSegments, Dictionary<char, List<string>> dictionary, int index)
        {
            if (index == unsolvedSegments.Count)
            {
                // Все отрезки заполнены, кроссворд решен.
                return true;
            }

            var segment = unsolvedSegments[index];
            List<string> wordList;

            if (charContainer[segment.startCellX, segment.startCellY] != '\0')
            {
                wordList = (List<string>) dictionary[char.ToUpper(charContainer[segment.startCellX, segment.startCellY])];
            }
            else
            {
                wordList = (List<string>) dictionary['*'];
            }

            for (int i = 0; i < wordList.Count; i++)
            {
                if (TryFillSegment(segment, wordList[i]))
                {
                    // Слово подходит для данного отрезка, переходим к следующему.
                    if (Solve(unsolvedSegments, dictionary, index + 1))
                    {
                        // Если решение найдено, вернуть true.
                        return true;
                    }

                    // Если не удалось решить кроссворд с текущей комбинацией слов,
                    // отменить заполнение и попробовать следующее слово.
                    ClearSegment(segment);
                }
            }

            // Если ни одно из слов не подошло для данного отрезка, вернуть false.
            return false;
        }

        //преобразовать массив слов в словарь для быстрого поиска слов
        private Dictionary<char, List<string>> SetWordDictionary(string[] wordList)
        {
            Dictionary<char, List<string>> wordDictionary = new Dictionary<char, List<string>>();

            foreach (var word in wordList)
            {
                char firstChar = char.ToUpper(word[0]);

                if (!wordDictionary.ContainsKey(firstChar))
                {
                    wordDictionary[firstChar] = new List<string>();
                }

                wordDictionary[firstChar].Add(word);
            }

            // Add all words to the "all" key in the dictionary
            wordDictionary['*'] = new List<string>(wordList);

            // Sort the lists by word length in descending order
            foreach (var key in wordDictionary.Keys.ToList())
            {
                wordDictionary[key] = wordDictionary[key].OrderByDescending(x => x.Length).ToList();
            }

            return wordDictionary;
        }

        private void ClearSegment(CrosswordSegment segment)
        {
            // Очистить отрезок от заполнения.
            if (segment.dir == Directions.Across)
            {
                for (int i = segment.startCellX; i < segment.startCellX + segment.length; i++)
                {
                    charContainer[i, segment.startCellY] = '\0';
                }
            }
            else
            {
                for (int j = segment.startCellY; j < segment.startCellY + segment.length; j++)
                {
                    charContainer[segment.startCellX, j] = '\0';
                }
            }
        }

        public char GetContent(int i, int j) => charContainer[i, j];
    }
    //направление заполнения отрезка
    public enum Directions
    {
        Down,
        Across
    }

    public class CrosswordSegment
    {
        public Directions dir;
        public int startCellX, startCellY;
        public int length;

        public CrosswordSegment(int x, int y, Directions d, int l)
        {
            startCellX = x;
            startCellY = y;
            dir = d;
            length = l;
        }
    }

}
