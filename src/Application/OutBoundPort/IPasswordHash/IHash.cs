using System;
using System.Collections.Generic;
using System.Text;

namespace Application.OutBoundPort.IPasswordHash
{
    public interface IHash
    {
        string Hash(string password);
    }
}
