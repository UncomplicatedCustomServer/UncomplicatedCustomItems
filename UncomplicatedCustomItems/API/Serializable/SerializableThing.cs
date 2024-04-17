using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API.Serializable.Interfaces
{
    public abstract class SerializableThing<T> where T : CustomThing
    {
        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract int Id { get; set; }

        public abstract T Create(Player player);
    }
}
