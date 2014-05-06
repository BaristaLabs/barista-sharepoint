using System;
using System.Collections.Generic;
using System.Text;

namespace Barista.DDay.Collections
{
    public interface IGroupedObject<TGroup>
    {
        event EventHandler<ObjectEventArgs<TGroup, TGroup>> GroupChanged;
        TGroup Group { get; set; }
    }
}
