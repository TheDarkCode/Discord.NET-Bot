namespace ArcadesBot
{
    public interface IOwnable<TId>
    {
        TId OwnerId { get; }
    }
}
