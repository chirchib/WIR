using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2
{
    class Program
    {
        static void Main(string[] args)
        {
            BKTree tree = new BKTree();

            string[] dictionary = { "hell","help","shell","smell",
                                    "fell","felt","oops","pop","oouch","halt"
                                  };

            tree.LoadDictionary(dictionary);

            // Совпадения поиск help
            string str = "felj";
            IEnumerable<string> result = tree.GetSpellingSuggestions(str);

            foreach (string s in dictionary)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine();
            Console.WriteLine("Совпадения: " + str);
            foreach (string s in result)
            {
                Console.Write(s + " ");
            }

            Console.WriteLine();
            Console.ReadKey();
        }
    }

    class BKTree
    {
        private BKNode m_root = null;
        private static int s_defaultTolerance = 2;

        // Добавляет новое слово в дерево
        public void AddWord(string word)
        {
            if (m_root == null)
            {
                m_root = new BKNode(word);
            }
            else
            {
                m_root.AddNode(word);
            }
        }

        public bool WordExists(string word)
        {
            return m_root != null ? m_root.FindWord(word) : false;
        }

        public IEnumerable<string> GetSpellingSuggestions(string word)
        {
            return GetSpellingSuggestions(word, s_defaultTolerance);
        }

        public IEnumerable<string> GetSpellingSuggestions(string word, int tolerance)
        {
            if (m_root == null) return null;

            List<string> suggestions = new List<string>();
            suggestions.AddRange(m_root.FindSuggestions(word, tolerance));
            return suggestions;
        }


        public void LoadDictionary(string[] Text)
        {

            for (int i = 0; i < Text.Length; i++)
            {
                AddWord(Text[i]);
            }
        }



        private class BKNode
        {
            private string m_word;
            private Dictionary<int, BKNode> m_children;

            public BKNode(string word)
            {
                m_word = word;
                m_children = new Dictionary<int, BKNode>();
            }

            public void AddNode(string word)
            {
                int dist = Levenshtein.Compute(word, m_word);

                // Слово уже в словаре!
                if (dist == 0) return;

                BKNode child;
                if (m_children.TryGetValue(dist, out child))
                {
                    // У нас уже есть еще один узел на таком же расстоянии, поэтому добавьте это слово в поддерево этого узла.
                    child.AddNode(word);
                }
                else
                {
                    // У нас нет ребенка на таком расстоянии, поэтому добавляется новый ребенок
                    m_children.Add(dist, new BKNode(word));
                }
            }

            public bool FindWord(string word)
            {
                int dist = Levenshtein.Compute(word, m_word);

                // Слово уже в словаре!
                if (dist == 0) return true;

                BKNode child;
                if (m_children.TryGetValue(dist, out child))
                {
                    // У нас уже есть еще один узел на таком же расстоянии, поэтому добавьте это слово в поддерево этого узла.
                    return child.FindWord(word);
                }

                return false;
            }

            public IEnumerable<string> FindSuggestions(string word, int tolerance)
            {
                List<string> suggestions = new List<string>();

                int dist = Levenshtein.Compute(word, m_word);
                if (dist < tolerance)
                {
                    suggestions.Add(m_word);
                }

                foreach (int key in m_children.Keys)
                {
                    if (key >= dist - tolerance && key <= dist + tolerance)
                    {
                        suggestions.AddRange(m_children[key].FindSuggestions(word, tolerance));
                    }
                }

                return suggestions;
            }
        }
    }

    static class Levenshtein
    {
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; ++i)
            {
                for (int j = 1; j <= m; ++j)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}
