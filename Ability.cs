using Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace PofyTools.Data
{
    [System.Serializable]
    public class AbilityDefinition : Definition
    {
        #region Contstructors
        public AbilityDefinition() { }
        public AbilityDefinition(string key)
        {
            this.id = key;
        }
        #endregion

        [Header("Display Name ID")]
        public string displayNameId;

        //[TextArea]
        [Header("Description")]
        public string descriptionId;

        [Header("Base Abilities")]
        public List<string> baseAbilities = new List<string>();

        //id is same as of the category
        public int IdHash { get; private set; }

        [Header("Group ID")]
        [Tooltip("Group ID is used for visual grouping under one button.")]
        public string groupId;
        public int GropIdHash { get; private set; }

        public AbilityCost[] cost;
    }

    [System.Serializable]
    public struct AbilityCost
    {
        public CurrencyType type;
        public int amount;
    }
    /// <summary>
    /// ...Goran...
    /// </summary>
    public enum CurrencyType : int
    {
        SkillPoints = 0,
        Legs = 1,
        Arms = 2,
        Heads = 3,
    }

    [System.Serializable]
    public class AbilityData : DefinableData<AbilityDefinition>
    {
        #region CategoryData

        public AbilityData(AbilityDefinition definition) : base(definition) { }

        #region Runtime Data
        public List<AbilityData> _subabilities = new List<AbilityData>();

        public List<AbilityData> _superabilities = new List<AbilityData>();
        #endregion

        #region Initialization API

        /// <summary>
        /// Add Sub-Ability and propagate to all Super-Abilities
        /// </summary>
        /// <param name="data"></param>
        public void AddSubability(AbilityData data)
        {
            this._subabilities.AddOnce(data);
            foreach (var ability in this._superabilities)
            {
                ability.AddSubability(data);
            }
        }

        /// <summary>
        /// Add Super-Ability and propagate to all Sub-Abilities
        /// </summary>
        /// <param name="data"></param>
        public void AddSuperability(AbilityData data)
        {
            this._superabilities.AddOnce(data);
            foreach (var ability in this._subabilities)
            {
                ability.AddSuperability(data);
            }
        }

        /// <summary>
        /// Is Ability with abilityId Sub-Ability of this Ability (xD)
        /// </summary>
        /// <param name="abilityId"></param>
        /// <returns></returns>
        public bool IsSubabilityOf(string abilityId)
        {
            if (this.id == abilityId)
                return true;

            foreach (var superabilityId in this.descriptor.superabilityIds)
            {
                if (abilityId == superabilityId)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        public Descriptor descriptor = new Descriptor();

        [System.Serializable]
        public class Descriptor
        {
            public string id;
            public List<string> subabilityIds = new List<string>();
            public List<string> superabilityIds = new List<string>();
        }

        #endregion

        public enum State : int
        {
            Unavailable = 0,
            Available = 1,
            Active = 2,
        }

        public State state;
    }

    [System.Serializable]
    public class AbilityDefinitionSet : DefinitionSet<AbilityDefinition>
    {
        public AbilityDefinitionSet(string fullPath, string filename, bool scramble = false, bool encode = false, string extension = "") : base(fullPath, filename, scramble, encode, extension)
        {
        }

        #region Save
        public void Optimize()
        {
            for (int i = this._content.Count - 1;i >= 0;i--)
            {
                var element = this._content[i];
                if (string.IsNullOrEmpty(element.id))
                {
                    this._content.RemoveAt(i);
                    continue;
                }

                element.displayNameId = (string.IsNullOrEmpty(element.displayNameId)) ? element.id.ToTitle() : element.displayNameId;

                element.baseAbilities.RemoveAll(x => string.IsNullOrEmpty(x) || x == element.id);
            }

            DataUtility.OptimizeDefinitions(this._content);
        }

        public override void Save()
        {
            Optimize();
            base.Save();
        }
        #endregion
    }

    [System.Serializable]
    public class AbilityDataSet : DataSet<string, AbilityData>
    {
        public AbilityDataSet(DefinitionSet<AbilityDefinition> abilityDefinitionSet)
        {
            Initialize(abilityDefinitionSet.GetContent());
        }

        public AbilityDataSet(List<AbilityDefinition> _content)
        {
            Initialize(_content);
        }

        /// <summary>
        /// Topmost Abilities.
        /// </summary>
        public List<AbilityData> rootAbilities = new List<AbilityData>();

        /// <summary>
        /// Bottommost Abilities.
        /// </summary>
        public List<AbilityData> leafAbilities = new List<AbilityData>();

        public bool Initialize(List<AbilityDefinition> abilityDefinitions)
        {
            if (!this.IsInitialized)
            {
                this._contentDictionary = new Dictionary<string, AbilityData>(abilityDefinitions.Count);

                foreach (var abilityDefinition in abilityDefinitions)
                {
                    AbilityData data = new AbilityData(abilityDefinition);

                    //list
                    this._content.Add(data);

                    //dictionary
                    this._contentDictionary[data.id] = data;

                    //
                    if (abilityDefinition.baseAbilities.Count == 0)
                    {
                        this.rootAbilities.Add(data);
                    }
                }

                Initialize();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Data-Crunching Initialization
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            if (!this.IsInitialized)
            {
                //find subcategories
                foreach (var data in this._content)
                {
                    foreach (var baseAbility in data.Definition.baseAbilities)
                    {
                        AbilityData baseData;
                        if (this._contentDictionary.TryGetValue(baseAbility, out baseData))
                        {
                            baseData.AddSubability(data);
                        }

                        data.AddSuperability(baseData);
                    }
                }

                //find leafs
                foreach (var data in this._content)
                {
                    if (data._subabilities.Count == 0)
                        this.leafAbilities.Add(data);
                }

                //Propagate rootcategories
                foreach (var data in this.rootAbilities)
                {
                    foreach (var sub in data._subabilities)
                    {
                        sub.AddSuperability(data);
                    }
                }

                //Propagate leafs
                foreach (var data in this.leafAbilities)
                {
                    foreach (var super in data._superabilities)
                    {
                        super.AddSubability(data);
                    }
                }

                foreach (var data in this._content)
                {
                    data.descriptor.id = data.id;
                    foreach (var sub in data._subabilities)
                    {
                        data.descriptor.subabilityIds.Add(sub.id);
                    }
                    foreach (var sup in data._superabilities)
                    {
                        data.descriptor.superabilityIds.Add(sup.id);
                    }
                }

                //PofyTools.UI.NotificationView.Show("Game Definitions Initialized!", null, -1f);
                this.IsInitialized = true;
                return true;
            }
            return false;
        }

        public List<AbilityData.Descriptor> GetDescriptors()
        {
            List<AbilityData.Descriptor> result = new List<AbilityData.Descriptor>(this._content.Count);

            foreach (var data in this._content)
            {
                result.Add(data.descriptor);
            }

            return result;
        }
    }

}
