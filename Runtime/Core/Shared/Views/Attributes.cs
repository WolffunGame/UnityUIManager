using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public interface IAttributes
    {
        object Get(Type type);

        T Get<T>();

        void Add(Type type, object target);

        void Add<T>(T target);

        object Remove(Type type);

        T Remove<T>();

        IEnumerator GetEnumerator();

    }

    public class Attributes : IAttributes
    {
        private Dictionary<Type, object> attributes = null;

        public virtual void Add(Type type, object target)
        {
            if (attributes == null)
                attributes = new Dictionary<Type, object>();

            if (type == null || target == null)
                return;

            attributes[type] = target;
        }

        public virtual void Add<T>(T target)
        {
            Add(typeof(T), target);
        }

        public virtual object Get(Type type)
        {
            if (type == null || attributes == null || !attributes.ContainsKey(type))
                return null;

            return attributes[type];
        }

        public virtual T Get<T>()
        {
            return (T) Get(typeof(T));
        }

        public virtual object Remove(Type type)
        {
            if (type == null || attributes == null || !attributes.ContainsKey(type))
                return null;

            object target = attributes[type];
            attributes.Remove(type);
            return target;
        }

        public virtual T Remove<T>()
        {
            return (T) Remove(typeof(T));
        }

        public virtual IEnumerator GetEnumerator()
        {
            if (attributes == null)
                return new EmptyEnumerator();

            return attributes.GetEnumerator();
        }

        class EmptyEnumerator : IEnumerator
        {
            public object Current
            {
                get { return null; }
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }
        }
    }
}