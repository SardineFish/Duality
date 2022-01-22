using System;
using System.Collections.Generic;
using UnityEngine;

namespace SardineFish.Utils
{
    public interface IKeyValuePairEditorEx
    {
    }

    public interface ISerializableDictionary
    {
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializableDictionary, ISerializationCallbackReceiver
    {
        [Serializable]
        struct KeyValuePair : IKeyValuePairEditorEx
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        private List<KeyValuePair> pairs = new List<KeyValuePair>();


        public void OnBeforeSerialize()
        {
            pairs.Clear();
            pairs.Capacity = this.Count;
            foreach (var pair in this)
            {
                pairs.Add(new KeyValuePair()
                {
                    key = pair.Key,
                    value = pair.Value
                });
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();
            foreach (var pair in this.pairs)
            {
                if (this.ContainsKey(pair.key))
                {
                    TKey key = default;
                    this.Add(key, pair.value);
                }
                else
                    this[pair.key] = pair.value;
            }
        }
    }
}