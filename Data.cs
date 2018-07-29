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

        public Dictionary<TKey, TValue> content = new Dictionary<TKey, TValue>();

        public virtual bool Initialize()
        {
            if (!this.IsInitialized)
            {
                if (this._content.Count == 0)
                {
                    BuildDictionary();
                    this.IsInitialized = true;
                    return true;
                }

                Debug.LogWarning("Content not available. Aborting Data Set Initialization... " + typeof(TValue).ToString());
                return false;
            }
            return false;
        }

        protected void BuildDictionary()
        {
            this.content.Clear();
            
            //Add content from list to dictionary
            foreach (var element in this._content)
            {
                if (this.content.ContainsKey(element.id))
                    Debug.LogWarning("Id " + element.id + " present in the set. Overwriting...");
                this.content[element.id] = element;
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

            if (!this.content.TryGetValue(key, out result))
                Debug.LogWarning("Value Not Found For Key: " + key);

            return result;
        }

        /// <summary>
        /// Gets random element from content.
        /// </summary>
        /// <returns>Random element</returns>
        public TValue GetRandom()
        {
            return this._content.GetRandom();
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
            return new List<TKey>(this.content.Keys);
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

        /// <summary>
        /// Definition Set via file path
        /// </summary>
        /// <param name="path">Definition set file path.</param>
        public DefinitionSet(string path)
        {
            this._path = path;
        }

        #region IInitializable implementation

        public override bool Initialize()
        {
            Load();
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
            //Read the list content from file
            DefinitionSet<T>.LoadDefinitionSet(this);

            ////Create set's dictionary same size as list
            //this.content = new Dictionary<string, T>(this._content.Count);

            ////Add definitions from list to dicionary
            //foreach (var def in this._content)
            //{
            //    if (this.content.ContainsKey(def.id))
            //        Debug.LogWarning("Key " + def.id + " present in the set. Overwriting...");
            //    this.content[def.id] = def;
            //}
        }

        public void Reload()
        {
            DefinitionSet<T>.LoadDefinitionSet(this);
            this.content.Clear();
            //Add definitions from list to dicionary
            foreach (var def in this._content)
            {
                if (this.content.ContainsKey(def.id))
                    Debug.LogWarning("Key " + def.id + " present in the set. Overwriting...");
                this.content[def.id] = def;
            }
        }
        #endregion

        #region IO

        public static void LoadDefinitionSet(DefinitionSet<T> definitionSet)
        {
            string fullPath = Application.dataPath + definitionSet._path;
            DataUtility.LoadOverwrite(fullPath, definitionSet);
        }

        public static void SaveDefinitionSet(DefinitionSet<T> definitionSet)
        {
            string fullPath = Application.dataPath + definitionSet._path;
            DataUtility.Save(fullPath, definitionSet);
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
            return LoadResult.Done;
        }

        //TODO: T Load

        #endregion

        #region SAVE

        public static void Save(string fullPath, object objectToSave, bool scramble = false, bool encode = false)
        {
            if (objectToSave == null)
            {
                Debug.LogWarningFormat("{0}Object you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                return;
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return;
            }

            var json = JsonUtility.ToJson(objectToSave);

            json = (encode) ? DataUtility.EncodeTo64(json) : json;
            json = (scramble) ? DataUtility.Scramble(json) : json;

            File.WriteAllText(fullPath, json);
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
    }

}