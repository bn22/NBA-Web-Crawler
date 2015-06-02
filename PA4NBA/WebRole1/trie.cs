using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class trie
    {
        private trieNode root;

        /// <summary>
        /// This method creates the initalize trie, which contains a single trieNode.
        /// </summary>
        public trie()
        {
            this.root = new trieNode();
        }

        /// <summary>
        ///This method adds the given String into the trie 
        /// </summary>
        /// <param name="title">String</param>
        public void addTitles(String title)
        {
            title = title.ToLower();
            root = addTitlesHelper(title, root);
        }

        /// <summary>
        /// This method returns the child of the current root so the given String can be stored.
        /// If a trieNode doesn't exist, this method will create a new trieNode to store the current
        /// letter of the given String
        /// </summary>
        /// <param name="title">String</param>
        /// <param name="root">trieNode</param>
        /// <returns>trieNode</returns>
        private trieNode addTitlesHelper(String title, trieNode root)
        {
            int pos = title[0] - 'a';
            if (pos == -2)
            {
                pos = 26;
            }
            if (root.child[pos] == null)
            {
                if (title[0] == 95)
                {
                    root.child[pos] = new trieNode(' ', root.value);
                }
                else
                {
                    root.child[pos] = new trieNode(title[0], root.value);
                }
            }
            if (title.Length > 1)
            {
                root.child[pos] = addTitlesHelper(title.Substring(1), root.child[pos]);
            }
            else //title.Length <= 1
            {
                root.child[pos].word = true;
            }
            return root;
        }

        /// <summary>
        /// This method uses the given String to find the 10 possible suggestions from the trie.
        /// The 10 possible suggestions is returned as a List<String>
        /// </summary>
        /// <param name="prefix">String</param>
        /// <returns>List<String></returns>

        public List<String> findSuggestions(String prefix)
        {
            prefix = prefix.ToLower();
            List<String> results = new List<String>();
            Boolean allAlphabet = true;
            for (int i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] < 65 || prefix[i] > 90)
                {
                    if (prefix[i] < 97 || prefix[i] > 122)
                    {
                        if (prefix[i] != 32)
                        {
                            allAlphabet = false;
                        }
                    }
                }
                if (allAlphabet == false)
                {
                    return results;
                }
            }
            return findSuggestionsHelper(prefix, root, results);
        }

        /// <summary>
        /// This method tranverses through the trie using the given String. This finds location
        /// of the trieNode that are the child of the last character in the given String
        /// </summary>
        /// <param name="prefix">String</param>
        /// <param name="root">trieNode</param>
        /// <param name="results">List<String></param>
        /// <returns>List<String></returns>
        private List<String> findSuggestionsHelper(String prefix, trieNode root, List<String> results)
        {
            trieNode lastLetterOfPrefix = new trieNode();
            if (root == null)
            {
                return results;
            }
            else //root != null
            {
                for (int i = 0; i < prefix.Length; i++)
                {
                    int pos = prefix[i] - 'a';
                    if (pos == -2 || pos == -65)
                    {
                        pos = 26;
                    }
                    try
                    {
                        root = root.child[pos];
                    }
                    catch (NullReferenceException)
                    {
                    }
                    lastLetterOfPrefix = root;
                }
                for (int i = 0; i < 27; i++)
                {
                    try
                    {
                        listPopulator(root.child[i], results);
                    }
                    catch (NullReferenceException)
                    {

                    }
                }
            }
            return results;
        }

        /// <summary>
        /// This method uses the given trieNode to check if they are the end of the word. If the given trieNode is the end of the word,
        /// it is added to the given List<String> that stores the 10 possible results.
        /// </summary>
        /// <param name="root">trieNode</param>
        /// <param name="results">List<String></param>
        private void listPopulator(trieNode root, List<String> results)
        {
            if (root != null && results.Count() <= 10)
            {
                if (root.word == true)
                {
                    results.Add(root.value);
                }
                for (int i = 0; i < 27; i++)
                {
                    listPopulator(root.child[i], results);
                }
            }
        }

         public Boolean isEmpty()
        {
            for (int i = 0; i < 27; i++)
            {
                if (root.child[i] != null)
                {
                    return false;
                } 
            }
             return true;
        }
    }
}