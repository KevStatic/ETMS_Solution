<<<<<<< HEAD
﻿using System.Threading.Tasks;
=======
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
<<<<<<< HEAD
        Task<User> GetByUsernameAsync(string username);
    }
}
=======
        Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
