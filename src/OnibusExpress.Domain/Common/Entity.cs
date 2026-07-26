namespace OnibusExpress.Domain.Common;

/// <summary>
/// Base de entidade com identidade por <see cref="Id"/>. Igualdade e hash
/// são derivados da identidade, não dos atributos.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity() => Id = Guid.NewGuid();

    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}
