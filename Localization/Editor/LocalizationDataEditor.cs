namespace PofyTools.Local
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;


    public class LocalizationDataEditor : EditorWindow
    {
        private string[] _languages = null;
        private string _selectedLanguage = "";
        private int _selectedLanguageIndex = -1;
        private Localization.LanguageData _data;

        [MenuItem("PofyTools/Localization Data Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one
            LocalizationDataEditor window = (LocalizationDataEditor)EditorWindow.GetWindow(typeof(LocalizationDataEditor));
            window.ReadData();
            window.Show();
        }

        public void ReadData()
        {
            Localization.Initialize();
            this._languages = Localization.GetLanguages();

            //Select first language (EN)
            if (this._languages.Length > 0)
            {
                this._selectedLanguage = this._languages[Mathf.Max(this._selectedLanguageIndex, 0)];
                this._data = Localization.SetLanguage(this._selectedLanguage);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Data"))
            {
                if (this._dirty)
                {
                    if (EditorUtility.DisplayDialog("Changes not saved", "Discard changes?", "Discard", "Cancel"))
                    {
                        ReadData();
                        this._dirty = false;
                    }
                }
                else
                {
                    ReadData();
                    this._dirty = false;
                }

            }

            if (GUILayout.Button("Save Data"))
            {
                Localization.SaveData();
                AssetDatabase.Refresh();
                this._dirty = false;
            }

            if (GUILayout.Button("Import Excel File"))
            {
                //File browser / other stuff
            }
            EditorGUILayout.EndHorizontal();


            if (this._data == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            int lastIndex = this._selectedLanguageIndex;

            EditorGUILayout.LabelField("Select language:");
            this._selectedLanguageIndex = EditorGUILayout.Popup(this._selectedLanguageIndex, this._languages);

            if (GUILayout.Button("Add Language"))
            {
                AddLanguage();
            }
            EditorGUILayout.EndHorizontal();

            if (this._selectedLanguageIndex >= 0)
            {
                if (lastIndex != this._selectedLanguageIndex)
                {
                    this._selectedLanguage = this._languages[this._selectedLanguageIndex];
                    this._data = Localization.SetLanguage(this._selectedLanguage);
                }

                DrawPairs();
                if (GUILayout.Button("Add Entry"))
                {
                    AddPair();
                }
            }
        }

        private Vector2 _scrollPosition;

        void DrawPairs()
        {
            int indexToRemove = -1;
            this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
            for (int i = 0; i < this._data.keys.Count; i++)
            {
                var lastKey = this._data.keys[i];
                var lastValue = this._data.values[i];

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Remove"/*, GUILayout.Width(25)*/))
                {
                    indexToRemove = i;
                }

                this._data.keys[i] = EditorGUILayout.TextField(this._data.keys[i]);
                this._data.values[i] = EditorGUILayout.TextField(this._data.values[i]);

                if (lastKey != this._data.keys[i] || lastValue != this._data.values[i])
                {
                    EditorUtility.SetDirty(this);
                    this._dirty = true;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (indexToRemove != -1)
            {
                RemovePairAt(indexToRemove);
            }

            EditorGUILayout.EndScrollView();
        }

        private Rect _popupRect;

        void AddPair()
        {
            var popup = new LocalizationDataAddEntryPopup();
            //popup.editorWindow = this;
            PopupWindow.Show(this._popupRect, popup);

            if (Event.current.type == EventType.Repaint)
                this._popupRect = GUILayoutUtility.GetLastRect();
        
            this._dirty = true;
        }

        void AddLanguage()
        {
            if (this._dirty)
            {
                if (EditorUtility.DisplayDialog("Add New Language", "You must save changes before adding a new language?", "Save and Continue", "Cancel"))
                {
                    Localization.SaveData();
                    AssetDatabase.Refresh();
                    this._dirty = false;

                    var popup = new LocalizationDataAddLanguagePopup();
                    //popup.editorWindow = this;
                    PopupWindow.Show(this._popupRect, popup);

                    if (Event.current.type == EventType.Repaint)
                        this._popupRect = GUILayoutUtility.GetLastRect();
                }
            }
            else
            {
                var popup = new LocalizationDataAddLanguagePopup();
                //popup.editorWindow = this;
                PopupWindow.Show(this._popupRect, popup);

                if (Event.current.type == EventType.Repaint)
                    this._popupRect = GUILayoutUtility.GetLastRect();
            }
        }

        void RemovePairAt(int index)
        {
            if (EditorUtility.DisplayDialog("Remove Entry", "Are you sure?", "Remove", "Cancel"))
            {
                Localization.RemovePairAt(index);
                this._dirty = true;
            }
        }

        private bool _dirty = false;

        void OnDestroy()
        {
            if (this._dirty)
            {
                if (EditorUtility.DisplayDialog("Changes not saved", "Save changes?", "Save", "Discard"))
                {
                    Localization.SaveData();
                    AssetDatabase.Refresh();
                    this._dirty = false;
                }
            }
            Localization.Clear();
        }
    }
}