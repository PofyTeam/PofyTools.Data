namespace PofyTools.Local
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    //using Excel;
    using UnityEngine;

    public static class Localization
    {
        #region Nested Classes

        /// <summary>
        /// Container class for data I/O. Contains data for all languages.
        /// </summary>
        [System.Serializable]
        public class LocalizationData
        {
            public List<LanguageData> data = new List<LanguageData>();
        }

        /// <summary>
        /// Container class for Language data serialization. It is required to use ALPHA2 language abbreviation for language key.
        /// </summary>
        [System.Serializable]
        public class LanguageData
        {
            public string languageKey = "";

            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();

        }

        #endregion

        #region Variables

        public const string TAG = "<color=green><b>LOCALIZATION:</b></color> ";

        private static LocalizationData _Loaded = null;
        private static Dictionary <string, LanguageData> _Languages = null;
        private static Dictionary <string,string> _Strings = null;

        public static Dictionary<string,string> Strings
        {
            get{ return _Strings; }
        }

        private static bool _Initialized = false;

        #endregion

        #region API

        /// <summary>
        /// Initialize the static class and loads Localization Data
        /// </summary>
        public static void Initialize()
        {
            Clear();
            LoadData();

            _Initialized = true;
        }

        /// <summary>
        /// Clears static variables replacing them with new objects.
        /// </summary>
        public static void Clear()
        {
        
            _Loaded = new LocalizationData();
            _Languages = new Dictionary<string, LanguageData>();
            _Strings = new Dictionary<string, string>();
            _Initialized = false;
            Debug.Log(TAG + "Data Cleared!");
        }

        /// <summary>
        /// Loads the localization data and populates the _Languages dictionary.
        /// </summary>
        private static void LoadData()
        {
            if (File.Exists("Assets/Resources/Definitions/localization_data.json"))
            {
//                var json = JsonUtility.ToJson(_Loaded, false);
//                System.IO.File.WriteAllText("Assets/Resources/Definitions/localization_data.json", json);
                var _data = Resources.Load<TextAsset>("Definitions/localization_data").text;
                //Debug.LogError(TAG + "\n" + _data);
                JsonUtility.FromJsonOverwrite(_data, _Loaded);


                _Languages.Clear();
                foreach (var langData in _Loaded.data)
                {
                    _Languages[langData.languageKey] = langData;
                }

                Debug.Log(TAG + "Data Loaded!");
            }
            else
            {
                Debug.LogError(TAG + "Data File Not Found!");
            }
        }

        /// <summary>
        /// Saves the data to JSON.
        /// </summary>
        public static void SaveData()
        {
            if (_Loaded != null)
            {
                var json = JsonUtility.ToJson(_Loaded, false);
                File.WriteAllText("Assets/Resources/Definitions/localization_data.json", json);//C:\svn\
            }
            else
            {
                Debug.LogError(TAG + "Saving Localization Data to json failed!");
            }

            Debug.Log(TAG + "Data Saved!");
        }

        /// <summary>
        /// Sets the language. Populates the _Strings dictionary.
        /// </summary>
        /// <param name="languageKey">Language key - MUST be ALPHA2 language abbreviation.</param>
        public static LanguageData SetLanguage(string languageKey)
        {
            languageKey = languageKey.ToUpper();
            LanguageData data = null;

            if (string.IsNullOrEmpty(languageKey) || languageKey.Length != 2)
            {
                Debug.LogError(TAG + "Language key - " + languageKey + " not valid. Language not set!");
                return data;
            }

            if (_Languages != null)
            {
                if (_Languages.TryGetValue(languageKey, out data))
                {
                    _Strings.Clear();

                    for (int i = 0; i < data.keys.Count; i++)
                    {
                        var key = data.keys[i];

                        if (!_Strings.ContainsKey(key))
                            _Strings[key] = data.values[i];
                        else
                            Debug.LogError(TAG + "Key already added - " + key);
                    }

                    if (LanguageChanged != null)
                        LanguageChanged();
                }
                else
                {
                    Debug.LogError(TAG + "No Language " + languageKey + " found.");
                }
            }
            else
            {
                Debug.LogError(TAG + "LocalizationData not loaded");
            }

            return data;
        }

        /// <summary>
        /// Returns localized string if available or just returns the key.
        /// </summary>
        /// <param name="key">Key.</param>
        public static string Get(string key)
        {
            string cache = key;
            _Strings.TryGetValue(key, out cache);

            return cache;
        }

        /// <summary>
        /// Returns localized string and replaces numeric string codes with provided arguments.
        /// Numeric string codes start with [1].
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="args">Arguments to replace numeric string code with.</param>
        public static string Get(string key, params string[] args)
        {
            string value = Get(key);

            if (args != null && args.Length != 0)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    value = value.Replace("[" + (i + 1).ToString() + "]", args[i]);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the Array of all defined Language Data.
        /// </summary>
        /// <returns> Array of ALPHA2 languages.</returns>
        public static string[] GetLanguages()
        {
            return new List<string>(_Languages.Keys).ToArray();
        }

        public static LanguageData AddLanguage(string languageKey)
        {
            LanguageData data = null;
            if (_Languages.TryGetValue(languageKey, out data))
            {
                Debug.LogWarning(TAG + "Language \"" + languageKey + "\" already defined.");
                return data;
            }

            data = new LanguageData();
            data.languageKey = languageKey;

            if (_Loaded.data.Count > 0)
            {
                data.keys = new List<string>(_Loaded.data[0].keys);
                data.values.Clear();
                foreach (var key in data.keys)
                {
                    data.values.Add("");
                }
            }

            _Loaded.data.Add(data);

            return data;
        }

        /// <summary>
        /// Gets the List of language data.
        /// </summary>
        /// <returns>The data.</returns>
        public static List<LanguageData> GetData()
        {
            if (_Loaded == null)
                LoadData();
        
            return _Loaded.data;
        }

        /// <summary>
        /// Adds the key/value pair to all language data.
        /// </summary>
        /// <param name="key">Key.</param>
        public static void AddPair(string key)
        {
            var allData = GetData();
            foreach (var _data in allData)
            {
                _data.keys.Add(key);
                _data.values.Add("");
            }
        }

        /// <summary>
        /// Removes the pair at index from all languages.
        /// </summary>
        /// <param name="index">Index.</param>
        public static void RemovePairAt(int index)
        {
            var data = Localization.GetData();

            if (data.Count != 0 && data[0].keys.Count > index)
            {
                foreach (var _data in data)
                {
                    _data.keys.RemoveAt(index);
                    _data.values.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Removes the pair.
        /// </summary>
        /// <returns><c>true</c>, if pair was removed, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        public static bool RemovePair(string key)
        {
            var data = Localization.GetData();
            int index = -1;
            index = data[0].keys.IndexOf(key);

            if (index != -1)
            {
                RemovePairAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the language data with provided Language Key.
        /// </summary>
        /// <returns><c>true</c>, if language data was removed, <c>false</c> otherwise.</returns>
        /// <param name="languageKey">Language Key (ALPHA2).</param>
        public static bool RemoveLanguageData(string languageKey)
        {
            LanguageData data = null;

            if (_Languages.TryGetValue(languageKey, out data))
            {
                _Languages.Remove(languageKey);
                return RemoveLanguageData(data);
            }
            return false;
        }

        /// <summary>
        /// Removes the language data.
        /// </summary>
        /// <returns><c>true</c>, if language data was removed, <c>false</c> otherwise.</returns>
        /// <param name="data">Data.</param>
        public static bool RemoveLanguageData(LanguageData data)
        {
            if (_Loaded.data.Remove(data))
            {
                if (_Languages != null)
                    _Languages.Remove(data.languageKey);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if has key the specified key.
        /// </summary>
        /// <returns><c>true</c> if has key the specified key; otherwise, <c>false</c>.</returns>
        /// <param name="key">Key.</param>
        public static bool HasKey(string key)
        {
        
            var allData = GetData();
            if (allData.Count > 0)
                return allData[0].keys.Contains(key);
            return false;
            
        }

        /// <summary>
        /// Checks if Language Data is define for provided Languge Key (ALPHA2)
        /// </summary>
        /// <returns><c>true</c> if has language the specified languageKey; otherwise, <c>false</c>.</returns>
        /// <param name="languageKey">Language key.</param>
        public static bool HasLanguage(string languageKey)
        {

            var allData = GetData();
            foreach (var data in allData)
            {
                if (data.languageKey == languageKey)
                    return true;
            }
            return false;

        }

        //TODO
        public static void ExcelToJson()
        {
//        FileStream streamer = File.Open("Assets\\Resources\\Definitions\\localization_data.xlsx", FileMode.Open, FileAccess.Read);
//
//        IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(streamer);
//
//        DataSet result = excelReader.AsDataSet();
//        while (excelReader.Read())
////        {
//            Debug.Log(excelReader.GetString(0));
////        }
//
//        excelReader.Close();
        }

        /// <summary>
        /// Gets a value indicating is initialized.
        /// </summary>
        /// <value><c>true</c> if is initialized; otherwise, <c>false</c>.</value>
        public static bool IsInitialized
        {
            get{ return _Initialized; }
        }

        #endregion

        #region Events

        public static UpdateDelegate LanguageChanged;

        #endregion
    }

    public interface ILocalizable
    {
        void OnLanguageChange();
    }
}