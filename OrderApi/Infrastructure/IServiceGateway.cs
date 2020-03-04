using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Infrastructure
{
    public interface IServiceGateway<T>
    {
        T Get(int id);
    }
}
