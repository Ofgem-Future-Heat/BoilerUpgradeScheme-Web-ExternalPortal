namespace Ofgem.Web.BUS.ExternalPortal.Core.Utilities
{
    public static class B2CUtilities
    {
        /// <summary>
        /// Validates the provided ADB2C Id for null/empty string
        /// </summary>
        /// <param name="adb2cId">adb2cId to be validated</param>
        /// <returns>True/False flag (success/failure)</returns>
        public static bool IsValidAdb2cId(string adb2cId)
        {
            if (!string.IsNullOrWhiteSpace(adb2cId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates the provided businessAccountId for null/empty string
        /// </summary>
        /// <param name="businessAccountId">businessAccountId to be validated</param>
        /// <returns>True/False flag (success/failure)</returns>
        public static bool IsValidBusinessAccountId(string businessAccountId)
        {
            if (!string.IsNullOrWhiteSpace(businessAccountId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates the provided externalUserId for null/empty string
        /// </summary>
        /// <param name="externalUserId">externalUserId to be validated</param>
        /// <returns>True/False flag (success/failure)</returns>
        public static bool IsValidExternalUserId(string externalUserId)
        {
            if (!string.IsNullOrWhiteSpace(externalUserId))
            {
                return true;
            }
            return false;
        }
    }
}
