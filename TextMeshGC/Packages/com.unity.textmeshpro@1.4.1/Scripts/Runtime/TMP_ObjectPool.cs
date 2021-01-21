using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace TMPro
{

    internal class TMP_ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return m_Stack.Count; } }

        public TMP_ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }

    internal class TMP_ArrayObjectPool<T>
    {
        /// <summary>
        /// 是否正在使用对象池,后续增加释放机制时使用
        /// </summary>
        private bool m_IsUsingPool = true;
        private readonly Dictionary<int, Stack<T[]>> m_StackDic = new Dictionary<int, Stack<T[]>>();
        private readonly UnityAction<T[]> m_ActionOnGet;
        private readonly UnityAction<T[]> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive
        {
            get
            {
                int count = 0;
                foreach (var stack in m_StackDic.Values)
                {
                    count += stack.Count;
                }
                return count;
            }
        }

        public TMP_ArrayObjectPool(UnityAction<T[]> actionOnGet, UnityAction<T[]> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T[] Get(int count)
        {
            T[] element;
            if (m_IsUsingPool)
            {
                if (m_StackDic.TryGetValue(count, out Stack<T[]> stack) == false)
                {
                    stack = new Stack<T[]>();
                    m_StackDic[count] = stack;
                }

                if (stack.Count == 0)
                {
                    element = new T[count];
                    countAll++;
                }
                else
                {
                    element = stack.Pop();
                }
                if (m_ActionOnGet != null)
                    m_ActionOnGet(element);
            }
            else
            {
                element = new T[count];
            }
            return element;
        }

        public void Release(int count, T[] element)
        {
            if (m_IsUsingPool)
            {
                if (m_StackDic.TryGetValue(count, out Stack<T[]> stack) == false)
                {
                    stack = new Stack<T[]>();
                    m_StackDic[count] = stack;
                }
                if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
                    Debug.LogErrorFormat("Internal error. Trying to destroy object that is already released to pool. count = {0}", count.ToString());
                if (m_ActionOnRelease != null)
                    m_ActionOnRelease(element);
                stack.Push(element);
            }
            else
            {

            }
        }
    }
}