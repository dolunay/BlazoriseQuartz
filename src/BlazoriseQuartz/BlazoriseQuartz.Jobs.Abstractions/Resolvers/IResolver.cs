using System;
namespace BlazoriseQuartz.Jobs.Abstractions.Resolvers
{
    public interface IResolver
    {
        string Resolve(string varBlock);
    }
}

