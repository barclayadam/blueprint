using System;

namespace Blueprint.Core.Apm
{
    internal class NulloApmDependencyOperation : IApmDependencyOperation
    {
        internal static readonly NulloApmDependencyOperation Instance = new NulloApmDependencyOperation();

        public void MarkSuccess(string resultCode)
        {
        }

        public void MarkFailure(string resultCode, Exception exception = null)
        {
        }
    }
}