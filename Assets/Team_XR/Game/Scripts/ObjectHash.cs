using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ObjectHash<T>
{
    T[] m_objectArray;
    Dictionary<string, int> m_dict;

    ObjectHash(T[] array) {
  
        m_dict = new Dictionary<string, int>();
        m_objectArray = array;
        for (int i = 0; i < array.Length; i++)
        {
            
            m_dict.Add(
            ((array[i]) as Object).name, i);
        }
    }

    static public ObjectHash<T> GetStringDictionaryFromArray(T[] array){
        if (array.Length > 0 && array[0] is Object ){
            return new ObjectHash<T>(array);
        }

        return null;
    }

    public T this[string key] {
        get {
            if (m_dict.ContainsKey (key))
                return m_objectArray [m_dict[key]];
            return default(T);
        }

    }
}