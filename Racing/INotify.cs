using System;

namespace Racing
{
    public interface INotify
    {
        void Notify(String ntf, Ntf type);
    }

    public enum Ntf
    {
        MESSAGE,
        WARNING,
        ERROR,
    }
}
