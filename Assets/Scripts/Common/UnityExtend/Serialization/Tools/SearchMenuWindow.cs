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
    private Paging<MenuItem> _pages = null;
    private Vector2 _scrollPosition;
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

    private void OnGUI()
    {
        if (_pages == null) return;
        var searchText = GUILayout.TextField(_searchText);
        if(searchText != _searchText) {
            _searchText = searchText;
            UpdatePages();
        }
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.BeginVertical();
        foreach (var item in _pages.GetPageItems())
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(item.title)))
            {
                ApplySelectedItem(item);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
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
            _pages = new Paging<MenuItem>(_menuItems, 10);
        }
        else
        {
            var regex = CreateSearchRegex(_searchText);
            _pages = new Paging<MenuItem>(_menuItems.Where(i => regex.IsMatch(i.title)).ToList(), 10);
        }
    }
    private static Regex CreateSearchRegex(string str)
    {
        return new(string.Join(".*", str.Split(" ")), RegexOptions.IgnoreCase);
    }
}