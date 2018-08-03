using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PofyTools.Data
{

    [System.Serializable]
    public class CategoryDefinition : Definition
    {
        public CategoryDefinition(string key)
        {
            this.id = key;
        }

        [Header("Display Name")]
        public string displayName;

        [TextArea]
        [Header("Category Description")]
        public string categoryDescription;

        [Header("Base Categories")]
        public List<string> baseCategories = new List<string>();

        //[Header("NameSet")]
        //public NameSet nameSet;
        //public NameSet influenceSet;
    }

    public class CategoryData : DefinableData<CategoryDefinition>
    {
        public CategoryData(CategoryDefinition definition) : base(definition) { }

        #region API
        public void AddSubcategory(CategoryData data)
        {
            this.subcategories.Add(data.id);
        }
        #endregion

        #region Runtime Data
        public List<string> subcategories = new List<string>();
        #endregion
    }

    public class CategoryDataSet : DataSet<string, CategoryData>
    {
        public CategoryDataSet(DefinitionSet<CategoryDefinition> categoryDefinitionSet)
        {
            Initialize(categoryDefinitionSet.GetContent());
        }

        /// <summary>
        /// Topmost categories.
        /// </summary>
        public List<CategoryData> rootCategories = new List<CategoryData>();

        public bool Initialize(List<CategoryDefinition> categoryDefs)
        {
            if (!this.IsInitialized)
            {
                this._contentDictionary = new Dictionary<string, CategoryData>(categoryDefs.Count);

                foreach (var category in categoryDefs)
                {
                    CategoryData data = new CategoryData(category);

                    //list
                    this._content.Add(data);

                    //dictionary
                    this._contentDictionary[data.id] = data;

                    if (category.baseCategories.Count == 0)
                    {
                        this.rootCategories.Add(data);
                    }
                }

                Initialize();

                return true;
            }
            return false;
        }

        public override bool Initialize()
        {
            if (!this.IsInitialized)
            {

                foreach (var data in this._content)
                {
                    foreach (var baseCategory in data.Definition.baseCategories)
                    {
                        CategoryData baseData;
                        if (this._contentDictionary.TryGetValue(baseCategory, out baseData))
                        {
                            baseData.AddSubcategory(data);
                        }
                    }
                }
                PofyTools.UI.NotificationView.Show("Game Definitions Initialized!", null, -1f);
                this.IsInitialized = true;
                return true;
            }
            return false;
        }
    }

}