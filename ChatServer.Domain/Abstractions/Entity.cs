// ReSharper disable ConvertConstructorToMemberInitializers
namespace ChatServer.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; init; }
    public DateTime Updated { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Deleted { get; private set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity()
    {

    }
    
    public void SetCreated() => Created = DateTime.UtcNow;

    public void SetLastModified() => Updated = DateTime.UtcNow;

    public void SetDelete() => Deleted = DateTime.UtcNow;

    public void RevertDelete() => Deleted = null;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id == other.Id; //identifier equality
    }

    public static bool operator ==(Entity a, Entity b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
        {
            return true;
        }
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
        {
            return false;
        }
        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

}