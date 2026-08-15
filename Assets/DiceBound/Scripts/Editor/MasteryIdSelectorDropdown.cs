using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DiceBound.Editor
{
    public class MasteryIdSelectorDropdown : AdvancedDropdown
    {
        private readonly Action<string> _onSelected;
        private List<string> _ids = new List<string>();

        public MasteryIdSelectorDropdown(AdvancedDropdownState state, Action<string> onSelected) : base(state)
        {
            _onSelected = onSelected;
            minimumSize = new Vector2(220, 300);
        }

        public void Setup(List<string> ids)
        {
            _ids = ids ?? new List<string>();
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Mastery Id");

            foreach (var id in _ids.OrderBy(x => x))
            {
                root.AddChild(new IdItem(id));
            }

            if (!root.children.Any())
            {
                root.AddChild(new AdvancedDropdownItem("No ids found in DT_Mastery"));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is IdItem idItem)
            {
                _onSelected?.Invoke(idItem.Id);
            }
        }

        private class IdItem : AdvancedDropdownItem
        {
            public string Id { get; }

            public IdItem(string id) : base(id)
            {
                Id = id;
            }
        }
    }
}
