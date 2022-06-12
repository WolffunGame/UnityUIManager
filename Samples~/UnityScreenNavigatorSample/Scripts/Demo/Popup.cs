using System;
using UnityEngine;

namespace Demo.Scripts.Demo
{
    public class Popup<T> where T : PopupProperty
    {
        public T Property { get; set; }
        
        public Popup(T property)
        {
            Property = property;
        }
        
        
    }

    public abstract class PopupProperty
    {
        public string Name { get; set; }
    }

    public class Popup1Property : PopupProperty
    {
        public string Age { get; set; }
    }

    public class Demo : MonoBehaviour
    {
        private void Start()
        {
            var property = new Popup1Property()
            {
                Name = "Dat", Age = "16"
            };
            var human = new Popup<PopupProperty>(property);
            
            Debug.Log(human.Property.Name);
        }
    }
}