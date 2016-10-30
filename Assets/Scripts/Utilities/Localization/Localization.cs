using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


public static class Localization
{
    public static string Get(string key, params object[] formatValues)
    {
        Init();

        SystemLanguage lang = langToClosestExisting[Language.Value];
        string value = itemsByLang[lang][key];
        value = value.Replace("\\n", "\n");
        return string.Format(value, formatValues);
    }

    public static GameLogic.Stat<SystemLanguage, object> Language
    {
        get
        {
            if (language == null)
                language = new GameLogic.Stat<SystemLanguage, object>(null, Application.systemLanguage);
            return language;
        }
    }
    private static GameLogic.Stat<SystemLanguage, object> language = null;

    public static IEnumerable<SystemLanguage> SupportedLanguages { get { Init();  return itemsByLang.Keys; } }
    public static bool IsSupported(SystemLanguage lang) { Init();  return itemsByLang.ContainsKey(lang); }


    private static Dictionary<SystemLanguage, Dictionary<string, string>> itemsByLang;
    private static Dictionary<SystemLanguage, SystemLanguage> langToClosestExisting;


    private static bool isInitialized = false;
    private static void Init()
    {
        if (isInitialized)
            return;
        isInitialized = true;


        //Read all localization files.

        itemsByLang = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        string folderPath = (Application.isEditor ?
                                 Path.Combine(Application.dataPath, "..\\Copy To Build\\Copy To _Data\\Localization") :
                                 Path.Combine(Application.dataPath, "Localization"));
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        foreach (FileInfo file in dir.GetFiles())
        {
            Debug.LogError(file.Name);
            itemsByLang.Add(ToSysLang(Path.GetFileNameWithoutExtension(file.Name)),
                            ReadFile(file));
        }

        CalculateClosestLanguages();
    }
    private static SystemLanguage ToSysLang(string langName)
    {
        switch (langName.ToLower())
        {
            case "afr":
            case "afrikaans":
                return SystemLanguage.Afrikaans;

            case "ara":
            case "ar":
            case "arabic":
                return SystemLanguage.Arabic;

            case "baq":
            case "eus":
            case "eu":
            case "basque":
                return SystemLanguage.Basque;

            case "bel":
            case "be":
            case "belarusian":
                return SystemLanguage.Belarusian;

            case "bul":
            case "bg":
            case "bulgarian":
                return SystemLanguage.Bulgarian;

            case "cat":
            case "ca":
                return SystemLanguage.Catalan;

            case "chi":
            case "zho":
            case "zh":
            case "chinese":
                return SystemLanguage.Chinese;

            case "cze":
            case "ces":
            case "czech":
                return SystemLanguage.Czech;

            case "dan":
            case "da":
            case "danish":
                return SystemLanguage.Danish;

            case "dut":
            case "nld":
            case "dutch":
                return SystemLanguage.Dutch;

            case "en":
            case "eng":
            case "english":
                return SystemLanguage.English;

            case "est":
            case "et":
            case "estonian":
                return SystemLanguage.Estonian;

            case "fao":
            case "fo":
            case "faroese":
                return SystemLanguage.Faroese;

            case "fin":
            case "fi":
                return SystemLanguage.Finnish;

            case "fre":
            case "fra":
            case "french":
                return SystemLanguage.French;

            case "ger":
            case "deu":
                return SystemLanguage.German;

            case "gre":
            case "ell":
            case "greek":
                return SystemLanguage.Greek;

            case "heb":
            case "he":
            case "hebrew":
                return SystemLanguage.Hebrew;

            case "hun":
            case "hu":
            case "hungarian":
                return SystemLanguage.Hungarian;

            case "ice":
            case "isl":
            case "is":
            case "icelandic":
                return SystemLanguage.Icelandic;

            case "ind":
            case "id":
            case "indonesian":
                return SystemLanguage.Indonesian;

            case "ita":
            case "it":
            case "italian":
                return SystemLanguage.Italian;

            case "jpn":
            case "ja":
            case "japanese":
                return SystemLanguage.Japanese;

            case "kor":
            case "ko":
            case "korean":
                return SystemLanguage.Korean;

            case "lav":
            case "lv":
            case "latvian":
                return SystemLanguage.Latvian;

            case "lit":
            case "lt":
            case "lithuanian":
                return SystemLanguage.Lithuanian;

            case "nor":
            case "no":
            case "norwegian":
                return SystemLanguage.Norwegian;

            case "pol":
            case "pl":
            case "polish":
                return SystemLanguage.Polish;

            case "por":
            case "pt":
            case "portugese":
                return SystemLanguage.Portuguese;

            case "rum":
            case "ron":
            case "ro":
            case "romanian":
                return SystemLanguage.Romanian;

            case "rus":
            case "ru":
            case "russian":
                return SystemLanguage.Russian;

            case "hrv":
            case "hr":
            case "croatian":
            case "srp":
            case "sr":
            case "serbian":
            case "bos":
            case "bs":
            case "bosnian":
                return SystemLanguage.SerboCroatian;

            case "slo":
            case "slk":
            case "sk":
            case "slovak":
                return SystemLanguage.Slovak;

            case "slv":
            case "sl":
            case "slovenian":
                return SystemLanguage.Slovenian;

            case "spa":
            case "es":
            case "spanish":
                return SystemLanguage.Spanish;

            case "tha":
            case "th":
            case "thai":
                return SystemLanguage.Thai;

            case "tur":
            case "tr":
            case "turkish":
                return SystemLanguage.Turkish;

            case "ukr":
            case "uk":
            case "ukranian":
                return SystemLanguage.Ukrainian;

            case "vie":
            case "vi":
            case "vietnamese":
                return SystemLanguage.Vietnamese;

            default:
                throw new NotImplementedException(langName);
        }
    }
    private static Dictionary<string, string> ReadFile(FileInfo langFile)
    {
        Dictionary<string, string> outItems = new Dictionary<string, string>();

        foreach (string line in File.ReadAllLines(langFile.FullName))
        {
            int splitter = line.IndexOf(':');
            outItems.Add(line.Substring(0, splitter),
                         line.Substring(splitter + 1));
        }

        return outItems;
    }
    private static void CalculateClosestLanguages()
    {
        langToClosestExisting = new Dictionary<SystemLanguage, SystemLanguage>();

        AddClosestLang(SystemLanguage.Afrikaans, SystemLanguage.Dutch, SystemLanguage.German);
        AddClosestLang(SystemLanguage.Basque, SystemLanguage.Spanish, SystemLanguage.French);
        AddClosestLang(SystemLanguage.Belarusian, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Bulgarian, SystemLanguage.Russian, SystemLanguage.Romanian, SystemLanguage.Hungarian);
        AddClosestLang(SystemLanguage.Catalan, SystemLanguage.Spanish);
        AddClosestLang(SystemLanguage.Chinese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional);
        AddClosestLang(SystemLanguage.ChineseSimplified, SystemLanguage.Chinese, SystemLanguage.ChineseTraditional);
        AddClosestLang(SystemLanguage.ChineseTraditional, SystemLanguage.Chinese, SystemLanguage.ChineseSimplified);
        AddClosestLang(SystemLanguage.Czech, SystemLanguage.Slovak, SystemLanguage.Polish, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Danish, SystemLanguage.Norwegian, SystemLanguage.Swedish, SystemLanguage.German);
        AddClosestLang(SystemLanguage.Dutch, SystemLanguage.German);
        AddClosestLang(SystemLanguage.Estonian, SystemLanguage.Finnish, SystemLanguage.Swedish, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Faroese, SystemLanguage.Icelandic, SystemLanguage.Danish);
        AddClosestLang(SystemLanguage.Finnish, SystemLanguage.Swedish, SystemLanguage.Estonian);
        AddClosestLang(SystemLanguage.French);
        AddClosestLang(SystemLanguage.German);
        AddClosestLang(SystemLanguage.Greek);
        AddClosestLang(SystemLanguage.Hebrew, SystemLanguage.Arabic);
        AddClosestLang(SystemLanguage.Hungarian, SystemLanguage.Romanian, SystemLanguage.Slovak, SystemLanguage.SerboCroatian);
        AddClosestLang(SystemLanguage.Icelandic, SystemLanguage.Danish, SystemLanguage.Faroese);
        AddClosestLang(SystemLanguage.Indonesian);
        AddClosestLang(SystemLanguage.Italian);
        AddClosestLang(SystemLanguage.Japanese);
        AddClosestLang(SystemLanguage.Korean);
        AddClosestLang(SystemLanguage.Latvian, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Lithuanian, SystemLanguage.Polish);
        AddClosestLang(SystemLanguage.Norwegian, SystemLanguage.Swedish, SystemLanguage.Danish);
        AddClosestLang(SystemLanguage.Polish, SystemLanguage.Belarusian, SystemLanguage.Czech, SystemLanguage.Lithuanian, SystemLanguage.Slovak, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Portuguese, SystemLanguage.Spanish);
        AddClosestLang(SystemLanguage.Romanian, SystemLanguage.Portuguese, SystemLanguage.Spanish);
        AddClosestLang(SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.SerboCroatian);
        AddClosestLang(SystemLanguage.Slovak, SystemLanguage.Czech, SystemLanguage.Portuguese, SystemLanguage.Russian);
        AddClosestLang(SystemLanguage.Slovenian, SystemLanguage.SerboCroatian);
        AddClosestLang(SystemLanguage.Spanish);
        AddClosestLang(SystemLanguage.Swedish, SystemLanguage.Norwegian, SystemLanguage.Danish);
        AddClosestLang(SystemLanguage.Thai, SystemLanguage.Chinese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional);
        AddClosestLang(SystemLanguage.Turkish, SystemLanguage.Arabic);
        AddClosestLang(SystemLanguage.Ukrainian, SystemLanguage.Russian, SystemLanguage.Belarusian, SystemLanguage.Polish, SystemLanguage.Slovak, SystemLanguage.Romanian);
        AddClosestLang(SystemLanguage.Vietnamese);
        AddClosestLang(SystemLanguage.Unknown);

        UnityEngine.Assertions.Assert.IsTrue(itemsByLang.ContainsKey(SystemLanguage.English));
        langToClosestExisting.Add(SystemLanguage.English, SystemLanguage.English);
    }
    private static void AddClosestLang(SystemLanguage l, params SystemLanguage[] ls)
    {
        //See if the language itself exists.
        if (itemsByLang.ContainsKey(l))
        {
            langToClosestExisting.Add(l, l);
            return;
        }

        //See if the other given languages exist, in order.
        foreach (SystemLanguage lang in ls)
        {
            if (itemsByLang.ContainsKey(lang))
            {
                langToClosestExisting.Add(l, lang);
                return;
            }
        }

        //We know english exists.
        langToClosestExisting.Add(l, SystemLanguage.English);
    }
}