using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SmbcApp.LearnGame.UIWidgets.ScrollView
{
    public sealed class UIScrollView : MonoBehaviour
    {
        [SerializeField] [Required] private Transform content;
        [SerializeField] [Required] private GameObject dividerPrefab;

        private readonly List<Item> _items = new();

        public void AddItem(Transform item, bool addDivider = false)
        {
            if (addDivider && _items.Any())
            {
                var divider = Instantiate(dividerPrefab, content).transform;
                divider.SetParent(content, false);
                _items.Add(new Item(item, divider));
            }
            else
            {
                _items.Add(new Item(item));
            }

            item.SetParent(content, false);
        }

        public void RemoveItem(Transform item)
        {
            var targetItem = _items.Find(v => v.Content == item);
            if (targetItem != null)
            {
                Destroy(targetItem.Content.gameObject);
                if (targetItem.Divider != null)
                    Destroy(targetItem.Divider.gameObject);
                _items.Remove(targetItem);
            }
            else
            {
                throw new Exception("Item not found");
            }
        }

        public void Clear()
        {
            foreach (var child in _items)
            {
                Destroy(child.Content.gameObject);
                if (child.Divider != null)
                    Destroy(child.Divider.gameObject);
            }

            _items.Clear();
        }

        private class Item
        {
            public readonly Transform Content;
            public readonly Transform Divider;

            public Item(Transform content, Transform divider)
            {
                Content = content;
                Divider = divider;
            }

            public Item(Transform content)
            {
                Content = content;
                Divider = null;
            }
        }
    }
}