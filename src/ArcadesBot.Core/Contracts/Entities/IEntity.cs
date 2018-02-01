namespace ArcadesBot
{
    public interface IEntity<TId>
    {
        TId Id { get; }
    }
}
