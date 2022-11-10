namespace Ofgem.Web.BUS.ExternalPortal.Core.Constants
{
    public static class Routes
    {
        public const string CD003 = "heating-type";
        public const string CD004 = "not-eligible";
        public const string CD005 = "find-installation-address";
        public const string CD009 = "new-build";
        public const string CD010 = "self-build";
        public const string CD012 = "confirm-eligibility";
        public const string CD019 = "contacting-property-owner";
        public const string CD020 = "owner-live-at-installation-address";
        public const string CD022 = "quote-details";
        public const string CD026 = "application-received";
        public const string CD029 = "choose-installation-address";
        public const string CD106 = "enter-installation-address";
        public const string CD119 = "cookies";
        public const string CD149 = "application";
        public const string CD150 = "manage-account";
        public const string CD151 = "create-new-application";
        public const string CD152 = "redeem-voucher";
        public const string CD153 = "get-help";
        public const string CD154a = "cancel-application";
        public const string CD154v = "cancel-voucher";
        public const string CD155a = "InstallerApplications";   // ToDo: CD155 url tbc by BA
        public const string CD155n = "InstallerNoApplications";   // ToDo: CD155 url tbc by BA
        public const string CD157 = "biomass-requirements";
        public const string CD158 = "gas-grid";
        public const string CD159 = "Cancel";    // ToDo: CD159 url tbc by BA
        public const string CD161 = "social-housing";
        public const string CD162 = "does-property-have-epc";
        public const string CD164 = "does-epc-have-recommendations";
        public const string CD165 = "exempt-from-epc";
        public const string CD167 = "replacing-fuel-type";
        public const string CD168 = "consent-online";
        public const string CD169 = "consent-Welsh";
        public const string CD171 = "check-your-answers";
        public const string CD172 = "find-property-owner-address";
        public const string CD173 = "choose-owner-address";
        public const string CD174 = "enter-property-owner-address";
        public const string CD175 = "give-feedback";
        public const string CD179 = "shared-loop";
        public const string CD180 = "cannot-find-address";
        public const string CD187 = "there-is-a-problem";
        public const string CD188 = "submit-self-build-evidence";
        public const string CD189 = "submit-epc-evidence";

        public static readonly IReadOnlyCollection<(string page, string route)> RouteList = new List<(string page, string route)>
        {
            (Pages.Path.CD003, CD003),
            (Pages.Path.CD004, CD004),
            (Pages.Path.CD005, CD005),
            (Pages.Path.CD009, CD009),
            (Pages.Path.CD010, CD010),
            (Pages.Path.CD012, CD012),
            (Pages.Path.CD019, CD019),
            (Pages.Path.CD020, CD020),
            (Pages.Path.CD022, CD022),
            (Pages.Path.CD026, CD026),
            (Pages.Path.CD029, CD029),
            (Pages.Path.CD106, CD106),
            (Pages.Path.CD119, CD119),
            (Pages.Path.CD149, CD149),
            (Pages.Path.CD150, CD150),
            (Pages.Path.CD151, CD151),
            (Pages.Path.CD152, CD152),
            (Pages.Path.CD153, CD153),
            (Pages.Path.CD155a, CD155a),
            (Pages.Path.CD155n, CD155n),
            (Pages.Path.CD154a, CD154a), 
            (Pages.Path.CD154v, CD154v), 
            (Pages.Path.CD157, CD157),
            (Pages.Path.CD158, CD158),
            (Pages.Path.CD159, CD159),
            (Pages.Path.CD161, CD161),
            (Pages.Path.CD162, CD162),
            (Pages.Path.CD164, CD164),
            (Pages.Path.CD165, CD165),
            (Pages.Path.CD167, CD167),
            (Pages.Path.CD168, CD168),
            (Pages.Path.CD169, CD169),
            (Pages.Path.CD171, CD171),
            (Pages.Path.CD172, CD172),
            (Pages.Path.CD173, CD173),
            (Pages.Path.CD174, CD174),
            (Pages.Path.CD175, CD175),
            (Pages.Path.CD179, CD179),
            (Pages.Path.CD180, CD180),
            (Pages.Path.CD187, CD187),
            (Pages.Path.CD188, CD188),
            (Pages.Path.CD189, CD189),
        };

        public static class Pages
        {
            public static class Name
            {
                public const string CD003 = "TechType";
                public const string CD004 = "DropOut";
                public const string CD005 = "InstallPostcode";
                public const string CD009 = "NewBuild";
                public const string CD010 = "EligibleBuild";
                public const string CD012 = "TechSpec";
                public const string CD019 = "PropertyOwnerName";
                public const string CD020 = "PropertyOwnerAddressSameAs";
                public const string CD022 = "QuoteDetails";
                public const string CD026 = "Confirmation";
                public const string CD029 = "InstallAddress";
                public const string CD106 = "InstallAddressManual";
                public const string CD119 = "Cookies";
                public const string CD149 = "InstallerApplicationDetail";
                public const string CD150 = "InstallerManageAccount";
                public const string CD151 = "InstallerApplicationNewStart";
                public const string CD152 = "InstallerApplicationRedeem";
                public const string CD153 = "InstallerHelp";
                public const string CD154a = "InstallerApplicationCancel";
                public const string CD154v = "InstallerApplicationVoucherCancel";
                public const string CD155a = "InstallerApplications";
                public const string CD155n = "InstallerNoApplications";
                public const string CD157 = "BiomassSpec";
                public const string CD158 = "GasGrid";
                public const string CD159 = "Cancel";
                public const string CD161 = "Social";
                public const string CD162 = "EPC";
                public const string CD164 = "EPCRecommendations";
                public const string CD165 = "EPCRecommendationsExempt";
                public const string CD167 = "FuelType";
                public const string CD168 = "PropertyOwnerContactDetails";
                public const string CD169 = "PropertyOwnerWelsh";
                public const string CD171 = "CheckYourAnswers";
                public const string CD172 = "PropertyOwnerPostcode";
                public const string CD173 = "PropertyOwnerAddressSelect";
                public const string CD174 = "PropertyOwnerAddressForm";
                public const string CD175 = "Feedback";
                public const string CD179 = "TechShared";
                public const string CD180 = "InstallAddressNotFound";
                public const string CD187 = "ThereIsAProblem";
                public const string CD188 = "EligibleBuildHalt";
                public const string CD189 = "EPCEvidence";
            }

            public static class Path
            {
                public const string CD003 = $"/ApplicationCreateNew/TechType/{Name.CD003}";
                public const string CD004 = $"/ApplicationCreateNew/DropOut/{Name.CD004}";
                public const string CD005 = $"/ApplicationCreateNew/InstallAddress/{Name.CD005}";
                public const string CD009 = $"/ApplicationCreateNew/NewBuild/{Name.CD009}";
                public const string CD010 = $"/ApplicationCreateNew/EligibleBuild/{Name.CD010}";
                public const string CD012 = $"/ApplicationCreateNew/TechType/{Name.CD012}";
                public const string CD019 = $"/ApplicationCreateNew/Property/{Name.CD019}";
                public const string CD020 = $"/ApplicationCreateNew/Property/{Name.CD020}";
                public const string CD022 = $"/ApplicationCreateNew/QuoteDetails/{Name.CD022}";
                public const string CD026 = $"/ApplicationCreateNew/Confirmation/{Name.CD026}";
                public const string CD029 = $"/ApplicationCreateNew/InstallAddress/{Name.CD029}";
                public const string CD106 = $"/ApplicationCreateNew/InstallAddress/{Name.CD106}";
                public const string CD119 = $"/Help/{Name.CD119}";
                public const string CD149 = $"/ApplicationDetail/{Name.CD149}";
                public const string CD150 = $"/ManageUserAccount/{Name.CD150}";
                public const string CD151 = $"/ApplicationCreateNew/{Name.CD151}";
                public const string CD152 = $"/ApplicationRedeemVoucher/{Name.CD152}";
                public const string CD153 = $"/ApplicationHelp/{Name.CD153}";
                public const string CD154a = $"/ApplicationCancel/{Name.CD154a}";
                public const string CD154v = $"/ApplicationCancel/{Name.CD154v}";
                public const string CD155a = $"/ApplicationsDashboard/{Name.CD155a}";
                public const string CD155n = $"/ApplicationsDashboard/{Name.CD155n}";
                public const string CD157 = $"/ApplicationCreateNew/TechType/{Name.CD157}";
                public const string CD158 = $"/ApplicationCreateNew/Gas/{Name.CD158}";
                public const string CD159 = $"/ApplicationCreateNew/Cancellation/{Name.CD159}";
                public const string CD161 = $"/ApplicationCreateNew/SocialHousing/{Name.CD161}";
                public const string CD162 = $"/ApplicationCreateNew/EPC/{Name.CD162}";
                public const string CD164 = $"/ApplicationCreateNew/EPC/{Name.CD164}";
                public const string CD165 = $"/ApplicationCreateNew/EPC/{Name.CD165}";
                public const string CD167 = $"/ApplicationCreateNew/FuelType/{Name.CD167}";
                public const string CD168 = $"/ApplicationCreateNew/Property/{Name.CD168}";
                public const string CD169 = $"/ApplicationCreateNew/Property/{Name.CD169}";
                public const string CD171 = $"/ApplicationCreateNew/CheckAnswers/{Name.CD171}";
                public const string CD172 = $"/ApplicationCreateNew/Property/{Name.CD172}";
                public const string CD173 = $"/ApplicationCreateNew/Property/{Name.CD173}";
                public const string CD174 = $"/ApplicationCreateNew/Property/{Name.CD174}";
                public const string CD175 = $"/ApplicationCreateNew/LeaveFeedback/{Name.CD175}";
                public const string CD179 = $"/ApplicationCreateNew/TechType/{Name.CD179}";
                public const string CD180 = $"/ApplicationCreateNew/InstallAddress/{Name.CD180}";
                public const string CD187 = $"/Shared/{Name.CD187}";
                public const string CD188 = $"/ApplicationCreateNew/EligibleBuild/{Name.CD188}";
                public const string CD189 = $"/ApplicationCreateNew/EPC/{Name.CD189}";
            }
        }
    }
}
