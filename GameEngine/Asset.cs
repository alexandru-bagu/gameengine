using System;

namespace GameEngine
{
    public abstract class Asset : IDisposable
    {
        public int Id { get; protected set; }
        public abstract void Dispose();
    }
}
