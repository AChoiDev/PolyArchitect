using System.Collections.Generic;
using System.Linq;

namespace PolyArchitect.Core {

// A class that maps integer IDs to objects
public class Registry<T> {

    private int objRegisteredCount = 0;
    private readonly Dictionary<int, T> IDToObj = new();
    public Registry() {

    }
    public virtual int Add(T obj) {
        IDToObj.Add(objRegisteredCount, obj);
        objRegisteredCount += 1;
        return objRegisteredCount - 1;
    }

    public virtual void Remove(int ID) {
        IDToObj.Remove(ID);
    }

    public T Get(int ID) {
        return IDToObj[ID];
    }


    public HashSet<T> GetValues() {
        return IDToObj.Values.ToHashSet();
    }
    public HashSet<int> GetIDs() {
        return IDToObj.Keys.ToHashSet();
    }

    public int Count => IDToObj.Count;
}
}