namespace GoldSavings.App;

using System;
using System.Collections.Generic;


    public class RandomizedList<T>
    {
        private readonly List<T> _items;
        private readonly Random _random;

        public RandomizedList()
        {
            _items = new List<T>();
            _random = new Random();
        }

        public void Add(T element)
        {
            int randomChoice = _random.Next(2);

            if (randomChoice == 0)
            {
                _items.Insert(0, element);
            }
            else
            {
                _items.Add(element);
            }
        }

        public T Get(int index)
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            }

            int maxIndex = Math.Min(index, _items.Count - 1);
            int randomIndex = _random.Next(0, maxIndex + 1);

            return _items[randomIndex];
        }

        public bool IsEmpty()
        {
            return _items.Count == 0;
        }

        public override string ToString()
        {
            return string.Join(", ", _items);
        }
    }
