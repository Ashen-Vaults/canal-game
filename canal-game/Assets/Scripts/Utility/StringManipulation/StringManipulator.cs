using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// This class should contain any methods
/// that will be used for manipulating strings.
/// </summary>
#pragma warning disable 0219 
public static class StringManipulator
{
    // Convert the string to camel case.
    public static string ToCamelCase(this string _string)
    {
        return StringManipulator.ToCamelCase(_string, false);
    }

    // Convert the string to camel case.
    public static string ToCamelCase(this string _string, bool _lowerCase)
    {
        // If there are 0 or 1 characters, just return the string.
        if (_string == null || _string.Length < 2)
            return _string;

        // Split the string into words.
        string[] words = _string.Split(
            new char[] { },
            StringSplitOptions.RemoveEmptyEntries);

        // Combine the words.
        string result = words[0].ToLower();
        for (int i = 1; i < words.Length; i++)
        {
            if (_lowerCase)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1).ToLower();
            }
            else
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }
        }
        return result;
    }

    /// <summary>
    /// Remove HTML from string with Regex.
    /// </summary>
    public static string RemoveTagsRegex(this string _source)
    {
        return Regex.Replace(_source, "<.*?>", string.Empty);
    }

    /// <summary>
    /// Compiled regular expression for performance.
    /// </summary>
    static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Singleline);

    /// <summary>
    /// Remove HTML from string with compiled Regex.
    /// </summary>
    public static string RemoveTagsRegexCompiled(this string _source)
    {
        return _htmlRegex.Replace(_source, string.Empty);
    }

    /// <summary>
    /// Remove HTML tags from string using char array.
    /// </summary>
    public static string RemoveHTMLTags(this string _source)
    {
        char[] array = new char[_source.Length];
        int arrayIndex = 0;
        bool inside = false;

        for (int i = 0; i < _source.Length; i++)
        {
            char let = _source[i];
            if (let == '<')
            {
                inside = true;
                continue;
            }
            if (let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = let;
                arrayIndex++;
            }
        }
        return new string(array, 0, arrayIndex);
    }

    /// <summary>
    /// Remove HTML tags from string using char array.
    /// </summary>
    public static string RemoveHTMLTags(this string _source, string _textToRemove)
    {
        char[] array = new char[_source.Length];
        int arrayIndex = 0;
        bool inside = false;

        for (int i = 0; i < _source.Length; i++)
        {
            char let = _source[i];
            if (let == '<')
            {
                inside = true;
                continue;
            }
            if (let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = let;
                arrayIndex++;
            }
        }
        return new string(array, 0, arrayIndex);
    }

    /// <summary>
    /// Replaces the whole word.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="word">The word.</param>
    /// <param name="bywhat">The bywhat.</param>
    /// <returns></returns>
    public static String ReplaceWholeWord(this String s, String word, String bywhat)
    {
        char firstLetter = word[0];
        StringBuilder sb = new StringBuilder();
        bool previousWasLetterOrDigit = false;
        int i = 0;
        while (i < s.Length - word.Length + 1)
        {
            bool wordFound = false;
            char c = s[i];
            if (c == firstLetter)
                if (!previousWasLetterOrDigit)
                    if (s.Substring(i, word.Length).Equals(word))
                    {
                        wordFound = true;
                        bool wholeWordFound = true;
                        if (s.Length > i + word.Length)
                        {
                            if (Char.IsLetterOrDigit(s[i + word.Length]))
                                wholeWordFound = false;
                        }

                        if (wholeWordFound)
                            sb.Append(bywhat);
                        else
                            sb.Append(word);

                        i += word.Length;
                    }

            if (!wordFound)
            {
                previousWasLetterOrDigit = Char.IsLetterOrDigit(c);
                sb.Append(c);
                i++;
            }
        }

        if (s.Length - i > 0)
            sb.Append(s.Substring(i));

        return sb.ToString();
    }

    /// <summary>
    /// Converts Unity Colors to hexadecimal.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns></returns>
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return "#"+hex;
    }

    /// <summary>
    /// Breaks the string into array, where
    /// the array size is the number of sections you want to break 
    /// the text into.
    /// </summary>
    /// <param name="_text">The _text.</param>
    /// <param name="_numSections">The _num sections.</param>
    /// <returns></returns>
    public static string[] BreakStringIntoArray(this string _text, int _numSections)
    {


        string[] _temp = _text.Split();

        float _numberOfWordsPerSection = (_temp.Length/_numSections);

        string[] _sections = new string[_numSections];

        List<string> _AllText = new List<string>();

        _AllText = _sections.ToList();

        return _sections;
    }

    public static IEnumerable<string> WholeChunks(this string str, int chunkSize)
    {
        for (int i = 0; i < str.Length; i += chunkSize)
            yield return str.Substring(i, chunkSize);
    }
}
