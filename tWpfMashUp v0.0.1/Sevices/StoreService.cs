using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using tWpfMashUp_v0._0._1.MVVM.Models;

namespace tWpfMashUp_v0._0._1.Sevices
{
    public class StoreService
    {
        private readonly Dictionary<string, dynamic> storeDictionary;

        public event EventHandler CurrentContactChanged;

        public StoreService() => storeDictionary = new Dictionary<string, dynamic>();
       
        public void Add(string key, dynamic obj)
        {
            if (obj == null) return;
            if (!HasKey(key))
            {
                storeDictionary.Add(key, obj);
            }
            else { storeDictionary[key] = obj; }
        }

        public dynamic Get(string key) => storeDictionary.TryGetValue(key, out var val) ? val : null;
       
        public bool HasKey(string key) => storeDictionary.ContainsKey(key);

        public void Remove(string key) => storeDictionary.Remove(key);

        internal void OnLogOut()
        {
            Remove(CommonKeys.Chats.ToString());
            Remove(CommonKeys.CurrentChat.ToString());
            Remove(CommonKeys.WithUser.ToString());
            Remove(CommonKeys.IsMyTurn.ToString());
        }

        public void InformContactChanged(object source, ChatRecivedEventArgs chatRecivedEventArgs) => CurrentContactChanged?.Invoke(source, chatRecivedEventArgs);


    }
}

