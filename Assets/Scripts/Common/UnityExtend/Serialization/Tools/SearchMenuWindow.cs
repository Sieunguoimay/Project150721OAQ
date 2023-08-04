#if UNITY_EDITOR
using Common.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public partial class SearchMenuWindow : EditorWindow
{
    private List<MenuItem> _menuItems = null;
    private Paging<MenuItem> _paging = null;
    private Vector2 _scrollPosition;
    private Vector2 _previousWindowSize;
    private bool _closeOnSelect;
    private string _searchText;

    private class MenuItem
    {
        public string title;
        public object data;
        public Action<object> onSelected;
    }

    public static SearchMenuWindow Create(string title = "SearchMenuWindow", bool closeOnSelect = false)
    {
        var window = GetWindow<SearchMenuWindow>(title);
        window._menuItems = new List<MenuItem>();
        window._closeOnSelect = closeOnSelect;
        return window;
    }
    public void AddItem(string title, Action<object> onSelected, object data)
    {
        _menuItems.Add(new MenuItem
        {
            data = data,
            title = title,
            onSelected = onSelected,
        });
    }

    public void ShowMenu()
    {
        UpdatePages();
        ShowAuxWindow();
    }
    private void OnEnable()
    {
        _previousWindowSize = this.position.size;
    }

    private void OnGUI()
    {
        if (_paging == null) return;

        WindowSizeChangeDetect();
        
        var searchText = GUILayout.TextField(_searchText);
        if (searchText != _searchText)
        {
            _searchText = searchText;
            UpdatePages();
        }
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.BeginVertical();
        foreach (var item in _paging.GetPageItems())
        {
            EditorGUILayout.BeginHorizontal();
            DrawMenuItem(item);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        
        DrawPagingNavigation();
        GUILayout.FlexibleSpace();
    }
    private void WindowSizeChangeDetect()
    {
        if (_previousWindowSize != this.position.size)
        {
            _previousWindowSize = this.position.size;
            //Debug.Log("Window resized! New size: " + _previousWindowSize);
        }
    }
    private void DrawMenuItem(MenuItem item)
    {
        GUIStyle buttonStyle = GUI.skin.button;
        buttonStyle.alignment = TextAnchor.MiddleLeft;
        if (GUILayout.Button(new GUIContent(item.title), buttonStyle))
        {
            ApplySelectedItem(item);
        }
    }
    private void DrawPagingNavigation()
    {
        var buttonWidth = GUILayout.Width(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        var leftClicked = GUILayout.Button("<", buttonWidth);
        GUILayout.Label(new GUIContent($"{_paging.PageIndex + 1}/{_paging.PageNum}"));
        var rightClicked = GUILayout.Button(">", buttonWidth);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (leftClicked)
        {
            _paging.PrevPage();
        }
        if (rightClicked)
        {
            _paging.NextPage();
        }
    }
    private void ApplySelectedItem(MenuItem item)
    {
        item.onSelected?.Invoke(item.data);
        if (_closeOnSelect)
        {
            Close();
        }
    }
    private void UpdatePages()
    {
        if (string.IsNullOrEmpty(_searchText))
        {
            _paging = new Paging<MenuItem>(_menuItems, 20);
        }
        else
        {
            var regex = CreateSearchRegex(_searchText);
            _paging = new Paging<MenuItem>(_menuItems.Where(i => regex.IsMatch(i.title)).ToList(), 20);
        }
    }
    private static Regex CreateSearchRegex(string str)
    {
        return new(string.Join(".*", str.Split(" ")), RegexOptions.IgnoreCase);
    }
}
#endif