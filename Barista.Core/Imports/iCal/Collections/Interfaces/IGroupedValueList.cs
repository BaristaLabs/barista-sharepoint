using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DDay.Collections
{
    public interface IGroupedValueList<TGroup, TInterface, TItem, TValueType> :
        IGroupedValueCollection<TGroup, TInterface, TItem, TValueType>,
        IGroupedList<TGroup, TInterface>
        where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
        where TItem : new()
    {        
    }
}
