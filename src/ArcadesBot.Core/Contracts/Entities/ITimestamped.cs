using System;

namespace ArcadesBot
{
    public interface ITimestamped
    {
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
    }
}
