using System;

namespace CodingMilitia.PlayBall.Auth.Web.Utilities
{
    public interface IBase64QrCodeGenerator
    {
        string Generate(Uri target);
    }
}