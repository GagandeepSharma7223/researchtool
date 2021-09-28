using System;
using System.Collections.Generic;
using System.Text;

namespace chapterone.data
{
    public class SchemaMismatchException : Exception
    {
        public SchemaMismatchException(string collectionName, long mismatchCount)
            : base($"'{collectionName}' collection has {mismatchCount} mismatches")
        {
        }

        public SchemaMismatchException(string collectionName, string message)
            : base($"Schema mismatch in '{collectionName}': {message}")
        {
        }

        public SchemaMismatchException(string collectionName)
            : base($"Schema mismatch in '{collectionName}'")
        {
        }
    }
}
