using System;

namespace Data.Models;

public interface IIdentifiable
{
    public Guid Id { get; set; }

}
