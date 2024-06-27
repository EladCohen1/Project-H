using System.Collections.Generic;
using System.Linq;

public class MultiLock<Locker>
{
    public List<Locker> list;
    public MultiLock()
    {
        list = new List<Locker>();
    }
    public void Lock(Locker locker)
    {
        if (!list.Contains(locker))
        {
            list.Add(locker);
        }
    }
    public void Unlock(Locker locker)
    {
        if (list.Contains(locker))
        {
            list.Remove(locker);
        }
    }
    public bool IsFree()
    {
        return !list.Any();
    }
}

// locker example

// enum MoveLocker
// {
//     WallSlide,
//     wallHop
// }