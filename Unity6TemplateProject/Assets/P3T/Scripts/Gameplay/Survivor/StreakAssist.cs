using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    /// Assists in making streaks more evenly distributed by their given weights.
    /// This script adjusts the weights based on the last power up chosen so the same power up isn't continuously chosen
    /// </summary>
    public class StreakAssist : MonoBehaviour
    {
        private List<Item> _itemsList = new();
        private List<Item> ValidItems => _itemsList.Where(item => item.ValidOption).ToList();
        private int _weightDecrease = 50;
        private int _weightIncrease = 20;
        private Item _defaultItem; //If none are chosen, default to use this one

        public void SetUp(List<Item> items, int decrease, int increase, Item defaultItem = null)
        {
            _itemsList = items;
            foreach (var item in _itemsList)
            {
                item.ValidOption = true;
            }
            _weightDecrease = decrease;
            _weightIncrease = increase;
            if (defaultItem != null) _defaultItem = defaultItem;
        }
    
        //When an item has been chosen, adjust weights accordingly
        public void ValueSelected(Item selectedGameItem)
        {
            //Find value in list and adjust its percent
            var selectedItem = _itemsList.Find(x => x == selectedGameItem);
            if (selectedItem != null)
            {
                selectedItem.Weight -= _weightDecrease; //Let weight go into negatives so its longer until chosen again
                // selectedItem.Weight = Mathf.Max(0, selectedItem.Weight -= weightDecrease);
            }
        
            RaiseWeightsOnOtherItems(selectedItem);
        }

        /// <summary>
        /// Increase other items if not at their max already
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        private void RaiseWeightsOnOtherItems(Item selectedItem)
        {
            foreach (var item in _itemsList)
            {
                if (item == selectedItem) continue;
                item.Weight = Mathf.Min(item.Weight += _weightIncrease, item.MaxWeight);
            }
        }

        //Add items individually instead of through setup
        public void AddItem(GameObject gameItem, int weight, int maxWeight)
        {
            _itemsList.Add(new Item(gameItem, weight, maxWeight));
        }

        //Randomly pick object - taking into account their weights
        public Item GetItem()
        {
            var totalWeights = 0;
            foreach (var power in ValidItems) totalWeights += power.Weight;
            if (totalWeights > 0) // its possible all options are negative values
            {
                var rand = Random.Range(0, totalWeights);
                var sum = 0;
                foreach (var item in ValidItems)
                {
                    //Loop until less than cumulative probability
                    if (rand <= (sum += item.Weight))
                    {
                        //Adjust weights since now has been selected
                        ValueSelected(item);
                        return item;
                    }
                }
            }
        
            if (_defaultItem == null) return null;
            // Default item may be selected, and weights still need to be raised for all other items
            RaiseWeightsOnOtherItems(_defaultItem);
            return _defaultItem.ValidOption ? _defaultItem : null;
        }

        /// <summary>
        /// Invalidate an item so it can no longer be chosen
        /// </summary>
        /// <param name="itemGameObject"></param>
        public void InvalidateItem(GameObject itemGameObject)
        {
            var selectedItem = _itemsList.Find(x => x.GameItem == itemGameObject);
            if (selectedItem != null)
            {
                selectedItem.ValidOption = false;
            }
        }

        /// <summary>
        /// Make an item valid again
        /// </summary>
        /// <returns></returns>
        public void ItemValidAgain(GameObject itemGameObject)
        {
            var selectedItem = _itemsList.Find(x => x.GameItem == itemGameObject);
            if (selectedItem != null)
            {
                selectedItem.ValidOption = true;
            }
        }
    
        [Serializable]
        public class Item
        {
            public GameObject GameItem;
            public int Weight;
            public int MaxWeight;
            /// <summary>
            /// Used for toggling an item
            /// </summary>
            private bool _enabledOption = true;
            public bool ValidOption
            {
                get => _enabledOption;
                set => _enabledOption = value;
            }

            public Item(GameObject gameItem, int weight, int maxWeight)
            {
                GameItem = gameItem;
                Weight = weight;
                MaxWeight = maxWeight;
                ValidOption = true;
            }
        }
    }
}
