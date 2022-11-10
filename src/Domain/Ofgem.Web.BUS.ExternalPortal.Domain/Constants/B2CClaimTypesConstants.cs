using System.Collections.Immutable;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Constants
{
    public static class B2CClaimTypesConstants
    {
        /// <summary>
        /// ClaimType of ExternalUserId returned from B2C
        /// </summary>
        public const string ClaimTypeExternalUserId = "extension_BUS_BusinessAccountExternalUserId";

        /// <summary>
        /// ClaimType of BusinessAccountId returned from B2C
        /// </summary>
        public const string ClaimTypeBusinessAccountId = "extension_BUS_BusinessAccountId";

        /// <summary>
        /// ClaimType of signInNames.emailAddress returned from B2C
        /// </summary>
        public const string ClaimTypeEmailAddress = "signInNames.emailAddress";

        /// <summary>
        /// ClaimType of action returned from B2C
        /// </summary>
        public const string ClaimTypeAction = "action";

        /// <summary>
        /// Constant for signup redirect path
        /// </summary>
        public const string SignUpRedirectPath = "/sign-up-complete";

        /// <summary>
        /// Constant for signinfirsttime redirect path
        /// </summary>
        public const string SignInFirstTimeRedirectPath = "/sign-up-complete";

        /// <summary>
        /// Constant for SignUp authentication action
        /// </summary>
        public const string SignUpAction = "signup";

        /// <summary>
        /// Constant for SignIn authentication action
        /// </summary>
        public const string SignInAction = "signin";

        /// <summary>
        /// Constant for SignInFirstTime authentication action
        /// </summary>
        public const string SignInFirstTimeAction = "signinfirsttime";
    }
}
