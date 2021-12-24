using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

// Поиск подстроки в строке с помощью суффиксного дерева (Suffix tree)  (Suffix tree)
namespace lab1
{
    class Program
    {
        static void Main(string[] args)
        {

            const int SIZE = 10000;
            string[] Text = new string[SIZE];

            Console.WriteLine("Размер массива: {0}", SIZE);

            int min_size = 5;
            int max_size = 10;

            Random random = new Random();
            char[] ABC = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            for (int i = 0; i < Text.Length; i++)
            {
                for (int j = 0; j < random.Next(min_size, max_size); j++)
                {
                    Text[i] += ABC[random.Next(0, ABC.Length - 1)];
                }
            }

            //DisplayStringArray(Text);

            string substr = "ababa";
            Console.WriteLine("Подстрока: " + substr);

            Timer timer1 = new Timer();
            timer1.Start();
            foreach (var str in Text)
            {
                BruteForceAlgorithm(str, substr);
            }
            timer1.Stop();
            Console.WriteLine("Время поиска наивного алгоритма: " + timer1.Duration.ToString());


            Timer timer2 = new Timer();
            timer2.Start();
            foreach (var str in Text)
            {
                SuffixTree tree = new SuffixTree(str);
                tree.Find(substr);
            }
            timer2.Stop();
            Console.WriteLine("Время поиска с помощью суффиксного дерева: " + timer2.Duration.ToString());


            Console.ReadKey();
        }

        /// Вывод в консоль массив
        static void DisplayStringArray(string[] array)
        {
            foreach (var str in array)
            {
                Console.WriteLine(str);
            }
        }

        /// нативный алгоритм, возвращает true если в строке есть вхождение указанной подстроки
        static bool BruteForceAlgorithm(string str, string substring)
        {
            for (int i = 0; i <= str.Length - substring.Length; i++)
            {
                bool success = true;
                for (int j = 0; j < substring.Length; j++)
                {
                    if (str[i + j] != substring[j])
                    {
                        success = false;
                        break;
                    }
                }
                if (success) return true;
            }
            return false;
        }


    }

    public class SuffixTree
    {
        public class Node
        {
            public int Index = -1;
            public Dictionary<char, Node> Children = new Dictionary<char, Node>();
        }

        public Node Root = new Node();
        public String Text;

        public void InsertSuffix(string s, int from)
        {
            var cur = Root;
            for (int i = from; i < s.Length; ++i)
            {
                var c = s[i];
                if (!cur.Children.ContainsKey(c))
                {
                    var n = new Node() { Index = from };
                    cur.Children.Add(c, n);



                    return;
                }
                cur = cur.Children[c];
            }
        }

        private static IEnumerable<Node> VisitTree(Node n)
        {
            // yield return: определяет возвращаемый элемент
            // yield break: указывает, что последовательность больше не имеет элементов
            foreach (var n1 in n.Children.Values)
                foreach (var n2 in VisitTree(n1))
                    yield return n2;
            yield return n;
        }

        public IEnumerable<int> Find(string s)
        {
            var n = FindNode(s);
            if (n == null) yield break;
            foreach (var n2 in VisitTree(n))
                yield return n2.Index;
        }

        private Node FindNode(string s)
        {
            var cur = Root;
            for (int i = 0; i < s.Length; ++i)
            {
                var c = s[i];
                if (!cur.Children.ContainsKey(c))
                {
                    // Мы находимся на листе-узле.
                    // Здесь мы проверяем, находится ли остальная часть строки в этом месте.
                    for (var j = i; j < s.Length; ++j)
                        if (cur.Index + j >= Text.Length || Text[cur.Index + j] != s[j])
                            return null;
                    return cur;
                }
                cur = cur.Children[c];
            }
            return cur;
        }

        public SuffixTree(string s)
        {
            Text = s;
            for (var i = s.Length - 1; i >= 0; --i)
                InsertSuffix(s, i);
        }
    }

    class Timer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        public Timer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                Console.WriteLine("Таймер не поддерживается	аппаратной частью компьютера.");
            }

        }

        public double Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
            return 1.0 * startTime / freq;
        }

        public double Stop()
        {
            QueryPerformanceCounter(out stopTime);
            return 1.0 * stopTime / freq;
        }

        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
    }
}
