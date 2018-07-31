namespace PofyTools.Local
{
    #pragma  warning disable 219
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class LocalizationDataAddEntryPopup : PopupWindowContent
    {
        private string _newKey = "NEW_ENTRY";
        private string _info = "New key has to unique.";
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
            return new Vector2(200, 150);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Enter new key:");
            this._newKey = EditorGUILayout.TextField(this._newKey);

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox(this._info, this._infoType);

            EditorGUILayout.BeginHorizontal();


            if (GUILayout.Button("Add"))
                this.AddPair();

            EditorGUILayout.EndHorizontal();
        }

        void AddPair()
        {
            var allData = Localization.GetData();

            if (Localization.HasKey(this._newKey))
            {
                this._info = "The key \"" + this._newKey + "\" already present in language data. Choose different key.";
                this._infoType = MessageType.Error;
            }
            else
            {
                Localization.AddPair(this._newKey);
                this._info = "The key \"" + this._newKey + "\" successfully added!";
                this._infoType = MessageType.Info;
            }
        }
    }
}