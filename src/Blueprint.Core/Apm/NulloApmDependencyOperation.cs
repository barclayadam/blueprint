using System;

namespace Blueprint.Core.Apm
{
    public class NulloApmDependencyOperation : IApmDependencyOperation
    {
        public static readonly NulloApmDependencyOperation Instance = new NulloApmDependencyOperation();

        public void MarkSuccess(string resultCode)
        {
        }

        public void MarkFailure(string resultCode, Exception exception = null)
        {
        }
    }
}
