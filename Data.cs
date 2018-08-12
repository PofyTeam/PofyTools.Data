namespace PofyTools.Data
{
    using Extensions;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Collection of keyable values obtainable via key or index.
    /// </summary>
    /// <typeparam name="TKey"> Key Type.</typeparam>
    /// <typeparam name="TValue">Value Type.</typeparam>
    [System.Serializable]
    public abstract class DataSet<TKey, TValue> : IInitializable, IContentProvider<List<TValue>> where TValue : Data<TKey>
    {
        [SerializeField]
        protected List<TValue> _content = new List<TValue>();

        protected Dictionary<TKey, TValue> _contentDictionary = new Dictionary<TKey, TValue>();

        public virtual bool Initialize()
        {
            if (!this.IsInitialized)
            {
                if (this._content.Count != 0)
                {
                    BuildDictionaries();
                    this.IsInitialized = true;
                    return true;
                }

                Debug.LogWarning("Content not available. Aborting Data Set Initialization... " + typeof(TValue).ToString());
                return false;
            }
            return false;
        }

        protected virtual void BuildDictionaries()
        {
            this._contentDictionary.Clear();

            //Add content from list to dictionary
            foreach (var element in this._content)
            {
                if (this._contentDictionary.ContainsKey(element.id))
                    Debug.LogWarning("Id " + element.id + " present in the set. Overwriting...");
                this._contentDictionary[element.id] = element;
            }
        }

        public virtual bool IsInitialized { get; protected set; }

        /// <summary>
        /// Gets content's element via key.
        /// </summary>
        /// <param name="key">Element's key.</param>
        /// <returns>Content's element.</returns>
        public TValue GetValue(TKey key)
        {
            TValue result = default(TValue);

            if (!this.IsInitialized)
            {
                Debug.LogWarning("Data Set Not Initialized! " + typeof(TValue).ToString());
                return result;
            }

            if (!this._contentDictionary.TryGetValue(key, out result))
                Debug.LogWarning("Value Not Found For Key: " + key);

            return result;
        }

        /// <summary>
        /// Gets random element from content.
        /// </summary>
        /// <returns>Random element</returns>
        public TValue GetRandom()
        {
            return this._content.TryGetRandom();
        }

        /// <summary>
        /// Gets random element different from the last random pick.
        /// </summary>
        /// <param name="lastRandomIndex">Index of previously randomly obtained element.</param>
        /// <returns>Random element different from last random.</returns>
        public TValue GetNextRandom(ref int lastRandomIndex)
        {
            int newIndex = lastRandomIndex;
            int length = this._content.Count;

            if (length > 1)
            {
                do
                {
                    newIndex = Random.Range(0, length);
                }
                while (lastRandomIndex == newIndex);
            }

            lastRandomIndex = newIndex;

            return this._content[newIndex];
        }

        /// <summary>
        /// Content's element count.
        /// </summary>
        public int Count
        {
            get { return this._content.Count; }
        }

        public virtual void SetContent(List<TValue> content)
        {
            this._content = content;
        }

        public virtual List<TValue> GetContent()
        {
            return this._content;
        }

        public List<TKey> GetKeys()
        {
            return new List<TKey>(this._contentDictionary.Keys);
        }
    }

    /// <summary>
    /// Collection of definitions obtainable via key or index
    /// </summary>
    /// <typeparam name="T">Definition Type</typeparam>
    [System.Serializable]
    public class DefinitionSet<T> : DataSet<string, T> where T : Definition
    {
        /// <summary>
        /// Definition set file path.
        /// </summary>
        protected string _path;
        protected string _filename;
        protected string _extension;

        protected bool _scrable;
        protected bool _encode;

        public string FullPath
        {
            get
            {
                return this._path + "/" + this._filename + "." + this._extension;
            }
        }

        public string ResourcePath
        {
            get
            {
                return this._path + "/" + this._filename;
            }
        }

        /// <summary>
        /// Definition Set via file path
        /// </summary>
        /// <param name="path">Definition set file path.</param>
        public DefinitionSet(string fullPath, string filename, bool scramble = false, bool encode = false, string extension = "")
        {
            this._path = fullPath;
            this._filename = filename;
            this._extension = extension;

            this._encode = encode;
            this._scrable = scramble;
        }

        #region IInitializable implementation

        public override bool Initialize()
        {
            //Load();
            return base.Initialize();
        }

        #endregion

        #region Instance Methods
        public void Save()
        {
            SaveDefinitionSet(this);
        }

        public void Load()
        {
            LoadDefinitionSet(this);
        }
        #endregion

        #region IO

        public static void LoadDefinitionSet(DefinitionSet<T> definitionSet)
        {
            //DataUtility.LoadOverwrite(definitionSet.FullPath, definitionSet, definitionSet._scrable, definitionSet._encode);
            DataUtility.ResourceLoad(definitionSet.ResourcePath, definitionSet, definitionSet._scrable, definitionSet._encode);

        }

        public static void SaveDefinitionSet(DefinitionSet<T> definitionSet)
        {
            //DataUtility.Save(definitionSet._path, definitionSet._filename, definitionSet, definitionSet._scrable, definitionSet._encode, definitionSet._extension);
            DataUtility.ResourceSave(definitionSet._path, definitionSet._filename, definitionSet, definitionSet._scrable, definitionSet._encode, definitionSet._extension);
        }

        #endregion
    }

    /// <summary>
    /// Collection of loaded data.
    /// </summary>
    /// <typeparam name="TData">Data Type</typeparam>
    /// <typeparam name="TDefinition">Definition Type</typeparam>
    [System.Serializable]
    public class DefinableDataSet<TData, TDefinition> : DataSet<string, TData> where TData : DefinableData<TDefinition> where TDefinition : Definition
    {
        public override bool Initialize()
        {
            return base.Initialize();
        }

        public void DefineSet(DefinitionSet<TDefinition> definitionSet)
        {
            foreach (var data in this._content)
            {
                data.Define(definitionSet.GetValue(data.id));
            }
        }

    }

    public abstract class Data<T>
    {
        public T id;
    }

    public abstract class Definition : Data<string>
    {
        //public string id;
    }

    public class DefinableData<T> : Data<string>, IDefinable<T> where T : Definition
    {
        public DefinableData(T definition)
        {
            Define(definition);
        }

        #region IDefinable

        public T Definition
        {
            get;
            protected set;
        }

        public bool IsDefined { get { return this.Definition != null; } }

        public void Define(T definition)
        {
            this.Definition = definition;
            this.id = this.Definition.id;
        }

        public void Undefine()
        {
            this.Definition = null;
            this.id = string.Empty;
        }

        #endregion
    }

    public interface IDefinable<T> where T : Definition
    {
        T Definition
        {
            get;
        }

        bool IsDefined { get; }

        void Define(T definition);

        void Undefine();
    }

    public interface IDatable<TKey, TValue> where TValue : Data<TKey>
    {
        TValue Data { get; }

        void AppendData(TValue data);

        void ReleaseData();
    }

    public interface IContentProvider<T>
    {
        void SetContent(T content);

        T GetContent();

    }

    public static class DataUtility
    {
        public const string TAG = "<color=yellow><b>DataUtility: </b></color>";

        #region LOAD
        public enum LoadResult : int
        {
            NullObject = -3,
            NullPath = -2,
            FileNotFound = -1,
            Done = 0
        }

        public static LoadResult LoadOverwrite(string fullPath, object objectToOverwrite, bool unscramble = false, bool decode = false)
        {
            if (objectToOverwrite == null)
            {
                Debug.LogWarningFormat("{0}Object to overwrite is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                return LoadResult.NullObject;
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return LoadResult.NullPath;
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogWarningFormat("{0}File \"{1}\" not found! Aborting...", TAG, fullPath);
                return LoadResult.FileNotFound;
            }

            var json = File.ReadAllText(fullPath);

            json = (unscramble) ? DataUtility.UnScramble(json) : json;
            json = (decode) ? DataUtility.DecodeFrom64(json) : json;

            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            Debug.LogFormat("{0}File \"{1}\" loaded successfully!", TAG, fullPath);
            return LoadResult.Done;
        }

        public static LoadResult ResourceLoad(string relativePath, object objectToOverwrite, bool unscramble = false, bool decode = false)
        {
            if (objectToOverwrite == null)
            {
                Debug.LogWarningFormat("{0}Object to overwrite is NULL! Aborting... (\"{1}\")", TAG, relativePath);
                return LoadResult.NullObject;
            }

            var textAsset = Resources.Load<TextAsset>(relativePath);

            if (textAsset == null)
            {
                Debug.LogWarningFormat("{0}File \"{1}\" not found! Aborting...", TAG, relativePath);
                return LoadResult.FileNotFound;
            }

            string json = textAsset.text;

            json = (unscramble) ? DataUtility.UnScramble(json) : json;
            json = (decode) ? DataUtility.DecodeFrom64(json) : json;

            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            Debug.LogFormat("{0}File \"{1}\" loaded successfully!", TAG, relativePath);
            return LoadResult.Done;
        }

        //TODO: T Load

        #endregion

        #region SAVE
        [System.Flags]
        public enum SaveResult : int
        {
            Done = 1 << 0,
            NullObject = 1 << 1,
            NullPath = 1 << 2,
            DirectoryCreated = 1 << 3,
        }

        public static SaveResult Save(string fullPath, string filename, object objectToSave, bool scramble = false, bool encode = false, string extension = "")
        {
            SaveResult result = 0;

            //Check input
            if (objectToSave == null)
            {
                Debug.LogWarningFormat("{0}Object you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                result = result.Add(SaveResult.NullObject);
                return result;
            }

            //Check Path
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                result = result.Add(SaveResult.NullPath);
                return result;
            }

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                result = result.Add(SaveResult.DirectoryCreated);
            }

            var json = JsonUtility.ToJson(objectToSave);

            json = (encode) ? DataUtility.EncodeTo64(json) : json;
            json = (scramble) ? DataUtility.Scramble(json) : json;

            File.WriteAllText(fullPath + "/" + filename + "." + extension, json);

            result = result.Add(SaveResult.Done);
            Debug.LogFormat("{0}File \"{1}\" saved successfully!", TAG, fullPath + "/" + filename + "." + extension);
            return result;
        }

        public static SaveResult ResourceSave(string relativePath, string filename, object objectToSave, bool scramble = false, bool encode = false, string extension = "")
        {
            string fullPath = Application.dataPath + "/Resources/" + relativePath;
            return Save(fullPath, filename, objectToSave, scramble, encode, extension);

        }

        #endregion

        #region SCRAMBLE

        static string Scramble(string toScramble)
        {
            StringBuilder toScrambleSB = new StringBuilder(toScramble);
            StringBuilder scrambleAddition = new StringBuilder(toScramble.Substring(0, toScramble.Length / 2 + 1));
            for (int i = 0, j = 0; i < toScrambleSB.Length; i = i + 2, ++j)
            {
                scrambleAddition[j] = toScrambleSB[i];
                toScrambleSB[i] = 'c';
            }

            StringBuilder finalString = new StringBuilder();
            int totalLength = toScrambleSB.Length;
            string length = totalLength.ToString();
            finalString.Append(length);
            finalString.Append("!");
            finalString.Append(toScrambleSB.ToString());
            finalString.Append(scrambleAddition.ToString());

            return finalString.ToString();
        }

        static string UnScramble(string scrambled)
        {
            int indexOfLenghtMarker = scrambled.IndexOf("!");
            string strLength = scrambled.Substring(0, indexOfLenghtMarker);
            int lengthOfRealData = int.Parse(strLength);
            StringBuilder toUnscramble = new StringBuilder(scrambled.Substring(indexOfLenghtMarker + 1, lengthOfRealData));
            string substitution = scrambled.Substring(indexOfLenghtMarker + 1 + lengthOfRealData);
            for (int i = 0, j = 0; i < toUnscramble.Length; i = i + 2, ++j)
                toUnscramble[i] = substitution[j];

            return toUnscramble.ToString();
        }

        #endregion

        #region ENCODE

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            return returnValue;
        }

        #endregion

        #region Textures

        public static void IncrementSaveToPNG(string filePath, string fileName, Texture2D texture)
        {
            int count = 0;

            if (texture == null)
            {
                Debug.LogWarningFormat("{0}Texture you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fileName);
                return;
            }

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return;
            }

            if (filePath[filePath.Length - 1] != '/' && fileName[0] != '/')
            {
                filePath += "/";
            }

            while (File.Exists(filePath + fileName + count + ".png"))
            {
                count++;
            }

            SaveToPNG(filePath + fileName + count + ".png", texture);
        }

        public static void SaveToPNG(string fullPath, Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogWarningFormat("{0}Texture you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                return;
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return;
            }

            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        }

        #endregion

        #region Strings

        public static List<string> OptimizeStringList(List<string> toOptimize)
        {
            toOptimize.Sort();
            for (int i = toOptimize.Count - 1; i >= 0; --i)
            {
                toOptimize[i] = toOptimize[i].Trim().ToLower();
                if (i < toOptimize.Count - 1)
                {
                    var left = toOptimize[i];
                    var right = toOptimize[i + 1];
                    if (left == right)
                    {
                        toOptimize.RemoveAt(i);
                    }
                }
            }
            return toOptimize;
        }

        public static string OptimizeString(string toOptimize)
        {
            return toOptimize.Trim().ToLower();
        }

        #endregion
    }

    /// <summary>
    /// String Float Pair.
    /// </summary>
    [System.Serializable]
    public struct StringValue
    {
        [SerializeField]
        private string _key;
        public string Key
        {
            get { return this._key; }
        }
        public float value;

        public StringValue(string key, float value)
        {
            this._key = key;
            this.value = value;
        }

        public StringValue(string key)
        {
            this._key = key;
            this.value = 0f;
        }

        #region Implicit Casts

        public static implicit operator float(StringValue stringValue)
        {
            return stringValue.value;
        }

        public static implicit operator string(StringValue stringValue)
        {
            return stringValue._key;
        }

        #endregion
    }

    /// <summary>
    /// String Int Pair.
    /// </summary>
    [System.Serializable]
    public struct StringAmount
    {
        [SerializeField]
        private string _key;
        public string Key
        {
            get { return this._key; }
        }
        public int amount;

        public StringAmount(string key, int amount)
        {
            this._key = key;
            this.amount = amount;
        }

        public StringAmount(string key)
        {
            this._key = key;
            this.amount = 0;
        }

        #region Implicit Casts

        public static implicit operator int(StringAmount stringAmount)
        {
            return stringAmount.amount;
        }

        public static implicit operator string(StringAmount stringAmount)
        {
            return stringAmount._key;
        }

        #endregion
    }


}