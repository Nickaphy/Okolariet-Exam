using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects
{
    // The shared base class for all entities in the domain.
    // It provides an Id property to any entity that inherits the aggregate root.
    public abstract class Entity
    {
        public Guid Id { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        public override bool Equals(object? obj)
            => obj is Entity other && Id == other.Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }

    // The outlet your aggregate root inherits from : Aggregateroot. It provides the Id property to any aggregate root that inherits it.
    public abstract class Aggregateroot : Entity
    {

    }
}
