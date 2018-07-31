namespace PofyTools.Local
{
    #pragma  warning disable 219
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class LocalizationDataAddLanguagePopup : PopupWindowContent
    {
        private string _newLanguage = "XX";
        private string _info = "New language ALPHA2 kek has to unique and two characters long.";
        private MessageType _infoType = MessageType.Info;
        private EditorWindow _target;

        //    public static void Init()
        //    {
        //        LocalizationDataAddEntryPopup window = ScriptableObject.CreateInstance<LocalizationDataAddEntryPopup>();
        //        //window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        //        window.position = new Rect(EditorWindow.focusedWindow.position.x, EditorWindow.focusedWindow.position.y, 250, 150);
        //
        //        window._target = EditorWindow.focusedWindow;
        //        if (!(window._target is LocalizationDataEditor))
        //            window._target = null;
        //
        //        window.ShowPopup();
        //    }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(500, 700);
        }

        private Localization.LanguageData _dataToDelete = null;
        private Vector2 _scrollPos;

        public override void OnGUI(Rect rect)
        {
        
            EditorGUILayout.LabelField("Add New Language", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Enter new language ALPHA2 key:");
            this._newLanguage = EditorGUILayout.TextField(this._newLanguage);
            this._newLanguage.ToUpper();
            if (this._newLanguage.Length > 2)
            {
                this._newLanguage = this._newLanguage.Substring(0, 2);
            }
            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox(this._info, this._infoType);

            EditorGUILayout.BeginHorizontal();

            this._scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos);
            foreach (var data in Localization.GetData())
            {
                if (GUILayout.Button("x " + data.languageKey))
                {
                    this._dataToDelete = data;
                }
            }
            EditorGUILayout.EndScrollView();

            if (this._dataToDelete != null)
            {
                if (EditorUtility.DisplayDialog("Delete Language Data", "Ae you sure you want to delete data for \"" + this._dataToDelete + "\". This action can not be undone?", "Delete", "Cancel"))
                {
                    Localization.RemoveLanguageData(this._dataToDelete.languageKey);
                    Localization.SaveData();
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Add"))
            {
                this.AddLanguage();
            }
            EditorGUILayout.EndHorizontal();
        }

        void AddLanguage()
        {
            var allData = Localization.GetData();
            var upper = this._newLanguage.ToUpper();

            if (Localization.HasLanguage(upper))
            {
                this._info = "The language key \"" + upper + "\" already present in localization data. Choose different language key.";
                this._infoType = MessageType.Error;
            }
            else
            {
                Localization.AddLanguage(upper);
                this._info = "The language key \"" + upper + "\" successfully added!";
                this._infoType = MessageType.Info;
                Localization.SaveData();
                AssetDatabase.Refresh();
            }
        }
    }
}