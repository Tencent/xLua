/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace XLua
{
    public class ObjectPool
    {
        const int LIST_END = -1;
        const int ALLOCED = -2;
        struct Slot
        {
            public int next;
            public object obj;

            public Slot(int next, object obj)
            {
                this.next = next;
                this.obj = obj;
            }
        }

        private Slot[] list = new Slot[512];
        private int freelist = LIST_END;
        private int count = 0;

        public object this[int i]
        {
            get
            {
                if (i >= 0 && i < count)
                {
                    return list[i].obj;
                }

                return null;
            }
        }

        public void Clear()
        {
            freelist = LIST_END;
            count = 0;
            list = new Slot[512];
        }

        void extend_capacity()
        {
            Slot[] new_list = new Slot[list.Length * 2];
            for (int i = 0; i < list.Length; i++)
            {
                new_list[i] = list[i];
            }
            list = new_list;
        }

        public int Add(object obj)
        {
            int index = LIST_END;

            if (freelist != LIST_END)
            {
                index = freelist;
                list[index].obj = obj;
                freelist = list[index].next;
                list[index].next = ALLOCED;
            }
            else
            {
                if (count == list.Length)
                {
                    extend_capacity();
                }
                index = count;
                list[index] = new Slot(ALLOCED, obj);
                count = index + 1;
            }

            return index;
        }

        public bool TryGetValue(int index, out object obj)
        {
            if (index >= 0 && index < count && list[index].next == ALLOCED)
            {
                obj = list[index].obj;
                return true;
            }

            obj = null;
            return false;
        }

        public object Get(int index)
        {
            if (index >= 0 && index < count)
            {
                return list[index].obj;
            }
            return null;
        }

        public object Remove(int index)
        {
            if (index >= 0 && index < count && list[index].next == ALLOCED)
            {
                object o = list[index].obj;
                list[index].obj = null;
                list[index].next = freelist;
                freelist = index;
                return o;
            }

            return null;
        }

        public object Replace(int index, object o)
        {
            if (index >= 0 && index < count)
            {
                object obj = list[index].obj;
                list[index].obj = o;
                return obj;
            }

            return null;
        }

        public int Check(int check_pos, int max_check, Func<object, bool> checker, Dictionary<object, int> reverse_map)
        {
            if (count == 0)
            {
                return 0;
            }
            for (int i = 0; i < Math.Min(max_check, count); ++i)
            {
                check_pos %= count;
                if (list[check_pos].next == ALLOCED && !Object.ReferenceEquals(list[check_pos].obj, null))
                {
                    if (!checker(list[check_pos].obj))
                    {
                        object obj = Replace(check_pos, null);
                        int obj_index;
                        if (reverse_map.TryGetValue(obj, out obj_index) && obj_index == check_pos)
                        {
                            reverse_map.Remove(obj);
                        }
                    }
                }
                ++check_pos;
            }

            return check_pos %= count;
        }

#if OBJECT_POOL_STAT
        public void Stat()
        {

            Hashtable ht = new Hashtable();
            int obj_num = 0;
            for (int i = 0; i < count; ++i)
            {
                if (list[i].next == -2 && !Object.ReferenceEquals(list[i].obj, null))
                {
                    Type type = list[i].obj.GetType();
                    ht[type] = ht[type] == null ? 1 : (int)ht[type] + 1;
                    ++obj_num;
                }
            }

            UnityEngine.Debug.Log("ObjectPool.Count = " + obj_num);
            foreach (var key in ht.Keys)
            {
                UnityEngine.Debug.Log("type:" + key + ", num:" + ht[key]);
            }
        }
#endif
    }
}