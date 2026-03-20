using System;
using System.Collections.Generic;
using System.Text;

namespace ETMS.Application.Interfaces
{
    public interface IUrlEncryptionService
    {
        string Encrypt(int id);
        int Decrypt(string encryptedId);
    }
}